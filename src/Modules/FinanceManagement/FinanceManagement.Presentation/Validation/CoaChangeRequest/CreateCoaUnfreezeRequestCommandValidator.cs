using FluentValidation;
using FinanceManagement.Application.CoaChangeRequest.Commands.CreateCoaUnfreezeRequest;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.CoaChangeRequest
{
    public class CreateCoaUnfreezeRequestCommandValidator : AbstractValidator<CreateCoaUnfreezeRequestCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public CreateCoaUnfreezeRequestCommandValidator()
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Reason)
                            .NotNull().WithMessage($"{nameof(CreateCoaUnfreezeRequestCommand.Reason)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateCoaUnfreezeRequestCommand.Reason)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            RuleFor(x => x.ChangeRequestIds)
                .NotEmpty().WithMessage("At least one impact-approved change request must be selected.");
        }
    }
}
