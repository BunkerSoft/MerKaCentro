using AutoMapper;
using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.Ports.Output;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SupplierService(
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<SupplierDto>> GetByIdAsync(Guid id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
        {
            return Result<SupplierDto>.Failure("Proveedor no encontrado");
        }

        return Result<SupplierDto>.Success(_mapper.Map<SupplierDto>(supplier));
    }

    public async Task<Result<SupplierDto>> GetByRucAsync(string ruc)
    {
        var supplier = await _supplierRepository.GetByRucAsync(ruc);
        if (supplier == null)
        {
            return Result<SupplierDto>.Failure("Proveedor no encontrado");
        }

        return Result<SupplierDto>.Success(_mapper.Map<SupplierDto>(supplier));
    }

    public async Task<Result<PagedResult<SupplierDto>>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        var suppliers = await _supplierRepository.GetAllAsync();
        var totalCount = suppliers.Count;

        var paged = suppliers
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = _mapper.Map<IEnumerable<SupplierDto>>(paged);
        return Result<PagedResult<SupplierDto>>.Success(
            new PagedResult<SupplierDto>(dtos, totalCount, page, pageSize));
    }

    public async Task<Result<IEnumerable<SupplierDto>>> GetActiveAsync()
    {
        var suppliers = await _supplierRepository.GetActiveAsync();
        return Result<IEnumerable<SupplierDto>>.Success(
            _mapper.Map<IEnumerable<SupplierDto>>(suppliers));
    }

    public async Task<Result<IEnumerable<SupplierDto>>> SearchAsync(string searchTerm)
    {
        var suppliers = await _supplierRepository.SearchAsync(searchTerm);
        return Result<IEnumerable<SupplierDto>>.Success(
            _mapper.Map<IEnumerable<SupplierDto>>(suppliers));
    }

    public async Task<Result<SupplierDto>> CreateAsync(CreateSupplierDto dto)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(dto.Ruc))
            {
                var exists = await _supplierRepository.RucExistsAsync(dto.Ruc);
                if (exists)
                {
                    return Result<SupplierDto>.Failure("Ya existe un proveedor con ese RUC");
                }
            }

            var ruc = !string.IsNullOrWhiteSpace(dto.Ruc)
                ? DocumentNumber.Create(dto.Ruc, DocumentType.RUC)
                : null;
            var phone = PhoneNumber.CreateOptional(dto.Phone);
            var email = Email.CreateOptional(dto.Email);
            var address = Address.Create(dto.Street ?? "", dto.District, dto.City, null);

            var supplier = Supplier.Create(
                dto.Name,
                dto.BusinessName,
                ruc,
                phone,
                email,
                address,
                dto.ContactPerson,
                dto.Notes);

            await _supplierRepository.AddAsync(supplier);
            await _unitOfWork.SaveChangesAsync();

            return Result<SupplierDto>.Success(_mapper.Map<SupplierDto>(supplier));
        }
        catch (DomainException ex)
        {
            return Result<SupplierDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<SupplierDto>> UpdateAsync(Guid id, UpdateSupplierDto dto)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
        {
            return Result<SupplierDto>.Failure("Proveedor no encontrado");
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(dto.Ruc))
            {
                var exists = await _supplierRepository.RucExistsAsync(dto.Ruc, id);
                if (exists)
                {
                    return Result<SupplierDto>.Failure("Ya existe otro proveedor con ese RUC");
                }
            }

            var ruc = !string.IsNullOrWhiteSpace(dto.Ruc)
                ? DocumentNumber.Create(dto.Ruc, DocumentType.RUC)
                : null;
            var phone = PhoneNumber.CreateOptional(dto.Phone);
            var email = Email.CreateOptional(dto.Email);
            var address = Address.Create(dto.Street ?? "", dto.District, dto.City, null);

            supplier.Update(
                dto.Name,
                dto.BusinessName,
                ruc,
                phone,
                email,
                address,
                dto.ContactPerson,
                dto.Notes);

            _supplierRepository.Update(supplier);
            await _unitOfWork.SaveChangesAsync();

            return Result<SupplierDto>.Success(_mapper.Map<SupplierDto>(supplier));
        }
        catch (DomainException ex)
        {
            return Result<SupplierDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<SupplierDto>> ActivateAsync(Guid id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
        {
            return Result<SupplierDto>.Failure("Proveedor no encontrado");
        }

        supplier.Activate();
        _supplierRepository.Update(supplier);
        await _unitOfWork.SaveChangesAsync();

        return Result<SupplierDto>.Success(_mapper.Map<SupplierDto>(supplier));
    }

    public async Task<Result<SupplierDto>> DeactivateAsync(Guid id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
        {
            return Result<SupplierDto>.Failure("Proveedor no encontrado");
        }

        supplier.Deactivate();
        _supplierRepository.Update(supplier);
        await _unitOfWork.SaveChangesAsync();

        return Result<SupplierDto>.Success(_mapper.Map<SupplierDto>(supplier));
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
        {
            return Result.Failure("Proveedor no encontrado");
        }

        if (supplier.PurchaseOrders.Count != 0)
        {
            return Result.Failure("No se puede eliminar un proveedor con ordenes de compra. Desactivelo en su lugar.");
        }

        _supplierRepository.Remove(supplier);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
