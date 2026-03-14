using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.ICountMaster;
using ProductionManagement.Application.CountMaster.Commands.CreateCountMaster;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.CountMaster
{
    public class CreateCountMasterCommandValidator : AbstractValidator<CreateCountMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICountMasterQueryRepository _queryRepo;
        private readonly IUOMLookup _uomLookup;

        public CreateCountMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ICountMasterQueryRepository queryRepo,
            IUOMLookup uomLookup)
        {
            _queryRepo = queryRepo;
            _uomLookup = uomLookup;

            var maxLengthShortName = maxLengthProvider.GetMaxLength<Domain.Entities.CountMaster>("ShortName") ?? 50;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<Domain.Entities.CountMaster>("CountDescription") ?? 250;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.CountDescription)
                            .NotNull()
                            .WithMessage($"{nameof(CreateCountMasterCommand.CountDescription)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCountMasterCommand.CountDescription)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ShortName)
                            .MaximumLength(maxLengthShortName)
                            .WithMessage($"{nameof(CreateCountMasterCommand.ShortName)} {rule.Error} {maxLengthShortName} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ShortName));

                        RuleFor(x => x.CountDescription)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreateCountMasterCommand.CountDescription)} {rule.Error} {maxLengthDescription} characters.");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.CountValue)
                            .GreaterThan(0m)
                            .WithMessage($"{nameof(CreateCountMasterCommand.CountValue)} {rule.Error}");

                        RuleFor(x => x.CountTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateCountMasterCommand.CountTypeId)} {rule.Error}");

                        RuleFor(x => x.UOMId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateCountMasterCommand.UOMId)} {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.CountTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.CountTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateCountMasterCommand.CountTypeId)} {rule.Error}")
                            .When(x => x.CountTypeId > 0);

                        RuleFor(x => x.CountCategoryId)
                            .MustAsync(async (id, ct) => await _queryRepo.CountCategoryExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateCountMasterCommand.CountCategoryId)} {rule.Error}")
                            .When(x => x.CountCategoryId.HasValue && x.CountCategoryId > 0);

                        RuleFor(x => x.UOMId)
                            .MustAsync(async (id, ct) =>
                            {
                                var uoms = await _uomLookup.GetByIdsAsync(new[] { id }, ct);
                                return uoms.Any();
                            })
                            .WithMessage($"{nameof(CreateCountMasterCommand.UOMId)} {rule.Error}")
                            .When(x => x.UOMId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
