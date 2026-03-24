using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Commands.DeleteComplaint;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.Complaint
{
    public class DeleteComplaintCommandValidator : AbstractValidator<DeleteComplaintCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IComplaintQueryRepository _queryRepository;

        public DeleteComplaintCommandValidator(IComplaintQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteComplaintCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Complaint {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
