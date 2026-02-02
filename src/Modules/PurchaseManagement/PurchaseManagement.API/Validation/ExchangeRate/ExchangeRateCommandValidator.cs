using PurchaseManagement.Application.ExchangeRate.Commands;
using FluentValidation;

namespace PurchaseManagement.API.Validation.ExchangeRate;

public sealed class ExchangeRateCommandValidator : AbstractValidator<ExchangeRateCommand>
{
    public ExchangeRateCommandValidator()
    {
        RuleFor(x => x.BaseCurrency)
            .NotEmpty().WithMessage("Base currency is required.")
            .Matches("^[A-Z]{3}$").WithMessage("Base currency must be ISO-4217 (3 uppercase letters).");

        RuleFor(x => x.Symbols)
            .NotEmpty().WithMessage("At least one quote currency is required.");

        RuleForEach(x => x.Symbols)
            .NotEmpty()
            .Matches("^[A-Z]{3}$").WithMessage("Quote currency must be ISO-4217 (3 uppercase letters).")
            .Must((cmd, quote) => !string.Equals(quote, cmd.BaseCurrency, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Quote currency cannot match base currency.");
    }
}
