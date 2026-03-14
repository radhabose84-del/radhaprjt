using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IMiscMaster;
using ProductionManagement.Application.MiscMaster.Commands.UpdateMiscMaster;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.MiscMaster
{
    public class UpdateMiscMasterCommandValidator : AbstractValidator<UpdateMiscMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMiscMasterQueryRepository _queryRepo;

        public UpdateMiscMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IMiscMasterQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthDesc = maxLengthProvider.GetMaxLength<Domain.Entities.MiscMaster>("Description") ?? 200;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Description)
                            .NotNull().WithMessage($"{nameof(UpdateMiscMasterCommand.Description)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateMiscMasterCommand.Description)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDesc)
                            .WithMessage($"{nameof(UpdateMiscMasterCommand.Description)} {rule.Error} {maxLengthDesc} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"Misc Master {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.SortOrder)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateMiscMasterCommand.SortOrder)} must be greater than zero.");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateMiscMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
