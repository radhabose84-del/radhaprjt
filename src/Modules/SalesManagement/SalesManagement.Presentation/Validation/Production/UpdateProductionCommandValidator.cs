using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IProductionPack;
using SalesManagement.Application.ProductionPack.Commands.UpdateProduction;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.Production
{
    public class UpdateProductionCommandValidator : AbstractValidator<UpdateProductionCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IProductionQueryRepository _queryRepository;

        public UpdateProductionCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IProductionQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.ProductionPackHeader>("Remarks") ?? 500;
            var maxLengthLineRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.ProductionPackDetail>("LineRemarks") ?? 250;

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
                        RuleFor(x => x.ProductionPackDetails)
                            .NotNull()
                            .WithMessage($"ProductionPackDetails {rule.Error}");

                        RuleFor(x => x.ProductionPackDetails!.WarehouseId)
                            .NotNull()
                            .WithMessage($"WarehouseId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"WarehouseId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null);

                        RuleFor(x => x.ProductionPackDetails!.ProductionPackDetails)
                            .NotNull()
                            .WithMessage($"ProductionPackDetails {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"ProductionPackDetails {rule.Error}")
                            .When(x => x.ProductionPackDetails != null);
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ProductionPackDetails!.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"Remarks {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => x.ProductionPackDetails != null && !string.IsNullOrWhiteSpace(x.ProductionPackDetails.Remarks));

                        RuleForEach(x => x.ProductionPackDetails!.ProductionPackDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.LineRemarks)
                                    .MaximumLength(maxLengthLineRemarks)
                                    .WithMessage($"LineRemarks {rule.Error} {maxLengthLineRemarks} characters.")
                                    .When(d => !string.IsNullOrWhiteSpace(d.LineRemarks));
                            })
                            .When(x => x.ProductionPackDetails?.ProductionPackDetails != null && x.ProductionPackDetails.ProductionPackDetails.Any());
                        break;

                    case "NotFound":
                        RuleFor(x => x.ProductionPackDetails!.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Production {rule.Error}")
                            .When(x => x.ProductionPackDetails != null);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.ProductionPackDetails!.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"IsActive {rule.Error}")
                            .When(x => x.ProductionPackDetails != null);
                        break;

                    case "GreaterThan":
                        RuleForEach(x => x.ProductionPackDetails!.ProductionPackDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.LotId)
                                    .GreaterThan(0)
                                    .WithMessage($"LotId {rule.Error}");

                                detail.RuleFor(d => d.ItemId)
                                    .GreaterThan(0)
                                    .WithMessage($"ItemId {rule.Error}");

                                detail.RuleFor(d => d.PackTypeId)
                                    .GreaterThan(0)
                                    .WithMessage($"PackTypeId {rule.Error}");

                                detail.RuleFor(d => d.StartPackNo)
                                    .GreaterThan(0)
                                    .WithMessage($"StartPackNo {rule.Error}");

                                detail.RuleFor(d => d.EndPackNo)
                                    .GreaterThan(0)
                                    .WithMessage($"EndPackNo {rule.Error}");

                                detail.RuleFor(d => d.BinId)
                                    .GreaterThan(0)
                                    .WithMessage($"BinId {rule.Error}");

                                detail.RuleFor(d => d.QualityStatusId)
                                    .GreaterThan(0)
                                    .WithMessage($"QualityStatusId {rule.Error}");

                                detail.RuleFor(d => d.NetWeightPerPack)
                                    .GreaterThan(0)
                                    .WithMessage($"NetWeightPerPack {rule.Error}");

                                detail.RuleFor(d => d.TotalBags)
                                    .GreaterThan(0)
                                    .WithMessage($"TotalBags {rule.Error}");

                                detail.RuleFor(d => d.TotalNetWeight)
                                    .GreaterThan(0)
                                    .WithMessage($"TotalNetWeight {rule.Error}");
                            })
                            .When(x => x.ProductionPackDetails?.ProductionPackDetails != null && x.ProductionPackDetails.ProductionPackDetails.Any());
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ProductionPackDetails!.WarehouseId)
                            .MustAsync(async (warehouseId, ct) =>
                                await _queryRepository.WarehouseExistsAsync(warehouseId))
                            .WithMessage($"WarehouseId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null && x.ProductionPackDetails.WarehouseId > 0);

                        // Detail-level FK validation
                        RuleForEach(x => x.ProductionPackDetails!.ProductionPackDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.LotId)
                                    .MustAsync(async (lotId, ct) =>
                                        await _queryRepository.LotExistsAsync(lotId))
                                    .WithMessage($"LotId {rule.Error}")
                                    .When(d => d.LotId > 0);

                                detail.RuleFor(d => d.PackTypeId)
                                    .MustAsync(async (packTypeId, ct) =>
                                        await _queryRepository.PackTypeExistsAsync(packTypeId))
                                    .WithMessage($"PackTypeId {rule.Error}")
                                    .When(d => d.PackTypeId > 0);

                                detail.RuleFor(d => d.ItemId)
                                    .MustAsync(async (itemId, ct) =>
                                        await _queryRepository.ItemExistsAsync(itemId))
                                    .WithMessage($"ItemId {rule.Error}")
                                    .When(d => d.ItemId > 0);

                                detail.RuleFor(d => d.BinId)
                                    .MustAsync(async (binId, ct) =>
                                        await _queryRepository.BinExistsAsync(binId))
                                    .WithMessage($"BinId {rule.Error}")
                                    .When(d => d.BinId > 0);

                                detail.RuleFor(d => d.QualityStatusId)
                                    .MustAsync(async (qualityStatusId, ct) =>
                                        await _queryRepository.QualityStatusExistsAsync(qualityStatusId))
                                    .WithMessage($"QualityStatusId {rule.Error}")
                                    .When(d => d.QualityStatusId > 0);
                            })
                            .When(x => x.ProductionPackDetails?.ProductionPackDetails != null && x.ProductionPackDetails.ProductionPackDetails.Any());
                        break;

                    default:
                        break;
                }
            }

            // Custom validation: EndPackNo >= StartPackNo
            RuleForEach(x => x.ProductionPackDetails!.ProductionPackDetails)
                .ChildRules(detail =>
                {
                    detail.RuleFor(d => d.EndPackNo)
                        .GreaterThanOrEqualTo(d => d.StartPackNo)
                        .WithMessage("EndPackNo must be greater than or equal to StartPackNo.")
                        .When(d => d.StartPackNo > 0 && d.EndPackNo > 0);
                })
                .When(x => x.ProductionPackDetails?.ProductionPackDetails != null && x.ProductionPackDetails.ProductionPackDetails.Any());

            // Custom validation: Pack range overlap check against DB (exclude current detail Id)
            RuleForEach(x => x.ProductionPackDetails!.ProductionPackDetails)
                .ChildRules(detail =>
                {
                    detail.RuleFor(d => d)
                        .MustAsync(async (d, ct) =>
                            !await _queryRepository.PackOverlapExistsAsync(
                                d.LotId,  d.StartPackNo, d.EndPackNo, d.Id > 0 ? d.Id : null))
                        .WithMessage("Pack range overlaps with an existing allocation for the same Lot and PackType.")
                        .When(d => d.LotId > 0 && d.PackTypeId > 0 && d.StartPackNo > 0 && d.EndPackNo > 0);
                })
                .When(x => x.ProductionPackDetails?.ProductionPackDetails != null && x.ProductionPackDetails.ProductionPackDetails.Any());

            // Custom validation: Duplicate pack range within same request (same Lot + overlapping StartPackNo/EndPackNo)
            RuleFor(x => x.ProductionPackDetails!.ProductionPackDetails)
                .Must(details =>
                {
                    if (details == null || details.Count < 2)
                        return true;

                    for (int i = 0; i < details.Count; i++)
                    {
                        for (int j = i + 1; j < details.Count; j++)
                        {
                            if (details[i].LotId == details[j].LotId
                                && details[i].LotId > 0
                                && details[i].StartPackNo <= details[j].EndPackNo
                                && details[i].EndPackNo >= details[j].StartPackNo)
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                })
                .WithMessage("Duplicate or overlapping pack ranges found within the same Lot in detail lines.")
                .When(x => x.ProductionPackDetails?.ProductionPackDetails != null && x.ProductionPackDetails.ProductionPackDetails.Count > 1);
        }
    }
}
