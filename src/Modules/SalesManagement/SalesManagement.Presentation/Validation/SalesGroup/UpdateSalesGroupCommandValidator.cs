#nullable disable
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Application.SalesGroup.Commands.UpdateSalesGroup;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesGroup
{
    public class UpdateSalesGroupCommandValidator : AbstractValidator<UpdateSalesGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesGroupQueryRepository _queryRepository;

        public UpdateSalesGroupCommandValidator(
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
                            .WithMessage($"{nameof(UpdateSalesGroupCommand.SalesGroupName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateSalesGroupCommand.SalesGroupName)} {rule.Error}");

                        RuleFor(x => x.SalesOfficeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateSalesGroupCommand.SalesOfficeId)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.SalesGroupName)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(UpdateSalesGroupCommand.SalesGroupName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SalesGroupName));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SalesGroupName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateSalesGroupCommand.SalesGroupName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.ResponsibleManager)
                            .MaximumLength(maxLengthManager)
                            .WithMessage($"{nameof(UpdateSalesGroupCommand.ResponsibleManager)} {rule.Error} {maxLengthManager} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ResponsibleManager));

                        RuleFor(x => x.RegionTerritory)
                            .MaximumLength(maxLengthRegion)
                            .WithMessage($"{nameof(UpdateSalesGroupCommand.RegionTerritory)} {rule.Error} {maxLengthRegion} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.RegionTerritory));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Sales Group {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.SalesOfficeId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesOfficeExistsAsync(id))
                            .WithMessage($"{nameof(UpdateSalesGroupCommand.SalesOfficeId)} {rule.Error}")
                            .When(x => x.SalesOfficeId > 0);

                        RuleFor(x => x.ProductCategoryId)
                            .MustAsync(async (id, ct) => await _queryRepository.ProductCategoryExistsAsync(id!.Value, ct))
                            .WithMessage($"{nameof(UpdateSalesGroupCommand.ProductCategoryId)} {rule.Error}")
                            .When(x => x.ProductCategoryId.HasValue && x.ProductCategoryId.Value > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                !await _queryRepository.AlreadyExistsAsync(cmd.SalesGroupName, cmd.SalesOfficeId, cmd.Id))
                            .WithMessage($"{nameof(UpdateSalesGroupCommand.SalesGroupName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SalesGroupName) && x.SalesOfficeId > 0 && x.Id > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateSalesGroupCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
