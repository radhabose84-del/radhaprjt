using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.FreightRfq.Commands.CreateFreightRfq;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.FreightRfq
{
    public class CreateFreightRfqCommandValidator : AbstractValidator<CreateFreightRfqCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IFreightRfqQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

        public CreateFreightRfqCommandValidator(
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
                        RuleFor(x => x.RfqDate)
                            .NotEmpty().WithMessage($"{nameof(CreateFreightRfqCommand.RfqDate)} {rule.Error}");
                        RuleFor(x => x.RfqTypeId)
                            .NotEmpty().WithMessage($"{nameof(CreateFreightRfqCommand.RfqTypeId)} {rule.Error}");
                        RuleFor(x => x.SourceLocation)
                            .NotNull().WithMessage($"{nameof(CreateFreightRfqCommand.SourceLocation)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateFreightRfqCommand.SourceLocation)} {rule.Error}");
                        RuleFor(x => x.SourceStation)
                            .NotNull().WithMessage($"{nameof(CreateFreightRfqCommand.SourceStation)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateFreightRfqCommand.SourceStation)} {rule.Error}");
                        RuleFor(x => x.DestinationLocation)
                            .NotNull().WithMessage($"{nameof(CreateFreightRfqCommand.DestinationLocation)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateFreightRfqCommand.DestinationLocation)} {rule.Error}");
                        RuleFor(x => x.DestinationStation)
                            .NotNull().WithMessage($"{nameof(CreateFreightRfqCommand.DestinationStation)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateFreightRfqCommand.DestinationStation)} {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.TotalQuantity)
                            .GreaterThan(0).WithMessage($"{nameof(CreateFreightRfqCommand.TotalQuantity)} {rule.Error}");
                        RuleFor(x => x.TotalBaleCount)
                            .GreaterThan(0).WithMessage($"{nameof(CreateFreightRfqCommand.TotalBaleCount)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SourceLocation)
                            .MaximumLength(srcLocMax).WithMessage($"{nameof(CreateFreightRfqCommand.SourceLocation)} {rule.Error} {srcLocMax} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.SourceLocation));
                        RuleFor(x => x.SourceStation)
                            .MaximumLength(srcStnMax).WithMessage($"{nameof(CreateFreightRfqCommand.SourceStation)} {rule.Error} {srcStnMax} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.SourceStation));
                        RuleFor(x => x.DestinationLocation)
                            .MaximumLength(dstLocMax).WithMessage($"{nameof(CreateFreightRfqCommand.DestinationLocation)} {rule.Error} {dstLocMax} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.DestinationLocation));
                        RuleFor(x => x.DestinationStation)
                            .MaximumLength(dstStnMax).WithMessage($"{nameof(CreateFreightRfqCommand.DestinationStation)} {rule.Error} {dstStnMax} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.DestinationStation));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.RfqTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscExistsAsync(id))
                            .WithMessage($"{nameof(CreateFreightRfqCommand.RfqTypeId)} {rule.Error}")
                            .When(x => x.RfqTypeId > 0);

                        RuleFor(x => x.PoReferenceId)
                            .MustAsync(async (id, ct) => await _queryRepository.PurchaseOrderExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateFreightRfqCommand.PoReferenceId)} {rule.Error}")
                            .When(x => x.PoReferenceId is > 0);
                        break;

                    default:
                        break;
                }
            }

            // PO Reference is mandatory when the RFQ Type is "PO Based".
            RuleFor(x => x.PoReferenceId)
                .MustAsync(async (cmd, poId, ct) => !await IsPoBasedAsync(cmd.RfqTypeId) || (poId is > 0))
                .WithMessage("PO Reference is required for a PO Based Freight RFQ.")
                .WithName(nameof(CreateFreightRfqCommand.PoReferenceId));

            // At least one transporter selected (the RFQ is emailed to them on save).
            RuleFor(x => x.Transporters)
                .NotEmpty().WithMessage("At least one transporter must be selected.");

            RuleForEach(x => x.Transporters).ChildRules(t =>
            {
                t.RuleFor(r => r.TransporterId)
                    .GreaterThan(0).WithMessage("Transporter is required.");
            });
        }

        private async Task<bool> IsPoBasedAsync(int rfqTypeId)
        {
            var poBased = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.FreightRfqType, MiscEnumEntity.FreightRfqTypePoBased);
            return poBased != null && poBased.Id == rfqTypeId;
        }
    }
}
