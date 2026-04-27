using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.ComplaintResolution.Commands.UpdateResolution;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.ComplaintResolution
{
    public class UpdateResolutionCommandValidator : AbstractValidator<UpdateResolutionCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IComplaintResolutionQueryRepository _queryRepository;

        public UpdateResolutionCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IComplaintResolutionQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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
                        RuleFor(x => x.ResolutionTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateResolutionCommand.ResolutionTypeId)} {rule.Error}");

                        RuleFor(x => x.ResolutionSummary)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateResolutionCommand.ResolutionSummary)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateResolutionCommand.ResolutionSummary)} {rule.Error}");

                        RuleFor(x => x.ClosureRemarks)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateResolutionCommand.ClosureRemarks)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateResolutionCommand.ClosureRemarks)} {rule.Error}")
                            .When(x => x.ClosureStatusId.HasValue && x.ClosureStatusId.Value > 0);
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ResolutionSummary)
                            .MaximumLength(maxLengthResolutionSummary)
                            .WithMessage($"{nameof(UpdateResolutionCommand.ResolutionSummary)} {rule.Error} {maxLengthResolutionSummary} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ResolutionSummary));

                        RuleFor(x => x.FinanceReference)
                            .MaximumLength(maxLengthFinanceReference)
                            .WithMessage($"{nameof(UpdateResolutionCommand.FinanceReference)} {rule.Error} {maxLengthFinanceReference} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.FinanceReference));

                        RuleFor(x => x.DispatchReference)
                            .MaximumLength(maxLengthDispatchReference)
                            .WithMessage($"{nameof(UpdateResolutionCommand.DispatchReference)} {rule.Error} {maxLengthDispatchReference} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.DispatchReference));

                        RuleFor(x => x.ActionDescription)
                            .MaximumLength(maxLengthActionDescription)
                            .WithMessage($"{nameof(UpdateResolutionCommand.ActionDescription)} {rule.Error} {maxLengthActionDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ActionDescription));

                        RuleFor(x => x.ClosureRemarks)
                            .MaximumLength(maxLengthClosureRemarks)
                            .WithMessage($"{nameof(UpdateResolutionCommand.ClosureRemarks)} {rule.Error} {maxLengthClosureRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ClosureRemarks));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage("Resolution not found.");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ResolutionTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(UpdateResolutionCommand.ResolutionTypeId)} {rule.Error}")
                            .When(x => x.ResolutionTypeId > 0);

                        RuleFor(x => x.ReturnLocationId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateResolutionCommand.ReturnLocationId)} {rule.Error}")
                            .When(x => x.ReturnLocationId.HasValue && x.ReturnLocationId.Value > 0);

                        RuleFor(x => x.ReturnStatusId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateResolutionCommand.ReturnStatusId)} {rule.Error}")
                            .When(x => x.ReturnStatusId.HasValue && x.ReturnStatusId.Value > 0);

                        RuleFor(x => x.ClosureStatusId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateResolutionCommand.ClosureStatusId)} {rule.Error}")
                            .When(x => x.ClosureStatusId.HasValue && x.ClosureStatusId.Value > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .Must(x => x == 0 || x == 1)
                            .WithMessage($"{nameof(UpdateResolutionCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // Block resolvers from setting ClosureStatus = "Closed" manually.
            // Closed is reserved for system-driven transitions when the downstream
            // artifact (Credit Note posted, Sales Return goods received, Replacement
            // dispatched) is verified complete. Until those auto-close hooks are wired,
            // the resolver should leave it at "Open" or "Ready for Closure".
            RuleFor(x => x.ClosureStatusId)
                .MustAsync(async (id, ct) => !await _queryRepository.IsClosureStatusClosedAsync(id!.Value))
                .WithMessage("ClosureStatus 'Closed' cannot be set manually. The system will mark a resolution as Closed only after the downstream action (Credit Note / Sales Return / Replacement) is verified.")
                .When(x => x.ClosureStatusId.HasValue && x.ClosureStatusId.Value > 0);

            // Business rule validations
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
