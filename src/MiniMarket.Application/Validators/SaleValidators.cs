using FluentValidation;
using MiniMarket.Application.DTOs;

namespace MiniMarket.Application.Validators;

public class CreateSaleDtoValidator : AbstractValidator<CreateSaleDto>
{
    public CreateSaleDtoValidator()
    {
        RuleFor(x => x.CashRegisterId)
            .NotEmpty().WithMessage("La caja registradora es requerida");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Debe incluir al menos un item")
            .Must(items => items.All(i => i.Quantity > 0))
            .WithMessage("Todas las cantidades deben ser mayores a cero");

        RuleForEach(x => x.Items).SetValidator(new CreateSaleItemDtoValidator());

        RuleFor(x => x.Payments)
            .NotEmpty().WithMessage("Debe incluir al menos un método de pago")
            .When(x => !x.IsCredit);

        RuleForEach(x => x.Payments).SetValidator(new CreateSalePaymentDtoValidator());

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("El cliente es requerido para ventas a crédito")
            .When(x => x.IsCredit);
    }
}

public class CreateSaleItemDtoValidator : AbstractValidator<CreateSaleItemDto>
{
    public CreateSaleItemDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("El producto es requerido");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("El precio unitario debe ser mayor a cero")
            .When(x => x.UnitPrice.HasValue);

        RuleFor(x => x.DiscountPercent)
            .InclusiveBetween(0, 100).WithMessage("El descuento debe estar entre 0 y 100")
            .When(x => x.DiscountPercent.HasValue);
    }
}

public class CreateSalePaymentDtoValidator : AbstractValidator<CreateSalePaymentDto>
{
    public CreateSalePaymentDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero");
    }
}
