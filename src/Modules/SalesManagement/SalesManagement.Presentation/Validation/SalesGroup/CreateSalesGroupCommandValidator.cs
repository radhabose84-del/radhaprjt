using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Application.SalesGroup.Commands.CreateSalesGroup;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesGroup
{
    public class CreateSalesGroupCommandValidator : AbstractValidator<CreateSalesGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesGroupQueryRepository _queryRepository;

        public CreateSalesGroupCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesGroupQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthName    = maxLengthProvider.GetMaxLength<Domain.Entities.SalesGroup>("SalesGroupName")      ?? 100;
            var maxLengthManager = maxLengthProvider.GetMaxLength<Domain.Entities.SalesGroup>("ResponsibleManager")  ?? 100;
            var maxLengthRegion  = maxLengthProvider.GetMaxLength<Domain.Entities.SalesGroup>("RegionTerritory")     ?? 100;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.SalesGroupName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesGroupCommand.SalesGroupName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesGroupCommand.SalesGroupName)} {rule.Error}");

                        RuleFor(x => x.SalesOfficeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateSalesGroupCommand.SalesOfficeId)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.SalesGroupName)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateSalesGroupCommand.SalesGroupName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SalesGroupName));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SalesGroupName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateSalesGroupCommand.SalesGroupName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.ResponsibleManager)
                            .MaximumLength(maxLengthManager)
                            .WithMessage($"{nameof(CreateSalesGroupCommand.ResponsibleManager)} {rule.Error} {maxLengthManager} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ResponsibleManager));

                        RuleFor(x => x.RegionTerritory)
                            .MaximumLength(maxLengthRegion)
                            .WithMessage($"{nameof(CreateSalesGroupCommand.RegionTerritory)} {rule.Error} {maxLengthRegion} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.RegionTerritory));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.SalesOfficeId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesOfficeExistsAsync(id))
                            .WithMessage($"{nameof(CreateSalesGroupCommand.SalesOfficeId)} {rule.Error}")
                            .When(x => x.SalesOfficeId > 0);

                        RuleFor(x => x.ProductCategoryId)
                            .MustAsync(async (id, ct) => await _queryRepository.ProductCategoryExistsAsync(id!.Value, ct))
                            .WithMessage($"{nameof(CreateSalesGroupCommand.ProductCategoryId)} {rule.Error}")
                            .When(x => x.ProductCategoryId.HasValue && x.ProductCategoryId.Value > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                !await _queryRepository.AlreadyExistsAsync(cmd.SalesGroupName, cmd.SalesOfficeId))
                            .WithMessage($"{nameof(CreateSalesGroupCommand.SalesGroupName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SalesGroupName) && x.SalesOfficeId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
