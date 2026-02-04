using AutoMapper;
using MiniMarket.Application.Common;
using MiniMarket.Application.DTOs;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.Ports.Output;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CustomerService(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CustomerDto>> GetByIdAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
            return Result<CustomerDto>.Failure("Cliente no encontrado");

        return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
    }

    public async Task<Result<CustomerDto>> GetByDocumentAsync(string documentNumber)
    {
        var customer = await _customerRepository.GetByDocumentAsync(documentNumber);
        if (customer == null)
            return Result<CustomerDto>.Failure("Cliente no encontrado");

        return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
    }

    public async Task<Result<PagedResult<CustomerDto>>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        var customers = await _customerRepository.GetAllAsync();
        var totalCount = customers.Count;

        var pagedCustomers = customers
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = _mapper.Map<IEnumerable<CustomerDto>>(pagedCustomers);
        return Result<PagedResult<CustomerDto>>.Success(
            new PagedResult<CustomerDto>(dtos, totalCount, page, pageSize));
    }

    public async Task<Result<IEnumerable<CustomerDto>>> GetActiveAsync()
    {
        var customers = await _customerRepository.GetByStatusAsync(CustomerStatus.Active);
        return Result<IEnumerable<CustomerDto>>.Success(
            _mapper.Map<IEnumerable<CustomerDto>>(customers));
    }

    public async Task<Result<IEnumerable<CustomerDebtDto>>> GetWithDebtAsync()
    {
        var customers = await _customerRepository.GetWithDebtAsync();
        return Result<IEnumerable<CustomerDebtDto>>.Success(
            _mapper.Map<IEnumerable<CustomerDebtDto>>(customers));
    }

    public async Task<Result<IEnumerable<CustomerDto>>> SearchAsync(string searchTerm)
    {
        var customers = await _customerRepository.SearchAsync(searchTerm);
        return Result<IEnumerable<CustomerDto>>.Success(
            _mapper.Map<IEnumerable<CustomerDto>>(customers));
    }

    public async Task<Result<CustomerDto>> CreateAsync(CreateCustomerDto dto)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(dto.DocumentNumber))
            {
                var exists = await _customerRepository.DocumentExistsAsync(dto.DocumentNumber);
                if (exists)
                    return Result<CustomerDto>.Failure("Ya existe un cliente con ese número de documento");
            }

            var documentNumber = DocumentNumber.CreateOptional(dto.DocumentNumber, dto.DocumentType);
            var phone = PhoneNumber.CreateOptional(dto.Phone);
            var email = Email.CreateOptional(dto.Email);
            var address = Address.Create(dto.Street ?? "", dto.District, dto.City, dto.Reference);
            var creditLimit = Money.Create(dto.CreditLimit);

            var customer = Customer.Create(
                dto.Name,
                documentNumber,
                phone,
                email,
                address,
                creditLimit,
                dto.Notes);

            await _customerRepository.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
        }
        catch (DomainException ex)
        {
            return Result<CustomerDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerDto dto)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
            return Result<CustomerDto>.Failure("Cliente no encontrado");

        try
        {
            if (!string.IsNullOrWhiteSpace(dto.DocumentNumber))
            {
                var exists = await _customerRepository.DocumentExistsAsync(dto.DocumentNumber, id);
                if (exists)
                    return Result<CustomerDto>.Failure("Ya existe otro cliente con ese número de documento");
            }

            var documentNumber = DocumentNumber.CreateOptional(dto.DocumentNumber, dto.DocumentType);
            var phone = PhoneNumber.CreateOptional(dto.Phone);
            var email = Email.CreateOptional(dto.Email);
            var address = Address.Create(dto.Street ?? "", dto.District, dto.City, dto.Reference);

            customer.Update(dto.Name, documentNumber, phone, email, address, dto.Notes);
            _customerRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync();

            return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
        }
        catch (DomainException ex)
        {
            return Result<CustomerDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<CustomerDto>> SetCreditLimitAsync(Guid id, decimal creditLimit)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
            return Result<CustomerDto>.Failure("Cliente no encontrado");

        try
        {
            customer.SetCreditLimit(Money.Create(creditLimit));
            _customerRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync();

            return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
        }
        catch (DomainException ex)
        {
            return Result<CustomerDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<CustomerDto>> ActivateAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
            return Result<CustomerDto>.Failure("Cliente no encontrado");

        customer.Activate();
        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
    }

    public async Task<Result<CustomerDto>> DeactivateAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
            return Result<CustomerDto>.Failure("Cliente no encontrado");

        customer.Deactivate();
        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
    }

    public async Task<Result<CustomerDto>> BlockAsync(Guid id, string? reason = null)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
            return Result<CustomerDto>.Failure("Cliente no encontrado");

        customer.Block(reason);
        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
            return Result.Failure("Cliente no encontrado");

        if (customer.HasDebt())
            return Result.Failure("No se puede eliminar un cliente con deuda pendiente");

        _customerRepository.Remove(customer);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
