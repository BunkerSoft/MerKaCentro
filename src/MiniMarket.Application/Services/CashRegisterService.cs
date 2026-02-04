using AutoMapper;
using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.Ports.Output;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Application.Services;

public class CashRegisterService : ICashRegisterService
{
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CashRegisterService(
        ICashRegisterRepository cashRegisterRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _cashRegisterRepository = cashRegisterRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CashRegisterDto>> GetByIdAsync(Guid id)
    {
        var cashRegister = await _cashRegisterRepository.GetWithMovementsAsync(id);
        if (cashRegister == null)
            return Result<CashRegisterDto>.Failure("Caja no encontrada");

        return Result<CashRegisterDto>.Success(_mapper.Map<CashRegisterDto>(cashRegister));
    }

    public async Task<Result<CashRegisterDto>> GetCurrentOpenAsync(Guid userId)
    {
        var cashRegister = await _cashRegisterRepository.GetOpenByUserAsync(userId);
        if (cashRegister == null)
            return Result<CashRegisterDto>.Failure("No hay caja abierta para este usuario");

        return Result<CashRegisterDto>.Success(_mapper.Map<CashRegisterDto>(cashRegister));
    }

    public async Task<Result<PagedResult<CashRegisterDto>>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        var cashRegisters = await _cashRegisterRepository.GetAllAsync();
        var totalCount = cashRegisters.Count;

        var paged = cashRegisters
            .OrderByDescending(c => c.OpenedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = _mapper.Map<IEnumerable<CashRegisterDto>>(paged);
        return Result<PagedResult<CashRegisterDto>>.Success(
            new PagedResult<CashRegisterDto>(dtos, totalCount, page, pageSize));
    }

    public async Task<Result<IEnumerable<CashRegisterDto>>> GetByUserAsync(Guid userId)
    {
        var cashRegisters = await _cashRegisterRepository.GetByUserAsync(userId);
        return Result<IEnumerable<CashRegisterDto>>.Success(
            _mapper.Map<IEnumerable<CashRegisterDto>>(cashRegisters));
    }

    public async Task<Result<IEnumerable<CashRegisterDto>>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var cashRegisters = await _cashRegisterRepository.GetByDateRangeAsync(from, to);
        return Result<IEnumerable<CashRegisterDto>>.Success(
            _mapper.Map<IEnumerable<CashRegisterDto>>(cashRegisters));
    }

