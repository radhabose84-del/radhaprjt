using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateMaster;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.ScheduleIII
{
    public class UpdateMasterCommandValidator : AbstractValidator<UpdateMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public UpdateMasterCommandValidator(IScheduleIIIQueryRepository queryRepository, IIPAddressService ipAddressService)
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
                        RuleFor(x => x.StatusId)
                            .NotEmpty().WithMessage($"{nameof(UpdateMasterCommand.StatusId)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.MasterNotFoundAsync(id))
                            .WithMessage($"Schedule III master {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        // Only a DRAFT structure is editable — a LOCKED structure rejects updates (FR-008).
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

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
