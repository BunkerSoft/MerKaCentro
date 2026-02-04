using AutoMapper;
using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.Ports.Output;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseCategoryRepository _categoryRepository;
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ExpenseService(
        IExpenseRepository expenseRepository,
        IExpenseCategoryRepository categoryRepository,
        ICashRegisterRepository cashRegisterRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
        _cashRegisterRepository = cashRegisterRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ExpenseDto>> GetByIdAsync(Guid id)
    {
        var expense = await _expenseRepository.GetByIdAsync(id);
        if (expense == null)
        {
            return Result<ExpenseDto>.Failure("Gasto no encontrado");
        }

        return Result<ExpenseDto>.Success(_mapper.Map<ExpenseDto>(expense));
    }

    public async Task<Result<PagedResult<ExpenseDto>>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        var expenses = await _expenseRepository.GetAllAsync();
        var totalCount = expenses.Count;

        var paged = expenses
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = _mapper.Map<IEnumerable<ExpenseDto>>(paged);
        return Result<PagedResult<ExpenseDto>>.Success(
            new PagedResult<ExpenseDto>(dtos, totalCount, page, pageSize));
    }

    public async Task<Result<IEnumerable<ExpenseDto>>> GetByCashRegisterAsync(Guid cashRegisterId)
    {
        var expenses = await _expenseRepository.GetByCashRegisterAsync(cashRegisterId);
        return Result<IEnumerable<ExpenseDto>>.Success(
            _mapper.Map<IEnumerable<ExpenseDto>>(expenses));
    }

    public async Task<Result<IEnumerable<ExpenseDto>>> GetByCategoryAsync(Guid categoryId)
    {
        var expenses = await _expenseRepository.GetByCategoryAsync(categoryId);
        return Result<IEnumerable<ExpenseDto>>.Success(
            _mapper.Map<IEnumerable<ExpenseDto>>(expenses));
    }

    public async Task<Result<IEnumerable<ExpenseDto>>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var expenses = await _expenseRepository.GetByDateRangeAsync(from, to);
        return Result<IEnumerable<ExpenseDto>>.Success(
            _mapper.Map<IEnumerable<ExpenseDto>>(expenses));
    }

    public async Task<Result<IEnumerable<ExpenseDto>>> GetTodayExpensesAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var expenses = await _expenseRepository.GetByDateRangeAsync(today, tomorrow);
        return Result<IEnumerable<ExpenseDto>>.Success(
            _mapper.Map<IEnumerable<ExpenseDto>>(expenses));
    }

    public async Task<Result<ExpenseDto>> CreateAsync(CreateExpenseDto dto, Guid userId)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                return Result<ExpenseDto>.Failure("Categoria de gasto no encontrada");
            }

            if (!category.IsActive)
            {
                return Result<ExpenseDto>.Failure("La categoria de gasto no esta activa");
            }

            if (dto.CashRegisterId.HasValue)
            {
                var cashRegister = await _cashRegisterRepository.GetByIdAsync(dto.CashRegisterId.Value);
                if (cashRegister == null)
                {
                    return Result<ExpenseDto>.Failure("Caja no encontrada");
                }

                if (cashRegister.Status != Domain.Enums.CashRegisterStatus.Open)
                {
                    return Result<ExpenseDto>.Failure("La caja no esta abierta");
                }

                // Register expense in cash register
                cashRegister.RegisterExpense(Money.Create(dto.Amount), dto.Description);
                _cashRegisterRepository.Update(cashRegister);
            }

            var amount = Money.Create(dto.Amount);
            var expense = Expense.Create(
                userId,
                dto.CategoryId,
                dto.Description,
                amount,
                dto.CashRegisterId,
                dto.Reference,
                dto.Notes);

            await _expenseRepository.AddAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            return Result<ExpenseDto>.Success(_mapper.Map<ExpenseDto>(expense));
        }
        catch (DomainException ex)
        {
            return Result<ExpenseDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<ExpenseDto>> UpdateAsync(Guid id, UpdateExpenseDto dto)
    {
        var expense = await _expenseRepository.GetByIdAsync(id);
        if (expense == null)
        {
            return Result<ExpenseDto>.Failure("Gasto no encontrado");
        }

        try
        {
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                return Result<ExpenseDto>.Failure("Categoria de gasto no encontrada");
            }

            if (!category.IsActive)
            {
                return Result<ExpenseDto>.Failure("La categoria de gasto no esta activa");
            }

            var amount = Money.Create(dto.Amount);
            expense.Update(dto.CategoryId, dto.Description, amount, dto.Reference, dto.Notes);

            _expenseRepository.Update(expense);
            await _unitOfWork.SaveChangesAsync();

            return Result<ExpenseDto>.Success(_mapper.Map<ExpenseDto>(expense));
        }
        catch (DomainException ex)
        {
            return Result<ExpenseDto>.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var expense = await _expenseRepository.GetByIdAsync(id);
        if (expense == null)
        {
            return Result.Failure("Gasto no encontrado");
        }

        // Can only delete expenses not associated with a closed cash register
        if (expense.CashRegisterId.HasValue)
        {
            var cashRegister = await _cashRegisterRepository.GetByIdAsync(expense.CashRegisterId.Value);
            if (cashRegister != null && cashRegister.Status != Domain.Enums.CashRegisterStatus.Open)
            {
                return Result.Failure("No se puede eliminar un gasto de una caja cerrada");
            }
        }

        _expenseRepository.Remove(expense);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<decimal>> GetTotalByDateRangeAsync(DateTime from, DateTime to)
    {
        var expenses = await _expenseRepository.GetByDateRangeAsync(from, to);
        var total = expenses.Sum(e => e.Amount.Amount);
        return Result<decimal>.Success(total);
    }

    public async Task<Result<decimal>> GetTotalByCategoryAsync(Guid categoryId, DateTime from, DateTime to)
    {
        var expenses = await _expenseRepository.GetByDateRangeAsync(from, to);
        var total = expenses.Where(e => e.CategoryId == categoryId).Sum(e => e.Amount.Amount);
        return Result<decimal>.Success(total);
    }
}
