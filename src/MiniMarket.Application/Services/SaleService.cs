using AutoMapper;
using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.Ports.Output;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Application.Services;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SaleService(
        ISaleRepository saleRepository,
        IProductRepository productRepository,
        ICashRegisterRepository cashRegisterRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _cashRegisterRepository = cashRegisterRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<SaleDto>> GetByIdAsync(Guid id)
    {
        var sale = await _saleRepository.GetWithItemsAsync(id);
        if (sale == null)
            return Result<SaleDto>.Failure("Venta no encontrada");

        return Result<SaleDto>.Success(_mapper.Map<SaleDto>(sale));
    }

    public async Task<Result<SaleDto>> GetByNumberAsync(string saleNumber)
    {
        var sale = await _saleRepository.GetByNumberAsync(saleNumber);
        if (sale == null)
            return Result<SaleDto>.Failure("Venta no encontrada");

        return Result<SaleDto>.Success(_mapper.Map<SaleDto>(sale));
    }

    public async Task<Result<PagedResult<SaleDto>>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        var sales = await _saleRepository.GetAllAsync();
        var totalCount = sales.Count;

        var paged = sales
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = _mapper.Map<IEnumerable<SaleDto>>(paged);
        return Result<PagedResult<SaleDto>>.Success(
            new PagedResult<SaleDto>(dtos, totalCount, page, pageSize));
    }

    public async Task<Result<PagedResult<SaleDto>>> GetByDateRangeAsync(DateTime from, DateTime to, int page = 1, int pageSize = 20)
    {
        var sales = await _saleRepository.GetByDateRangeAsync(from.Date, to.Date.AddDays(1).AddTicks(-1));
        var totalCount = sales.Count;

        var paged = sales
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = _mapper.Map<IEnumerable<SaleDto>>(paged);
        return Result<PagedResult<SaleDto>>.Success(
            new PagedResult<SaleDto>(dtos, totalCount, page, pageSize));
    }

    public async Task<Result<IEnumerable<SaleDto>>> GetByCustomerAsync(Guid customerId)
    {
        var sales = await _saleRepository.GetByCustomerAsync(customerId);
        return Result<IEnumerable<SaleDto>>.Success(
            _mapper.Map<IEnumerable<SaleDto>>(sales));
    }

    public async Task<Result<IEnumerable<SaleDto>>> GetByCashRegisterAsync(Guid cashRegisterId)
    {
        var sales = await _saleRepository.GetByCashRegisterAsync(cashRegisterId);
        return Result<IEnumerable<SaleDto>>.Success(
            _mapper.Map<IEnumerable<SaleDto>>(sales));
    }

    public async Task<Result<IEnumerable<SaleDto>>> GetTodaySalesAsync()
    {
        var today = DateTime.Today;
        var sales = await _saleRepository.GetByDateRangeAsync(today, today.AddDays(1).AddTicks(-1));
        return Result<IEnumerable<SaleDto>>.Success(
            _mapper.Map<IEnumerable<SaleDto>>(sales));
    }

    public async Task<Result<SaleDto>> CreateAsync(CreateSaleDto dto, Guid userId)
    {
        try
        {
            var cashRegister = await _cashRegisterRepository.GetOpenByUserAsync(userId);
            if (cashRegister == null)
                return Result<SaleDto>.Failure("Debe abrir una caja antes de realizar ventas");

            Customer? customer = null;
            if (dto.CustomerId.HasValue)
            {
                customer = await _customerRepository.GetByIdAsync(dto.CustomerId.Value);
                if (customer == null)
                    return Result<SaleDto>.Failure("Cliente no encontrado");
            }

            if (dto.IsCredit && customer == null)
                return Result<SaleDto>.Failure("Las ventas al credito requieren un cliente");

            var saleNumber = await _saleRepository.GenerateNextNumberAsync();
            var sale = Sale.Create(
                saleNumber,
                cashRegister.Id,
                userId,
                dto.CustomerId,
                dto.IsCredit,
                dto.Notes);

            foreach (var itemDto in dto.Items)
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                    return Result<SaleDto>.Failure($"Producto no encontrado: {itemDto.ProductId}");

                var quantity = Quantity.Create(itemDto.Quantity);
                var unitPrice = itemDto.UnitPrice.HasValue
                    ? Money.Create(itemDto.UnitPrice.Value)
                    : null;
                var discount = itemDto.DiscountPercent.HasValue
                    ? Percentage.Create(itemDto.DiscountPercent.Value)
                    : null;

                sale.AddItem(product, quantity, unitPrice, discount);

                product.RemoveStock(quantity, MovementType.Sale, sale.Number, null);
                _productRepository.Update(product);
            }

            foreach (var paymentDto in dto.Payments)
            {
                var amount = Money.Create(paymentDto.Amount);
                sale.AddPayment(paymentDto.Method, amount, paymentDto.Reference);
            }

            if (dto.IsCredit && customer != null)
            {
                customer.AddDebt(sale.Total);
                _customerRepository.Update(customer);
            }

            sale.Complete();

            if (!dto.IsCredit)
            {
                var cashAmount = dto.Payments
                    .Where(p => p.Method == PaymentMethod.Cash)
                    .Sum(p => p.Amount);

                if (cashAmount > 0)
                {
                    cashRegister.RegisterSale(Money.Create(cashAmount), sale.Number);
                    _cashRegisterRepository.Update(cashRegister);
                }
            }

            await _saleRepository.AddAsync(sale);
            await _unitOfWork.SaveChangesAsync();

            return Result<SaleDto>.Success(_mapper.Map<SaleDto>(sale));
        }
        catch (DomainException ex)
        {
            return Result<SaleDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<SaleDto>> CancelAsync(Guid saleId, string reason)
    {
        var sale = await _saleRepository.GetWithItemsAsync(saleId);
        if (sale == null)
            return Result<SaleDto>.Failure("Venta no encontrada");

        try
        {
            foreach (var item in sale.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.AddStock(item.Quantity, MovementType.Return, sale.Number, "Venta cancelada");
                    _productRepository.Update(product);
                }
            }

            sale.Cancel(reason);
            _saleRepository.Update(sale);
            await _unitOfWork.SaveChangesAsync();

            return Result<SaleDto>.Success(_mapper.Map<SaleDto>(sale));
        }
        catch (DomainException ex)
        {
            return Result<SaleDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<SaleSummaryDto>> GetDailySummaryAsync(DateTime date)
    {
        var sales = await _saleRepository.GetByDateRangeAsync(date.Date, date.Date.AddDays(1).AddTicks(-1));
        var completedSales = sales.Where(s => s.Status == SaleStatus.Completed).ToList();

        var summary = new SaleSummaryDto(
            TotalSales: completedSales.Count,
            TotalAmount: completedSales.Sum(s => s.Total.Amount),
            TotalCash: completedSales
                .SelectMany(s => s.Payments)
                .Where(p => p.Method == PaymentMethod.Cash)
                .Sum(p => p.Amount.Amount),
            TotalCard: completedSales
                .SelectMany(s => s.Payments)
                .Where(p => p.Method == PaymentMethod.Card)
                .Sum(p => p.Amount.Amount),
            TotalOther: completedSales
                .SelectMany(s => s.Payments)
                .Where(p => p.Method != PaymentMethod.Cash && p.Method != PaymentMethod.Card && p.Method != PaymentMethod.Credit)
                .Sum(p => p.Amount.Amount),
            TotalCredit: completedSales
                .Where(s => s.IsCredit)
                .Sum(s => s.Total.Amount));

        return Result<SaleSummaryDto>.Success(summary);
    }

    public async Task<Result<decimal>> GetDailySalesTotal(DateTime date)
    {
        var sales = await _saleRepository.GetByDateRangeAsync(date.Date, date.Date.AddDays(1).AddTicks(-1));
        var total = sales
            .Where(s => s.Status == SaleStatus.Completed)
            .Sum(s => s.Total.Amount);

        return Result<decimal>.Success(total);
    }
}
