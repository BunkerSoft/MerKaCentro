using FluentValidation;
using MiniMarket.Application.DTOs;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Application.Validators;

public class CreateCustomerDtoValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.DocumentNumber)
            .Must(BeValidDocument).WithMessage("El número de documento no es válido")
            .When(x => !string.IsNullOrWhiteSpace(x.DocumentNumber) && x.DocumentType.HasValue);

        RuleFor(x => x.Phone)
            .Matches(@"^[\d\s\-\+\(\)]{7,15}$").WithMessage("El teléfono no es válido")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El correo electrónico no es válido")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("El límite de crédito no puede ser negativo");
    }

    private bool BeValidDocument(CreateCustomerDto dto, string? documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber) || !dto.DocumentType.HasValue)
        {
            return true;
        }

        return dto.DocumentType.Value switch
        {
            DocumentType.DNI => documentNumber.Length == 8 && documentNumber.All(char.IsDigit),
            DocumentType.RUC => documentNumber.Length == 11 && documentNumber.All(char.IsDigit) && (documentNumber.StartsWith("10") || documentNumber.StartsWith("20")),
            DocumentType.CE => documentNumber.Length is >= 9 and <= 12,
            _ => false
        };
    }
}

public class UpdateCustomerDtoValidator : AbstractValidator<UpdateCustomerDto>
{
    public UpdateCustomerDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Phone)
            .Matches(@"^[\d\s\-\+\(\)]{7,15}$").WithMessage("El teléfono no es válido")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El correo electrónico no es válido")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
