using AutoMapper;
using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.Ports.Output;

namespace MiniMarket.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return Result<CategoryDto>.Failure("Categoría no encontrada");

        return Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category));
    }

    public async Task<Result<IEnumerable<CategoryDto>>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return Result<IEnumerable<CategoryDto>>.Success(
            _mapper.Map<IEnumerable<CategoryDto>>(categories));
    }

    public async Task<Result<IEnumerable<CategoryDto>>> GetActiveAsync()
    {
        var categories = await _categoryRepository.GetActiveAsync();
        return Result<IEnumerable<CategoryDto>>.Success(
            _mapper.Map<IEnumerable<CategoryDto>>(categories));
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto)
    {
        try
        {
            var category = Category.Create(dto.Name, dto.Description);
            await _categoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category));
        }
        catch (DomainException ex)
        {
            return Result<CategoryDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return Result<CategoryDto>.Failure("Categoría no encontrada");

        try
        {
            category.Update(dto.Name, dto.Description);
            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();

            return Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category));
        }
        catch (DomainException ex)
        {
            return Result<CategoryDto>.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return Result.Failure("Categoría no encontrada");

        var hasProducts = await _categoryRepository.HasProductsAsync(id);
        if (hasProducts)
            return Result.Failure("No se puede eliminar una categoría con productos asociados");

        _categoryRepository.Remove(category);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ActivateAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return Result.Failure("Categoría no encontrada");

        category.Activate();
        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeactivateAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return Result.Failure("Categoría no encontrada");

        category.Deactivate();
        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
