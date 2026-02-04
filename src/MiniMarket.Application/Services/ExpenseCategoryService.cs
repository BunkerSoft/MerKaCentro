using AutoMapper;
using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.Ports.Output;

namespace MiniMarket.Application.Services;

public class ExpenseCategoryService : IExpenseCategoryService
{
    private readonly IExpenseCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ExpenseCategoryService(
        IExpenseCategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ExpenseCategoryDto>> GetByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return Result<ExpenseCategoryDto>.Failure("Categoria de gasto no encontrada");
        }

        return Result<ExpenseCategoryDto>.Success(_mapper.Map<ExpenseCategoryDto>(category));
    }

    public async Task<Result<IEnumerable<ExpenseCategoryDto>>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return Result<IEnumerable<ExpenseCategoryDto>>.Success(
            _mapper.Map<IEnumerable<ExpenseCategoryDto>>(categories));
    }

    public async Task<Result<IEnumerable<ExpenseCategoryDto>>> GetActiveAsync()
    {
        var categories = await _categoryRepository.GetActiveAsync();
        return Result<IEnumerable<ExpenseCategoryDto>>.Success(
            _mapper.Map<IEnumerable<ExpenseCategoryDto>>(categories));
    }

    public async Task<Result<ExpenseCategoryDto>> CreateAsync(CreateExpenseCategoryDto dto)
    {
        try
        {
            var exists = await _categoryRepository.NameExistsAsync(dto.Name);
            if (exists)
            {
                return Result<ExpenseCategoryDto>.Failure("Ya existe una categoria con ese nombre");
            }

            var category = ExpenseCategory.Create(dto.Name, dto.Description);
            await _categoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return Result<ExpenseCategoryDto>.Success(_mapper.Map<ExpenseCategoryDto>(category));
        }
        catch (DomainException ex)
        {
            return Result<ExpenseCategoryDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<ExpenseCategoryDto>> UpdateAsync(Guid id, UpdateExpenseCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return Result<ExpenseCategoryDto>.Failure("Categoria de gasto no encontrada");
        }

        try
        {
            var exists = await _categoryRepository.NameExistsAsync(dto.Name, id);
            if (exists)
            {
                return Result<ExpenseCategoryDto>.Failure("Ya existe otra categoria con ese nombre");
            }

            category.Update(dto.Name, dto.Description);
            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();

            return Result<ExpenseCategoryDto>.Success(_mapper.Map<ExpenseCategoryDto>(category));
        }
        catch (DomainException ex)
        {
            return Result<ExpenseCategoryDto>.Failure(ex.Message);
        }
    }

    public async Task<Result> ActivateAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return Result.Failure("Categoria de gasto no encontrada");
        }

        category.Activate();
        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeactivateAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return Result.Failure("Categoria de gasto no encontrada");
        }

        category.Deactivate();
        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return Result.Failure("Categoria de gasto no encontrada");
        }

        var hasExpenses = await _categoryRepository.HasExpensesAsync(id);
        if (hasExpenses)
        {
            return Result.Failure("No se puede eliminar una categoria con gastos asociados");
        }

        _categoryRepository.Remove(category);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
