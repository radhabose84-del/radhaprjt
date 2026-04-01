using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Commands.DeleteYarnConversionHeader;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.YarnConversionHeader
{
    public class DeleteYarnConversionHeaderCommandValidator : AbstractValidator<DeleteYarnConversionHeaderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IYarnConversionHeaderQueryRepository _queryRepository;

        public DeleteYarnConversionHeaderCommandValidator(IYarnConversionHeaderQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteYarnConversionHeaderCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Yarn Conversion {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
