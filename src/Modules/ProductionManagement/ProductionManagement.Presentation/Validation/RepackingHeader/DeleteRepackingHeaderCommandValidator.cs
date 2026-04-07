using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.DeleteRepackingHeader;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.RepackingHeader
{
    public class DeleteRepackingHeaderCommandValidator : AbstractValidator<DeleteRepackingHeaderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IRepackingHeaderQueryRepository _queryRepository;

        public DeleteRepackingHeaderCommandValidator(IRepackingHeaderQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteRepackingHeaderCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"RepackingHeader {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
