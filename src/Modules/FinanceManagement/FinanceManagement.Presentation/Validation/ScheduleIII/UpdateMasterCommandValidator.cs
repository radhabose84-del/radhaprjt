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
                        RuleFor(x => x.ScheduleIIISectionItemId)
                            .NotEmpty().WithMessage($"{nameof(UpdateMasterCommand.ScheduleIIISectionItemId)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.DetailNotFoundAsync(id))
                            .WithMessage($"Schedule III line {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ScheduleIIISectionItemId)
                            .MustAsync(async (id, ct) => !await _queryRepository.LineItemNotFoundAsync(id))
                            .WithMessage($"{nameof(UpdateMasterCommand.ScheduleIIISectionItemId)} {rule.Error}")
                            .When(x => x.ScheduleIIISectionItemId > 0);

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

                    case "AlreadyExists":
                        // Uniqueness within the structure, excluding this detail row.
                        RuleFor(x => x.ScheduleIIISectionItemId)
                            .MustAsync(async (cmd, lineId, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId();
                                var divisionId = _ipAddressService.GetDivisionId();
                                if (companyId is null || divisionId is null) return true;
                                return !await _queryRepository.DetailLineExistsAsync(companyId.Value, divisionId.Value, lineId, cmd.Id);
                            })
                            .WithMessage("This line is already part of the Schedule III structure.")
                            .When(x => x.ScheduleIIISectionItemId > 0);

                        RuleFor(x => x.DisplayOrder)
                            .MustAsync(async (cmd, order, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId();
                                var divisionId = _ipAddressService.GetDivisionId();
                                if (companyId is null || divisionId is null) return true;
                                return !await _queryRepository.DetailDisplayOrderExistsAsync(companyId.Value, divisionId.Value, order, cmd.Id);
                            })
                            .WithMessage($"{nameof(UpdateMasterCommand.DisplayOrder)} {rule.Error}")
                            .When(x => x.DisplayOrder > 0);
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
