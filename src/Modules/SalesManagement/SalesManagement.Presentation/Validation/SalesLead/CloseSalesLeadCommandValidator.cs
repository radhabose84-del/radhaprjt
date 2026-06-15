using FluentValidation;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Commands.CloseSalesLead;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesLead
{
    public class CloseSalesLeadCommandValidator : AbstractValidator<CloseSalesLeadCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesLeadQueryRepository _queryRepository;
        private readonly IMarketingOfficerAccessFilter _accessFilter;

        public CloseSalesLeadCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesLeadQueryRepository queryRepository,
            IMarketingOfficerAccessFilter accessFilter)
        {
            _queryRepository = queryRepository;
            _accessFilter = accessFilter;

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.SalesLead>("ClosureRemarks") ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.ClosureTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CloseSalesLeadCommand.ClosureTypeId)} {rule.Error}");

                        RuleFor(x => x.ClosureRemarks)
                            .NotNull()
                            .WithMessage($"{nameof(CloseSalesLeadCommand.ClosureRemarks)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CloseSalesLeadCommand.ClosureRemarks)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ClosureRemarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(CloseSalesLeadCommand.ClosureRemarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ClosureRemarks));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0)
                            .WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Sales Lead {rule.Error}");

                        // A lead can be closed only once (read-only after closure)
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.IsClosedAsync(id))
                            .WithMessage("This lead is already closed and cannot be modified.")
                            .When(x => x.Id > 0);
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ClosureTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CloseSalesLeadCommand.ClosureTypeId)} {rule.Error}")
                            .When(x => x.ClosureTypeId > 0);

                        RuleFor(x => x.ClosureReasonId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CloseSalesLeadCommand.ClosureReasonId)} {rule.Error}")
                            .When(x => x.ClosureReasonId.HasValue && x.ClosureReasonId > 0);

                        RuleFor(x => x.ConvertWonLeadToId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CloseSalesLeadCommand.ConvertWonLeadToId)} {rule.Error}")
                            .When(x => x.ConvertWonLeadToId.HasValue && x.ConvertWonLeadToId > 0);

                        // Closure Reason is mandatory for every closure type EXCEPT Won
                        RuleFor(x => x.ClosureReasonId)
                            .MustAsync(async (cmd, reasonId, ct) =>
                                await _queryRepository.IsWonClosureTypeAsync(cmd.ClosureTypeId)
                                || (reasonId.HasValue && reasonId.Value > 0))
                            .WithMessage("Closure Reason is required.")
                            .When(x => x.ClosureTypeId > 0);

                        // Convert Won Lead To is mandatory ONLY for Won
                        RuleFor(x => x.ConvertWonLeadToId)
                            .MustAsync(async (cmd, targetId, ct) =>
                                !await _queryRepository.IsWonClosureTypeAsync(cmd.ClosureTypeId)
                                || (targetId.HasValue && targetId.Value > 0))
                            .WithMessage("Convert Won Lead To is required for a Won closure.")
                            .When(x => x.ClosureTypeId > 0);
                        break;

                    case "MarketingOfficerAccess":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) =>
                            {
                                if (!await _accessFilter.ShouldApplyFilterAsync(ct))
                                    return true;
                                var record = await _queryRepository.GetByIdAsync(id);
                                return record != null;
                            })
                            .WithMessage("You are not authorized to close this record.")
                            .When(x => x.Id > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
