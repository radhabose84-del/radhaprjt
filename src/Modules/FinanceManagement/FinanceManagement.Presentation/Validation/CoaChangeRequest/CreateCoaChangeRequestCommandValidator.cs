using FluentValidation;
using FinanceManagement.Application.CoaChangeRequest.Commands.CreateCoaChangeRequest;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.CoaChangeRequest
{
    public class CreateCoaChangeRequestCommandValidator : AbstractValidator<CreateCoaChangeRequestCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public CreateCoaChangeRequestCommandValidator()
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        // AC5 — the impact assessment is mandatory at raise time.
                        RuleFor(x => x.ImpactAssessment)
                            .NotNull().WithMessage($"{nameof(CreateCoaChangeRequestCommand.ImpactAssessment)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateCoaChangeRequestCommand.ImpactAssessment)} {rule.Error}");

                        RuleFor(x => x.Justification)
                            .NotNull().WithMessage($"{nameof(CreateCoaChangeRequestCommand.Justification)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateCoaChangeRequestCommand.Justification)} {rule.Error}");

                        RuleFor(x => x.ChangeType)
                            .NotNull().WithMessage($"{nameof(CreateCoaChangeRequestCommand.ChangeType)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateCoaChangeRequestCommand.ChangeType)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // The change must target an account and/or a group.
            RuleFor(x => x)
                .Must(x => x.TargetAccountId.HasValue || x.TargetAccountGroupId.HasValue)
                .WithMessage("A change request must target an account or an account group.");
        }
    }
}
