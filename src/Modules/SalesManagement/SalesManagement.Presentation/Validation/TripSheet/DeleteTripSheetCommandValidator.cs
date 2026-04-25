using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Application.TripSheet.Commands.DeleteTripSheet;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.TripSheet
{
    public class DeleteTripSheetCommandValidator : AbstractValidator<DeleteTripSheetCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ITripSheetQueryRepository _queryRepository;

        public DeleteTripSheetCommandValidator(ITripSheetQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
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
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteTripSheetCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"TripSheet {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
