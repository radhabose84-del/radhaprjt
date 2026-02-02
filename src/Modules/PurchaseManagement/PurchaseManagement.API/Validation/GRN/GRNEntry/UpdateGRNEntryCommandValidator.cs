using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.UpdateGRNEntry;
using FluentValidation;
using PurchaseManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.API.Validation.GRN.GRNEntry
{
    public class UpdateGRNEntryCommandValidator : AbstractValidator<UpdateGRNEntryCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IGRNEntryQueryRepository _igrnEntryQueryRepository;

        public UpdateGRNEntryCommandValidator(MaxLengthProvider maxLengthProvider, IGRNEntryQueryRepository igrnEntryQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _igrnEntryQueryRepository = igrnEntryQueryRepository;

            if (_validationRules == null || !_validationRules.Any())
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        // Apply rules to each GRN detail
                        RuleForEach(x => x.GrnEntryUpdate.UpdateGRNDetailsDtos)
                            .ChildRules(GateEntry =>
                            {
                                // DC Quantity required
                                GateEntry.RuleFor(pt => pt.DcQuantity)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(UpdateGRNEntryDto.UpdateGRNDetailsDto.DcQuantity)} {rule.Error}");

                                // DC Quantity >= 0
                                GateEntry.RuleFor(pt => pt.DcQuantity)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage("DC Quantity must be a positive value.");

                                // Received Quantity >= 0
                                GateEntry.RuleFor(pt => pt.ReceivedQuantity)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage("Received Quantity must be a positive value.");
                            });

                        // QC Rules (dependent on header IsQcApproved)
                        RuleForEach(x => x.GrnEntryUpdate.UpdateGRNDetailsDtos)
                                    .Must((command, detail) =>
                                        command.GrnEntryUpdate.IsQcApproved != 1 || 
                                        detail.ReceivedQuantity <= ((detail.QcAcceptedQuantity ?? 0) + (detail.QcRejectedQuantity ?? 0))
                                    )
                                    .WithMessage("Received Quantity cannot exceed the sum of QC Accepted and QC Rejected quantities.");

                                RuleForEach(x => x.GrnEntryUpdate.UpdateGRNDetailsDtos)
                                    .Must((command, detail) =>
                                        command.GrnEntryUpdate.IsQcApproved != 1 || 
                                        ((detail.QcAcceptedQuantity ?? 0) + (detail.QcRejectedQuantity ?? 0)) <= detail.ReceivedQuantity
                                    )
                                    .WithMessage("Total QC Accepted + QC Rejected cannot exceed Received Quantity.");

                        // Custom async tolerance validation
                        RuleFor(x => x.GrnEntryUpdate)
                            .CustomAsync(async (header, context, cancellationToken) =>
                            {
                                if (header.UpdateGRNDetailsDtos == null || !header.UpdateGRNDetailsDtos.Any())
                                    return;

                                foreach (var detail in header.UpdateGRNDetailsDtos)
                                {
                                    var toleranceData = await _igrnEntryQueryRepository
                                        .ValidateToleranceQuantity(header.PartyId, detail.PoId, detail.PoSlNoLocal ?? 0, detail.ItemId);

                                    if (toleranceData == null || !toleranceData.Any())
                                        continue;

                                    var data = toleranceData.First();

                                    decimal upperTol = detail.UpperTolerance ?? 0;
                                    decimal lowerTol = detail.LowerTolerance ?? 0;

                                    decimal allowedMaxQty = data.OrderQuantity + (data.OrderQuantity * upperTol / 100);
                                    decimal allowedMinQty = data.OrderQuantity - (data.OrderQuantity * lowerTol / 100);
                                    decimal newTotalQty = data.TotalGrnQty + detail.DcQuantity;

                                    // Partial receipt not allowed → must be within tolerance
                                    if (!data.IsPartialReceiptAllowed)
                                    {
                                        if (newTotalQty < allowedMinQty || newTotalQty > allowedMaxQty)
                                        {
                                            context.AddFailure(
                                                $"Partial receipt not allowed for PO {detail.PoId}, Item {detail.ItemId}, Line {detail.PoSlNoLocal}. " +
                                                $"Received total {newTotalQty} must be within tolerance range {allowedMinQty} - {allowedMaxQty}."
                                            );
                                            continue;
                                        }
                                    }

                                    // Exceeds allowed tolerance limit
                                    if (newTotalQty > allowedMaxQty)
                                    {
                                        context.AddFailure(
                                            $"DC Quantity exceeds the allowed tolerance limit for PO {detail.PoId}, Item {detail.ItemId}, Line {detail.PoSlNoLocal}. " +
                                            $"Allowed max qty: {allowedMaxQty}, Current total: {newTotalQty}."
                                        );
                                    }
                                }
                            });
                        break;
                }
            }
        }
    }
}
