using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Application.CustomerVisit.Commands.CreateCustomerVisit;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.CustomerVisit
{
    public class CreateCustomerVisitCommandValidator : AbstractValidator<CreateCustomerVisitCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICustomerVisitQueryRepository _queryRepository;

        public CreateCustomerVisitCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ICustomerVisitQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.CustomerVisit>("Remarks") ?? 500;
            var maxLengthImageName = maxLengthProvider.GetMaxLength<Domain.Entities.CustomerVisit>("ImageName") ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.CustomerId)
                            .NotNull()
                            .WithMessage($"{nameof(CreateCustomerVisitCommand.CustomerId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCustomerVisitCommand.CustomerId)} {rule.Error}");

                        RuleFor(x => x.VisitTypeId)
                            .NotNull()
                            .WithMessage($"{nameof(CreateCustomerVisitCommand.VisitTypeId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCustomerVisitCommand.VisitTypeId)} {rule.Error}");

                        RuleFor(x => x.VisitDateTime)
                            .NotNull()
                            .WithMessage($"{nameof(CreateCustomerVisitCommand.VisitDateTime)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCustomerVisitCommand.VisitDateTime)} {rule.Error}");

                        RuleFor(x => x.MarketingOfficerId)
                            .NotNull()
                            .WithMessage($"{nameof(CreateCustomerVisitCommand.MarketingOfficerId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCustomerVisitCommand.MarketingOfficerId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(CreateCustomerVisitCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));

                        RuleFor(x => x.ImageName)
                            .MaximumLength(maxLengthImageName)
                            .WithMessage($"{nameof(CreateCustomerVisitCommand.ImageName)} {rule.Error} {maxLengthImageName} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ImageName));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.CustomerId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.CustomerExistsAsync(id))
                            .WithMessage($"{nameof(CreateCustomerVisitCommand.CustomerId)} {rule.Error}")
                            .When(x => x.CustomerId > 0);

                        RuleFor(x => x.VisitTypeId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.VisitTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateCustomerVisitCommand.VisitTypeId)} {rule.Error}")
                            .When(x => x.VisitTypeId > 0);

                        RuleFor(x => x.MarketingOfficerId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepository.MarketingOfficerExistsAsync(id))
                            .WithMessage($"{nameof(CreateCustomerVisitCommand.MarketingOfficerId)} {rule.Error}")
                            .When(x => x.MarketingOfficerId > 0);
                        break;

                    case "Latitude":
                        RuleFor(x => x.Latitude)
                            .InclusiveBetween(-90m, 90m)
                            .WithMessage($"{nameof(CreateCustomerVisitCommand.Latitude)} {rule.Error}")
                            .When(x => x.Latitude.HasValue);
                        break;

                    case "Longitude":
                        RuleFor(x => x.Longitude)
                            .InclusiveBetween(-180m, 180m)
                            .WithMessage($"{nameof(CreateCustomerVisitCommand.Longitude)} {rule.Error}")
                            .When(x => x.Longitude.HasValue);
                        break;

                    case "GreaterThan":
                        // Validate detail-level ItemId
                        RuleForEach(x => x.Products).ChildRules(product =>
                        {
                            product.RuleFor(p => p.ItemId)
                                .GreaterThan(0)
                                .WithMessage($"ItemId {rule.Error}");
                        }).When(x => x.Products != null && x.Products.Any());
                        break;

                    default:
                        break;
                }
            }

            // Custom: validate each product ItemId exists via lookup
            RuleForEach(x => x.Products).ChildRules(product =>
            {
                product.RuleFor(p => p.ItemId)
                    .MustAsync(async (id, ct) => await _queryRepository.ItemExistsAsync(id))
                    .WithMessage("ItemId is inactive/deleted.")
                    .When(p => p.ItemId > 0);
            }).When(x => x.Products != null && x.Products.Any());

            // Custom: no duplicate ItemIds
            RuleFor(x => x.Products)
                .Must(products =>
                {
                    if (products == null) return true;
                    var itemIds = products.Select(p => p.ItemId).ToList();
                    return itemIds.Count == itemIds.Distinct().Count();
                })
                .WithMessage("Duplicate product selection is not allowed.")
                .When(x => x.Products != null && x.Products.Any());
        }
    }
}
