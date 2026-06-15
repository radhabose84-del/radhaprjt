using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IYarnType;
using ProductionManagement.Application.YarnType.Commands.UpdateYarnType;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.YarnType
{
    public class UpdateYarnTypeCommandValidator : AbstractValidator<UpdateYarnTypeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IYarnTypeQueryRepository _queryRepo;

        public UpdateYarnTypeCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IYarnTypeQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.YarnType>("YarnTypeName") ?? 50;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<Domain.Entities.YarnType>("Description") ?? 200;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.YarnTypeName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateYarnTypeCommand.YarnTypeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateYarnTypeCommand.YarnTypeName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.YarnTypeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateYarnTypeCommand.YarnTypeName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdateYarnTypeCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"Yarn Type {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) => !await _queryRepo.YarnTypeNameExistsAsync(cmd.YarnTypeName!, cmd.Id))
                            .WithMessage($"{nameof(UpdateYarnTypeCommand.YarnTypeName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.YarnTypeName));
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateYarnTypeCommand.IsActive)} {rule.Error}");
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.AdditionalPrice)
                            .GreaterThanOrEqualTo(0m)
                            .WithMessage($"{nameof(UpdateYarnTypeCommand.AdditionalPrice)} {rule.Error}")
                            .When(x => x.AdditionalPrice.HasValue);
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.CurrencyId)
                            .MustAsync(async (id, ct) => await _queryRepo.CurrencyExistsAsync(id!.Value, ct))
                            .WithMessage($"{nameof(UpdateYarnTypeCommand.CurrencyId)} {rule.Error}")
                            .When(x => x.CurrencyId.HasValue && x.CurrencyId.Value > 0);

                        // Currency is mandatory when an Additional Price is provided
                        RuleFor(x => x.CurrencyId)
                            .Must(id => id.HasValue && id.Value > 0)
                            .WithMessage($"{nameof(UpdateYarnTypeCommand.CurrencyId)} is required when {nameof(UpdateYarnTypeCommand.AdditionalPrice)} is provided.")
                            .When(x => x.AdditionalPrice.HasValue && x.AdditionalPrice.Value > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
