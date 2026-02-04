using FluentValidation;
using MiniMarket.Application.DTOs;

namespace MiniMarket.Application.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es requerido")
            .MaximumLength(50).WithMessage("El código no puede exceder 50 caracteres");

        RuleFor(x => x.Barcode)
            .Matches(@"^\d{8,14}$").WithMessage("El código de barras debe tener entre 8 y 14 dígitos")
            .When(x => !string.IsNullOrWhiteSpace(x.Barcode));

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres")
            .When(x => x.Description is not null);

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("La categoría es requerida");

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de compra no puede ser negativo");

        RuleFor(x => x.SalePrice)
            .GreaterThan(0).WithMessage("El precio de venta debe ser mayor a cero")
            .GreaterThanOrEqualTo(x => x.PurchasePrice)
            .WithMessage("El precio de venta no puede ser menor al precio de compra");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("La unidad es requerida")
            .MaximumLength(20).WithMessage("La unidad no puede exceder 20 caracteres");

        RuleFor(x => x.MinStock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo");
    }
}

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        RuleFor(x => x.Barcode)
            .Matches(@"^\d{8,14}$").WithMessage("El código de barras debe tener entre 8 y 14 dígitos")
            .When(x => !string.IsNullOrWhiteSpace(x.Barcode));

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres")
            .When(x => x.Description is not null);

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("La categoría es requerida");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("La unidad es requerida")
            .MaximumLength(20).WithMessage("La unidad no puede exceder 20 caracteres");

        RuleFor(x => x.MinStock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo");
    }
}

public class UpdateProductPricesDtoValidator : AbstractValidator<UpdateProductPricesDto>
{
    public UpdateProductPricesDtoValidator()
    {
        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de compra no puede ser negativo");

        RuleFor(x => x.SalePrice)
            .GreaterThan(0).WithMessage("El precio de venta debe ser mayor a cero")
            .GreaterThanOrEqualTo(x => x.PurchasePrice)
            .WithMessage("El precio de venta no puede ser menor al precio de compra");
    }
}
