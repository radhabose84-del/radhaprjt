using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.CreateMaster;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.ScheduleIII
{
    public class CreateMasterCommandValidator : AbstractValidator<CreateMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public CreateMasterCommandValidator(IScheduleIIIQueryRepository queryRepository, IIPAddressService ipAddressService)
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
                        RuleFor(x => x.ScheduleIIISectionId)
                            .NotEmpty().WithMessage($"{nameof(CreateMasterCommand.ScheduleIIISectionId)} {rule.Error}");
                        RuleFor(x => x.ScheduleIIISectionItemId)
                            .NotEmpty().WithMessage($"{nameof(CreateMasterCommand.ScheduleIIISectionItemId)} {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ScheduleIIISectionId)
                            .MustAsync(async (id, ct) => await _queryRepository.SectionExistsAsync(id))
                            .WithMessage($"{nameof(CreateMasterCommand.ScheduleIIISectionId)} {rule.Error}")
                            .When(x => x.ScheduleIIISectionId > 0);

                        RuleFor(x => x.ScheduleIIISectionItemId)
                            .MustAsync(async (id, ct) => !await _queryRepository.LineItemNotFoundAsync(id))
                            .WithMessage($"{nameof(CreateMasterCommand.ScheduleIIISectionItemId)} {rule.Error}")
                            .When(x => x.ScheduleIIISectionItemId > 0);

                        // Structure must not be locked (token company/division).
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId();
                                var divisionId = _ipAddressService.GetDivisionId();
                                if (companyId is null || divisionId is null) return true;
                                return !await _queryRepository.IsStructureLockedAsync(companyId.Value, divisionId.Value);
                            })
                            .WithMessage("Structure is locked — updates are not allowed. Edits must go through change control (FR-008).");
                        break;

                    case "AlreadyExists":
                        // A line appears at most once per structure, and DisplayOrder is unique per structure.
                        RuleFor(x => x.ScheduleIIISectionItemId)
                            .MustAsync(async (cmd, lineId, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId();
                                var divisionId = _ipAddressService.GetDivisionId();
                                if (companyId is null || divisionId is null) return true;
                                return !await _queryRepository.DetailLineExistsAsync(companyId.Value, divisionId.Value, lineId);
                            })
                            .WithMessage("This line is already part of the Schedule III structure.")
                            .When(x => x.ScheduleIIISectionItemId > 0);

                        RuleFor(x => x.DisplayOrder)
                            .MustAsync(async (cmd, order, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId();
                                var divisionId = _ipAddressService.GetDivisionId();
                                if (companyId is null || divisionId is null) return true;
                                return !await _queryRepository.DetailDisplayOrderExistsAsync(companyId.Value, divisionId.Value, order);
                            })
                            .WithMessage($"{nameof(CreateMasterCommand.DisplayOrder)} {rule.Error}")
                            .When(x => x.DisplayOrder > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
