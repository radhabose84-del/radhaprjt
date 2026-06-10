using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.FreightRfq.Commands.UpdateFreightRfq;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.FreightRfq
{
    public class UpdateFreightRfqCommandValidator : AbstractValidator<UpdateFreightRfqCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IFreightRfqQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

        public UpdateFreightRfqCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IFreightRfqQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;

            var srcLocMax = maxLengthProvider.GetMaxLength<PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>("SourceLocation") ?? 100;
            var srcStnMax = maxLengthProvider.GetMaxLength<PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>("SourceStation") ?? 100;
            var dstLocMax = maxLengthProvider.GetMaxLength<PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>("DestinationLocation") ?? 100;
            var dstStnMax = maxLengthProvider.GetMaxLength<PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>("DestinationStation") ?? 100;

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
                        RuleFor(x => x.RfqTypeId)
                            .NotEmpty().WithMessage($"{nameof(UpdateFreightRfqCommand.RfqTypeId)} {rule.Error}");
                        RuleFor(x => x.SourceLocation)
                            .NotNull().WithMessage($"{nameof(UpdateFreightRfqCommand.SourceLocation)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateFreightRfqCommand.SourceLocation)} {rule.Error}");
                        RuleFor(x => x.SourceStation)
                            .NotNull().WithMessage($"{nameof(UpdateFreightRfqCommand.SourceStation)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateFreightRfqCommand.SourceStation)} {rule.Error}");
                        RuleFor(x => x.DestinationLocation)
                            .NotNull().WithMessage($"{nameof(UpdateFreightRfqCommand.DestinationLocation)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateFreightRfqCommand.DestinationLocation)} {rule.Error}");
                        RuleFor(x => x.DestinationStation)
                            .NotNull().WithMessage($"{nameof(UpdateFreightRfqCommand.DestinationStation)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateFreightRfqCommand.DestinationStation)} {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.TotalQuantity)
                            .GreaterThan(0).WithMessage($"{nameof(UpdateFreightRfqCommand.TotalQuantity)} {rule.Error}");
                        RuleFor(x => x.TotalBaleCount)
                            .GreaterThan(0).WithMessage($"{nameof(UpdateFreightRfqCommand.TotalBaleCount)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SourceLocation)
                            .MaximumLength(srcLocMax).WithMessage($"{nameof(UpdateFreightRfqCommand.SourceLocation)} {rule.Error} {srcLocMax} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.SourceLocation));
                        RuleFor(x => x.SourceStation)
                            .MaximumLength(srcStnMax).WithMessage($"{nameof(UpdateFreightRfqCommand.SourceStation)} {rule.Error} {srcStnMax} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.SourceStation));
                        RuleFor(x => x.DestinationLocation)
                            .MaximumLength(dstLocMax).WithMessage($"{nameof(UpdateFreightRfqCommand.DestinationLocation)} {rule.Error} {dstLocMax} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.DestinationLocation));
                        RuleFor(x => x.DestinationStation)
                            .MaximumLength(dstStnMax).WithMessage($"{nameof(UpdateFreightRfqCommand.DestinationStation)} {rule.Error} {dstStnMax} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.DestinationStation));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => await _queryRepository.NotFoundAsync(id))
                            .WithName("Freight RFQ")
                            .WithMessage($"Freight RFQ {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.RfqTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscExistsAsync(id))
                            .WithMessage($"{nameof(UpdateFreightRfqCommand.RfqTypeId)} {rule.Error}")
                            .When(x => x.RfqTypeId > 0);

                        RuleFor(x => x.PoReferenceId)
                            .MustAsync(async (id, ct) => await _queryRepository.PurchaseOrderExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateFreightRfqCommand.PoReferenceId)} {rule.Error}")
                            .When(x => x.PoReferenceId is > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateFreightRfqCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // R4 — only a Draft RFQ may be edited.
            RuleFor(x => x.Id)
                .MustAsync(async (id, ct) => (await _queryRepository.GetStatusCodeAsync(id)) == MiscEnumEntity.Draft)
                .WithMessage("Only a Draft Freight RFQ can be edited.")
                .WithName("Freight RFQ")
                .When(x => x.Id > 0);

            // PO Reference is mandatory when the RFQ Type is "PO Based".
            RuleFor(x => x.PoReferenceId)
                .MustAsync(async (cmd, poId, ct) => !await IsPoBasedAsync(cmd.RfqTypeId) || (poId is > 0))
                .WithMessage("PO Reference is required for a PO Based Freight RFQ.")
                .WithName(nameof(UpdateFreightRfqCommand.PoReferenceId));
        }

        private async Task<bool> IsPoBasedAsync(int rfqTypeId)
        {
            var poBased = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.FreightRfqType, MiscEnumEntity.FreightRfqTypePoBased);
            return poBased != null && poBased.Id == rfqTypeId;
        }
    }
}
