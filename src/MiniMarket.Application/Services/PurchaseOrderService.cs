using AutoMapper;
using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.Ports.Output;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Application.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PurchaseOrderService(
        IPurchaseOrderRepository purchaseOrderRepository,
        ISupplierRepository supplierRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _supplierRepository = supplierRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PurchaseOrderDto>> GetByIdAsync(Guid id)
    {
        var order = await _purchaseOrderRepository.GetWithItemsAsync(id);
        if (order == null)
        {
            return Result<PurchaseOrderDto>.Failure("Orden de compra no encontrada");
        }

        return Result<PurchaseOrderDto>.Success(_mapper.Map<PurchaseOrderDto>(order));
    }

    public async Task<Result<PurchaseOrderDto>> GetByNumberAsync(string number)
    {
        var order = await _purchaseOrderRepository.GetByNumberAsync(number);
        if (order == null)
        {
            return Result<PurchaseOrderDto>.Failure("Orden de compra no encontrada");
        }

        return Result<PurchaseOrderDto>.Success(_mapper.Map<PurchaseOrderDto>(order));
    }

    public async Task<Result<PagedResult<PurchaseOrderDto>>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        var orders = await _purchaseOrderRepository.GetAllAsync();
        var totalCount = orders.Count;

        var paged = orders
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = _mapper.Map<IEnumerable<PurchaseOrderDto>>(paged);
        return Result<PagedResult<PurchaseOrderDto>>.Success(
            new PagedResult<PurchaseOrderDto>(dtos, totalCount, page, pageSize));
    }

    public async Task<Result<IEnumerable<PurchaseOrderDto>>> GetBySupplierAsync(Guid supplierId)
    {
        var orders = await _purchaseOrderRepository.GetBySupplierAsync(supplierId);
        return Result<IEnumerable<PurchaseOrderDto>>.Success(
            _mapper.Map<IEnumerable<PurchaseOrderDto>>(orders));
    }

    public async Task<Result<IEnumerable<PurchaseOrderDto>>> GetByStatusAsync(PurchaseOrderStatus status)
    {
        var orders = await _purchaseOrderRepository.GetByStatusAsync(status);
        return Result<IEnumerable<PurchaseOrderDto>>.Success(
            _mapper.Map<IEnumerable<PurchaseOrderDto>>(orders));
    }

    public async Task<Result<IEnumerable<PurchaseOrderDto>>> GetPendingAsync()
    {
        var pending = await _purchaseOrderRepository.GetByStatusAsync(PurchaseOrderStatus.Pending);
        var partial = await _purchaseOrderRepository.GetByStatusAsync(PurchaseOrderStatus.PartiallyReceived);

        var combined = pending.Concat(partial).OrderByDescending(o => o.CreatedAt);
        return Result<IEnumerable<PurchaseOrderDto>>.Success(
            _mapper.Map<IEnumerable<PurchaseOrderDto>>(combined));
    }

    public async Task<Result<PurchaseOrderDto>> CreateAsync(CreatePurchaseOrderDto dto, Guid userId)
    {
        try
        {
            var supplier = await _supplierRepository.GetByIdAsync(dto.SupplierId);
            if (supplier == null)
            {
                return Result<PurchaseOrderDto>.Failure("Proveedor no encontrado");
            }

            if (!supplier.IsActive)
            {
                return Result<PurchaseOrderDto>.Failure("El proveedor no esta activo");
            }

            var orderNumber = await _purchaseOrderRepository.GenerateNextNumberAsync();
            var order = PurchaseOrder.Create(orderNumber, dto.SupplierId, userId, dto.Notes);

            foreach (var itemDto in dto.Items)
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                {
                    return Result<PurchaseOrderDto>.Failure($"Producto no encontrado: {itemDto.ProductId}");
                }

                var quantity = Quantity.Create(itemDto.Quantity);
                var unitCost = Money.Create(itemDto.UnitCost);

                order.AddItem(product.Id, product.Name, quantity, unitCost);
            }

            await _purchaseOrderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return Result<PurchaseOrderDto>.Success(_mapper.Map<PurchaseOrderDto>(order));
        }
        catch (DomainException ex)
        {
            return Result<PurchaseOrderDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<PurchaseOrderDto>> ReceiveItemsAsync(Guid id, IEnumerable<ReceiveItemDto> items)
    {
        var order = await _purchaseOrderRepository.GetWithItemsAsync(id);
        if (order == null)
        {
            return Result<PurchaseOrderDto>.Failure("Orden de compra no encontrada");
        }

        try
        {
            foreach (var itemDto in items)
            {
                var quantity = Quantity.Create(itemDto.ReceivedQuantity);
                order.ReceiveItem(itemDto.ProductId, quantity);

                // Update product stock
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                if (product != null)
                {
                    product.AddStock(quantity, MovementType.Purchase, order.Number, "Recepcion de mercancia");
                    _productRepository.Update(product);
                }
            }

            _purchaseOrderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();

            return Result<PurchaseOrderDto>.Success(_mapper.Map<PurchaseOrderDto>(order));
        }
        catch (DomainException ex)
        {
            return Result<PurchaseOrderDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<PurchaseOrderDto>> MarkAsReceivedAsync(Guid id)
    {
        var order = await _purchaseOrderRepository.GetWithItemsAsync(id);
        if (order == null)
        {
            return Result<PurchaseOrderDto>.Failure("Orden de compra no encontrada");
        }

        try
        {
            // Add stock for all pending items
            foreach (var item in order.Items.Where(i => !i.IsFullyReceived()))
            {
                var pendingQuantity = item.GetPendingQuantity();
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.AddStock(pendingQuantity, MovementType.Purchase, order.Number, "Recepcion completa");
                    _productRepository.Update(product);
                }
            }

            order.MarkAsReceived();
            _purchaseOrderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();

            return Result<PurchaseOrderDto>.Success(_mapper.Map<PurchaseOrderDto>(order));
        }
        catch (DomainException ex)
        {
            return Result<PurchaseOrderDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<PurchaseOrderDto>> CancelAsync(Guid id, string? reason = null)
    {
        var order = await _purchaseOrderRepository.GetWithItemsAsync(id);
        if (order == null)
        {
            return Result<PurchaseOrderDto>.Failure("Orden de compra no encontrada");
        }

        try
        {
            order.Cancel(reason);
            _purchaseOrderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();

            return Result<PurchaseOrderDto>.Success(_mapper.Map<PurchaseOrderDto>(order));
        }
        catch (DomainException ex)
        {
            return Result<PurchaseOrderDto>.Failure(ex.Message);
        }
    }
}
