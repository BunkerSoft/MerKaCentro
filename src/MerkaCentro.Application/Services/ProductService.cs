using AutoMapper;
using MerkaCentro.Application.Common;
using MerkaCentro.Application.DTOs;
using MerkaCentro.Domain.Entities;
using MerkaCentro.Domain.Enums;
using MerkaCentro.Domain.Exceptions;
using MerkaCentro.Domain.Ports.Output;
using MerkaCentro.Domain.ValueObjects;

namespace MerkaCentro.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return Result<ProductDto>.Failure("Producto no encontrado");

        return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
    }

    public async Task<Result<ProductDto>> GetByBarcodeAsync(string barcode)
    {
        var product = await _productRepository.GetByBarcodeAsync(barcode);
        if (product == null)
            return Result<ProductDto>.Failure("Producto no encontrado");

        return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
    }

    public async Task<Result<PagedResult<ProductDto>>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        var products = await _productRepository.GetAllAsync();
        var totalCount = products.Count;

        var pagedProducts = products
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = _mapper.Map<IEnumerable<ProductDto>>(pagedProducts);
        var result = new PagedResult<ProductDto>(dtos, totalCount, page, pageSize);

        return Result<PagedResult<ProductDto>>.Success(result);
    }

    public async Task<Result<PagedResult<ProductDto>>> SearchAsync(string searchTerm, int page = 1, int pageSize = 20)
    {
        var products = await _productRepository.SearchAsync(searchTerm);
        var totalCount = products.Count;

        var pagedProducts = products
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = _mapper.Map<IEnumerable<ProductDto>>(pagedProducts);
        var result = new PagedResult<ProductDto>(dtos, totalCount, page, pageSize);

        return Result<PagedResult<ProductDto>>.Success(result);
    }

    public async Task<Result<IEnumerable<ProductDto>>> GetByCategoryAsync(Guid categoryId)
    {
        var products = await _productRepository.GetByCategoryAsync(categoryId);
        return Result<IEnumerable<ProductDto>>.Success(
            _mapper.Map<IEnumerable<ProductDto>>(products));
    }

    public async Task<Result<IEnumerable<ProductDto>>> GetLowStockAsync()
    {
        var products = await _productRepository.GetLowStockAsync();
        return Result<IEnumerable<ProductDto>>.Success(
            _mapper.Map<IEnumerable<ProductDto>>(products));
    }

    public async Task<Result<IEnumerable<ProductDto>>> GetActiveAsync()
    {
        var products = await _productRepository.GetAllAsync();
        var activeProducts = products.Where(p => p.Status == ProductStatus.Active).ToList();
        return Result<IEnumerable<ProductDto>>.Success(
            _mapper.Map<IEnumerable<ProductDto>>(activeProducts));
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductDto dto)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
            if (category == null)
                return Result<ProductDto>.Failure("Categoría no encontrada");

            if (!string.IsNullOrWhiteSpace(dto.Barcode))
            {
                var existingByBarcode = await _productRepository.GetByBarcodeAsync(dto.Barcode);
                if (existingByBarcode != null)
                    return Result<ProductDto>.Failure("Ya existe un producto con este código de barras");
            }

            var barcode = string.IsNullOrWhiteSpace(dto.Barcode) ? null : Barcode.Create(dto.Barcode);
            var purchasePrice = Money.Create(dto.PurchasePrice, dto.Currency);
            var salePrice = Money.Create(dto.SalePrice, dto.Currency);
            var minStock = Quantity.Create(dto.MinStock);
            var initialStock = Quantity.Create(dto.CurrentStock);

            var productCode = await GenerateProductCodeAsync();

            var product = Product.Create(
                code: productCode,
                name: dto.Name,
                categoryId: dto.CategoryId,
                purchasePrice: purchasePrice,
                salePrice: salePrice,
                unit: dto.UnitOfMeasure,
                barcode: barcode,
                description: dto.Description,
                minStock: minStock);

            // NO agregar stock aquí - hacerlo después de SaveChanges
            // para evitar problemas con EF Core y Owned Entities múltiples

            await _productRepository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            // Ahora agregar el stock inicial si existe
            if (initialStock.Value > 0)
            {
                product.AddStock(initialStock, MovementType.Purchase, null, "Stock inicial");
                _productRepository.Update(product);
                await _unitOfWork.SaveChangesAsync();
            }

            return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
        }
        catch (DomainException ex)
        {
            return Result<ProductDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<ProductDto>.Failure($"Error al crear producto: {ex.Message}");
        }
    }

    public async Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return Result<ProductDto>.Failure("Producto no encontrado");

        try
        {
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
            if (category == null)
                return Result<ProductDto>.Failure("Categoría no encontrada");

            if (!string.IsNullOrWhiteSpace(dto.Barcode))
            {
                var existingByBarcode = await _productRepository.GetByBarcodeAsync(dto.Barcode);
                if (existingByBarcode != null && existingByBarcode.Id != id)
                    return Result<ProductDto>.Failure("Ya existe un producto con este código de barras");
            }

            var barcode = string.IsNullOrWhiteSpace(dto.Barcode) ? null : Barcode.Create(dto.Barcode);
            var purchasePrice = Money.Create(dto.PurchasePrice, product.PurchasePrice.Currency);
            var salePrice = Money.Create(dto.SalePrice, product.SalePrice.Currency);
            var minStock = Quantity.Create(dto.MinStock);

            product.Update(
                name: dto.Name,
                categoryId: dto.CategoryId,
                unit: dto.UnitOfMeasure,
                barcode: barcode,
                description: dto.Description,
                minStock: minStock,
                allowFractions: product.AllowFractions);

            // Solo actualizar precios si son diferentes
            if (purchasePrice != product.PurchasePrice || salePrice != product.SalePrice)
            {
                product.UpdatePrices(purchasePrice, salePrice);
            }

            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();

            return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
        }
        catch (DomainException ex)
        {
            return Result<ProductDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<ProductDto>.Failure($"Error al actualizar producto: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return Result.Failure("Producto no encontrado");

        var hasSales = await _productRepository.HasSalesAsync(id);
        if (hasSales)
            return Result.Failure("No se puede eliminar un producto con ventas asociadas. Desactívelo en su lugar.");

        _productRepository.Remove(product);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdateStockAsync(Guid id, decimal quantity, string reason)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return Result.Failure("Producto no encontrado");

        try
        {
            var qty = Quantity.Create(Math.Abs(quantity));

            if (quantity > 0)
            {
                product.AddStock(qty, MovementType.Adjustment, null, reason);
            }
            else if (quantity < 0)
            {
                product.RemoveStock(qty, MovementType.Adjustment, null, reason);
            }

            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> UpdatePriceAsync(Guid id, decimal newPrice, string? reason = null)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return Result.Failure("Producto no encontrado");

        try
        {
            var newSalePrice = Money.Create(newPrice, product.SalePrice.Currency);
            product.UpdatePrices(product.PurchasePrice, newSalePrice);

            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task<string> GenerateProductCodeAsync()
    {
        var count = await _productRepository.CountAsync();
        return $"PROD{(count + 1):D6}";
    }
}
