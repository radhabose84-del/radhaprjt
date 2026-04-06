using FluentValidation;
using LogisticsManagement.Application.Common.Interfaces.IFreightMaster;
using LogisticsManagement.Application.FreightMaster.Commands.DeleteFreightMaster;
using Shared.Validation.Common;

namespace LogisticsManagement.Presentation.Validation.FreightMaster
{
    public class DeleteFreightMasterCommandValidator : AbstractValidator<DeleteFreightMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IFreightMasterQueryRepository _queryRepository;

        public DeleteFreightMasterCommandValidator(IFreightMasterQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteFreightMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"FreightMaster {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
