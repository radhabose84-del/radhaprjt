using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Application.CertificationMaster.Commands.DeleteCertificationMaster;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.CertificationMaster
{
    public class DeleteCertificationMasterCommandValidator : AbstractValidator<DeleteCertificationMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICertificationMasterQueryRepository _queryRepository;

        public DeleteCertificationMasterCommandValidator(ICertificationMasterQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteCertificationMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Certification Master {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
