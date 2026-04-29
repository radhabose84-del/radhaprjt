using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.ComplaintResolution.Commands.SubmitResolution;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.ComplaintResolution
{
    public class SubmitResolutionCommandValidator : AbstractValidator<SubmitResolutionCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IComplaintResolutionQueryRepository _queryRepository;
        private readonly IComplaintQueryRepository _complaintQueryRepository;

        public SubmitResolutionCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IComplaintResolutionQueryRepository queryRepository,
            IComplaintQueryRepository complaintQueryRepository)
        {
            _queryRepository = queryRepository;
            _complaintQueryRepository = complaintQueryRepository;

            var maxLengthResolutionSummary = maxLengthProvider.GetMaxLength<Domain.Entities.ComplaintResolution>("ResolutionSummary") ?? 2000;
            var maxLengthFinanceReference = maxLengthProvider.GetMaxLength<Domain.Entities.ComplaintResolution>("FinanceReference") ?? 100;
            var maxLengthDispatchReference = maxLengthProvider.GetMaxLength<Domain.Entities.ComplaintResolution>("DispatchReference") ?? 100;
            var maxLengthActionDescription = maxLengthProvider.GetMaxLength<Domain.Entities.ComplaintResolution>("ActionDescription") ?? 2000;
            var maxLengthClosureRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.ComplaintResolution>("ClosureRemarks") ?? 2000;

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
                        RuleFor(x => x.ComplaintHeaderId)
                            .NotEmpty()
                            .WithMessage($"{nameof(SubmitResolutionCommand.ComplaintHeaderId)} {rule.Error}");

                        RuleFor(x => x.ResolutionTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(SubmitResolutionCommand.ResolutionTypeId)} {rule.Error}");

                        RuleFor(x => x.ResolutionSummary)
                            .NotNull()
                            .WithMessage($"{nameof(SubmitResolutionCommand.ResolutionSummary)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(SubmitResolutionCommand.ResolutionSummary)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ResolutionSummary)
                            .MaximumLength(maxLengthResolutionSummary)
                            .WithMessage($"{nameof(SubmitResolutionCommand.ResolutionSummary)} {rule.Error} {maxLengthResolutionSummary} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ResolutionSummary));

                        RuleFor(x => x.FinanceReference)
                            .MaximumLength(maxLengthFinanceReference)
                            .WithMessage($"{nameof(SubmitResolutionCommand.FinanceReference)} {rule.Error} {maxLengthFinanceReference} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.FinanceReference));

                        RuleFor(x => x.DispatchReference)
                            .MaximumLength(maxLengthDispatchReference)
                            .WithMessage($"{nameof(SubmitResolutionCommand.DispatchReference)} {rule.Error} {maxLengthDispatchReference} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.DispatchReference));

                        RuleFor(x => x.ActionDescription)
                            .MaximumLength(maxLengthActionDescription)
                            .WithMessage($"{nameof(SubmitResolutionCommand.ActionDescription)} {rule.Error} {maxLengthActionDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ActionDescription));

                        RuleFor(x => x.ClosureRemarks)
                            .MaximumLength(maxLengthClosureRemarks)
                            .WithMessage($"{nameof(SubmitResolutionCommand.ClosureRemarks)} {rule.Error} {maxLengthClosureRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ClosureRemarks));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ComplaintHeaderId)
                            .MustAsync(async (id, ct) => await _queryRepository.ComplaintExistsAsync(id))
                            .WithMessage("Complaint not found.")
                            .When(x => x.ComplaintHeaderId > 0);

                        RuleFor(x => x.ResolutionTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(SubmitResolutionCommand.ResolutionTypeId)} {rule.Error}")
                            .When(x => x.ResolutionTypeId > 0);

                        RuleFor(x => x.ReturnLocationId)
                            .MustAsync(async (id, ct) => await _queryRepository.WarehouseExistsAsync(id!.Value))
                            .WithMessage($"{nameof(SubmitResolutionCommand.ReturnLocationId)} {rule.Error}")
                            .When(x => x.ReturnLocationId.HasValue && x.ReturnLocationId.Value > 0);

                        RuleFor(x => x.ReturnStatusId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(SubmitResolutionCommand.ReturnStatusId)} {rule.Error}")
                            .When(x => x.ReturnStatusId.HasValue && x.ReturnStatusId.Value > 0);

                        RuleFor(x => x.ClosureStatusId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(SubmitResolutionCommand.ClosureStatusId)} {rule.Error}")
                            .When(x => x.ClosureStatusId.HasValue && x.ClosureStatusId.Value > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.ComplaintHeaderId)
                            .MustAsync(async (id, ct) => !await _queryRepository.ResolutionExistsForComplaintAsync(id))
                            .WithMessage("A resolution already exists for this complaint.")
                            .When(x => x.ComplaintHeaderId > 0);
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.ComplaintHeaderId)
                            .MustAsync(async (id, ct) => await _complaintQueryRepository.IsReadyForResolutionAsync(id))
                            .WithMessage("Resolution cannot be submitted. QC Review must be approved and all mandatory department feedbacks must be submitted first.")
                            .When(x => x.ComplaintHeaderId > 0);
                        break;

                    default:
                        break;
                }
            }

            // Block resolvers from setting ClosureStatus = "Closed" manually on submit.
            // Closed is reserved for system-driven transitions when the downstream
            // artifact (Credit Note posted, Sales Return goods received, Replacement
            // dispatched) is verified complete. Resolver should leave it at "Open" or
            // "Ready for Closure"; the system will close it once verification fires.
            RuleFor(x => x.ClosureStatusId)
                .MustAsync(async (id, ct) => !await _queryRepository.IsClosureStatusClosedAsync(id!.Value))
                .WithMessage("ClosureStatus 'Closed' cannot be set manually. The system will mark a resolution as Closed only after the downstream action (Credit Note / Sales Return / Replacement) is verified.")
                .When(x => x.ClosureStatusId.HasValue && x.ClosureStatusId.Value > 0);

            // Business rule: Sales Return fields mandatory when resolution type requires
            // Note: Conditional validation based on resolution type name would require async lookup.
            // Instead, validate that if return fields are provided, they are valid.
            RuleFor(x => x.ReturnQuantity)
                .GreaterThan(0)
                .WithMessage("Return Quantity must be greater than 0.")
                .When(x => x.ReturnQuantity.HasValue);

            RuleFor(x => x.CreditAmount)
                .GreaterThan(0)
                .WithMessage("Credit Amount must be greater than 0.")
                .When(x => x.CreditAmount.HasValue);

            RuleFor(x => x.ReplacementQuantity)
                .GreaterThan(0)
                .WithMessage("Replacement Quantity must be greater than 0.")
                .When(x => x.ReplacementQuantity.HasValue);
        }
    }
}
