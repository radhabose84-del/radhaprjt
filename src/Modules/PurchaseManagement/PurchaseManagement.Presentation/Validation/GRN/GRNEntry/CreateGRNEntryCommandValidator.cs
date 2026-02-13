using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNEntry;
using FluentValidation;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.GRN.GRNEntry
{
    public class CreateGRNEntryCommandValidator : AbstractValidator<CreateGRNEntryCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IGRNEntryQueryRepository _igrnEntryQueryRepository;

        public CreateGRNEntryCommandValidator(MaxLengthProvider maxLengthProvider, IGRNEntryQueryRepository igrnEntryQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _igrnEntryQueryRepository = igrnEntryQueryRepository;

            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                   
                           RuleFor(x => x.GrnEntryCreate.PartyId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateGRNEntryCommand.GrnEntryCreate.PartyId)} {rule.Error}");

                        RuleForEach(x => x.GrnEntryCreate.GRNDetailsDtos).ChildRules(GateEntry =>
                        {
                            // ✅ Rule 1: DcQuantity is required
                            GateEntry.RuleFor(pt => pt.DcQuantity)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreateGRNEntryCommand.GrnEntryCreate.GRNDetailsDtos)}.{nameof(CreateGRNEntryDto.CreateGRNDetailsDto.DcQuantity)} {rule.Error}");

                            // ✅ Optional: DcQuantity ≥ 0
                            GateEntry.RuleFor(pt => pt.DcQuantity)
                                .GreaterThanOrEqualTo(0)
                                .WithMessage("DC Quantity must be a positive value.");

                            // ✅ Optional: DcQuantity ≥ 0
                            GateEntry.RuleFor(pt => pt.ReceivedQuantity)
                                .GreaterThanOrEqualTo(0)
                                .WithMessage("ReceivedQuantity Quantity must be a positive value.");

                           RuleFor(x => x.GrnEntryCreate)
                                .CustomAsync(async (header, context, cancellationToken) =>
                                {
                                    if (header.GRNDetailsDtos == null || !header.GRNDetailsDtos.Any())
                                        return;

                                    foreach (var detail in header.GRNDetailsDtos)
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

                                        // ✅ Condition 1: Partial receipt not allowed → must be within tolerance band
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

                                        // ✅ Condition 2: Exceeds tolerance
                                        if (newTotalQty > allowedMaxQty)
                                        {
                                            context.AddFailure(
                                                $"DC Quantity exceeds allowed tolerance for PO {detail.PoId}, Item {detail.ItemId}, Line {detail.PoSlNoLocal}. " +
                                                $"Allowed max qty: {allowedMaxQty}, Current total: {newTotalQty}."
                                            );
                                        }
                                    }
                                });


                        });
                        break;
                }
            }

        }
    }
}