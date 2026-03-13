using FluentValidation;
using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Commands.DeleteEWaybillHeader;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.EWaybillHeader
{
    public class DeleteEWaybillHeaderCommandValidator : AbstractValidator<DeleteEWaybillHeaderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IEWaybillHeaderQueryRepository _queryRepository;

        public DeleteEWaybillHeaderCommandValidator(IEWaybillHeaderQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteEWaybillHeaderCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"EWaybill Header {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