    public async Task<Result<CashRegisterDto>> OpenAsync(OpenCashRegisterDto dto)
    {
        try
        {
            var hasOpen = await _cashRegisterRepository.HasOpenRegisterAsync(dto.UserId);
            if (hasOpen)
                return Result<CashRegisterDto>.Failure("El usuario ya tiene una caja abierta");

            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
                return Result<CashRegisterDto>.Failure("Usuario no encontrado");

            var initialCash = Money.Create(dto.InitialCash, dto.Currency);
            var cashRegister = CashRegister.Open(dto.UserId, initialCash);

            await _cashRegisterRepository.AddAsync(cashRegister);
            await _unitOfWork.SaveChangesAsync();

            return Result<CashRegisterDto>.Success(_mapper.Map<CashRegisterDto>(cashRegister));
        }
        catch (DomainException ex)
        {
            return Result<CashRegisterDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<CashRegisterDto>> CloseAsync(Guid id, decimal countedCash)
    {
        var cashRegister = await _cashRegisterRepository.GetWithMovementsAsync(id);
        if (cashRegister == null)
            return Result<CashRegisterDto>.Failure("Caja no encontrada");

        try
        {
            var finalCash = Money.Create(countedCash, cashRegister.InitialCash.Currency);
            cashRegister.Close(finalCash);
            _cashRegisterRepository.Update(cashRegister);
            await _unitOfWork.SaveChangesAsync();

            return Result<CashRegisterDto>.Success(_mapper.Map<CashRegisterDto>(cashRegister));
        }
        catch (DomainException ex)
        {
            return Result<CashRegisterDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<CashRegisterDto>> RegisterSaleAsync(Guid id, decimal amount, string currency = "PEN")
    {
        var cashRegister = await _cashRegisterRepository.GetWithMovementsAsync(id);
        if (cashRegister == null)
            return Result<CashRegisterDto>.Failure("Caja no encontrada");

        try
        {
            var saleAmount = Money.Create(amount, currency);
            cashRegister.RegisterSale(saleAmount, $"Venta-{DateTime.Now:yyyyMMddHHmmss}");
            _cashRegisterRepository.Update(cashRegister);
            await _unitOfWork.SaveChangesAsync();

            return Result<CashRegisterDto>.Success(_mapper.Map<CashRegisterDto>(cashRegister));
        }
        catch (DomainException ex)
        {
            return Result<CashRegisterDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<CashRegisterDto>> RegisterWithdrawalAsync(Guid id, RegisterMovementDto dto)
    {
        var cashRegister = await _cashRegisterRepository.GetWithMovementsAsync(id);
        if (cashRegister == null)
            return Result<CashRegisterDto>.Failure("Caja no encontrada");

        try
        {
            var amount = Money.Create(dto.Amount, dto.Currency);
            cashRegister.RegisterWithdrawal(amount, dto.Description);
            _cashRegisterRepository.Update(cashRegister);
            await _unitOfWork.SaveChangesAsync();

            return Result<CashRegisterDto>.Success(_mapper.Map<CashRegisterDto>(cashRegister));
        }
        catch (DomainException ex)
        {
            return Result<CashRegisterDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<CashRegisterDto>> RegisterDepositAsync(Guid id, RegisterMovementDto dto)
    {
        var cashRegister = await _cashRegisterRepository.GetWithMovementsAsync(id);
        if (cashRegister == null)
            return Result<CashRegisterDto>.Failure("Caja no encontrada");

        try
        {
            var amount = Money.Create(dto.Amount, dto.Currency);
            cashRegister.RegisterDeposit(amount, dto.Description);
            _cashRegisterRepository.Update(cashRegister);
            await _unitOfWork.SaveChangesAsync();

            return Result<CashRegisterDto>.Success(_mapper.Map<CashRegisterDto>(cashRegister));
        }
        catch (DomainException ex)
        {
            return Result<CashRegisterDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<CashRegisterDto>> RegisterExpenseAsync(Guid id, RegisterExpenseDto dto)
    {
        var cashRegister = await _cashRegisterRepository.GetWithMovementsAsync(id);
        if (cashRegister == null)
            return Result<CashRegisterDto>.Failure("Caja no encontrada");

        try
        {
            var amount = Money.Create(dto.Amount, dto.Currency);
            cashRegister.RegisterExpense(amount, dto.Description);
            _cashRegisterRepository.Update(cashRegister);
            await _unitOfWork.SaveChangesAsync();

            return Result<CashRegisterDto>.Success(_mapper.Map<CashRegisterDto>(cashRegister));
        }
        catch (DomainException ex)
        {
            return Result<CashRegisterDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<CashRegisterFullSummaryDto>> GetSummaryAsync(Guid id)
    {
        var cashRegister = await _cashRegisterRepository.GetWithMovementsAsync(id);
        if (cashRegister == null)
            return Result<CashRegisterFullSummaryDto>.Failure("Caja no encontrada");

        var summary = new CashRegisterFullSummaryDto(
            Id: cashRegister.Id,
            InitialCash: cashRegister.InitialCash.Amount,
            TotalSales: cashRegister.GetTotalSales().Amount,
            TotalDeposits: cashRegister.GetTotalDeposits().Amount,
            TotalWithdrawals: cashRegister.GetTotalWithdrawals().Amount,
            TotalExpenses: cashRegister.GetTotalExpenses().Amount,
            ExpectedCash: cashRegister.ExpectedCash.Amount,
            CountedCash: cashRegister.FinalCash?.Amount,
            Difference: cashRegister.Difference?.Amount,
            TotalTransactions: cashRegister.Movements.Count,
            Currency: cashRegister.InitialCash.Currency);

        return Result<CashRegisterFullSummaryDto>.Success(summary);
    }
}
