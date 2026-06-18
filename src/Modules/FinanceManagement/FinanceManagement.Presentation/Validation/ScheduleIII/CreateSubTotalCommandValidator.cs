using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.ScheduleIII
{
    public class CreateSubTotalCommandValidator : AbstractValidator<CreateSubTotalCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public CreateSubTotalCommandValidator(IScheduleIIIQueryRepository queryRepository, IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;

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
                        RuleFor(x => x.SubTotalTypeId)
                            .NotEmpty().WithMessage($"{nameof(CreateSubTotalCommand.SubTotalTypeId)} {rule.Error}");

                        RuleFor(x => x.Formulas)
                            .Must(f => f != null && f.Count > 0)
                            .WithMessage("A sub-total must have at least one operand in its formula.");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.SubTotalTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.SubTotalTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateSubTotalCommand.SubTotalTypeId)} {rule.Error}")
                            .When(x => x.SubTotalTypeId > 0);

                        // Structure must not be locked (post-lock edits go through FR-008). Structure = token company/division.
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId();
                                var divisionId = _ipAddressService.GetDivisionId();
                                if (companyId is null || divisionId is null) return true;
                                return !await _queryRepository.IsStructureLockedAsync(companyId.Value, divisionId.Value);
                            })
                            .WithMessage("Structure is locked — edits must go through change control (FR-008).");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
