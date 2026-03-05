using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Commands.DeleteStoTypeMaster;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.StoTypeMaster
{
    public class DeleteStoTypeMasterCommandValidator : AbstractValidator<DeleteStoTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IStoTypeMasterQueryRepository _queryRepository;

        public DeleteStoTypeMasterCommandValidator(IStoTypeMasterQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteStoTypeMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"STO Type Master {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
