using PurchaseManagement.Application.ExchangeRate.Queries.GetLatestRate;
using FluentValidation;

namespace PurchaseManagement.API.Validation.ExchangeRate;

public sealed class GetLatestRateQueryValidator : AbstractValidator<GetLatestRateQuery>
{
    public GetLatestRateQueryValidator()
    {
        RuleFor(x => x.BaseCurrency)
            .NotEmpty().Matches("^[A-Z]{3}$");
        RuleFor(x => x.QuoteCurrency)
            .NotEmpty().Matches("^[A-Z]{3}$")
            .NotEqual(x => x.BaseCurrency);
    }
}
