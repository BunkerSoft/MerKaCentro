using FluentValidation;
using MiniMarket.Application.DTOs;

namespace MiniMarket.Application.Validators;

public class OpenCashRegisterDtoValidator : AbstractValidator<OpenCashRegisterDto>
{
    public OpenCashRegisterDtoValidator()
    {
        RuleFor(x => x.InitialCash)
            .GreaterThanOrEqualTo(0).WithMessage("El monto inicial no puede ser negativo");
    }
}

public class CloseCashRegisterDtoValidator : AbstractValidator<CloseCashRegisterDto>
{
    public CloseCashRegisterDtoValidator()
    {
        RuleFor(x => x.FinalCash)
            .GreaterThanOrEqualTo(0).WithMessage("El monto final no puede ser negativo");
    }
}

public class CashWithdrawalDtoValidator : AbstractValidator<CashWithdrawalDto>
{
    public CashWithdrawalDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("El motivo es requerido")
            .MaximumLength(500).WithMessage("El motivo no puede exceder 500 caracteres");
    }
}

public class CashDepositDtoValidator : AbstractValidator<CashDepositDto>
{
    public CashDepositDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("El motivo es requerido")
            .MaximumLength(500).WithMessage("El motivo no puede exceder 500 caracteres");
    }
}
