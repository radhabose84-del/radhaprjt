using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Commands.DeleteVendorRatingGrade;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.VendorRatingGrade
{
    public class DeleteVendorRatingGradeCommandValidator : AbstractValidator<DeleteVendorRatingGradeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IVendorRatingGradeQueryRepository _queryRepository;

        public DeleteVendorRatingGradeCommandValidator(IVendorRatingGradeQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteVendorRatingGradeCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"VendorRatingGrade {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
