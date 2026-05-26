using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Application.ProductionPack.Commands.CreateProduction;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.Production
{
    public class CreateProductionCommandValidator : AbstractValidator<CreateProductionCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IProductionQueryRepository _queryRepository;

        public CreateProductionCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IProductionQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.ProductionPackEntryDetail>("Remarks") ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.ProductionPackEntries)
                            .NotNull()
                            .WithMessage($"ProductionPackEntries {rule.Error}");

                        // Header-level required fields
                        RuleFor(x => x.ProductionPackEntries!.WarehouseId)
                            .NotEmpty()
                            .WithMessage($"WarehouseId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null);

                        RuleFor(x => x.ProductionPackEntries!.ItemId)
                            .NotEmpty()
                            .WithMessage($"ItemId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null);

                        // At least one detail row required
                        RuleFor(x => x.ProductionPackEntries!.Details)
                            .NotEmpty()
                            .WithMessage("At least one detail row is required.")
                            .When(x => x.ProductionPackEntries != null);

                        // Detail-level required fields
                        RuleForEach(x => x.ProductionPackEntries!.Details).ChildRules(detail =>
                        {
                            detail.RuleFor(d => d.LotId)
                                .NotEmpty()
                                .WithMessage($"LotId {rule.Error}");
                        }).When(x => x.ProductionPackEntries?.Details != null);
                        break;

                    case "MaxLength":
                        RuleForEach(x => x.ProductionPackEntries!.Details).ChildRules(detail =>
                        {
                            detail.RuleFor(d => d.Remarks)
                                .MaximumLength(maxLengthRemarks)
                                .WithMessage($"Remarks {rule.Error} {maxLengthRemarks} characters.")
                                .When(d => !string.IsNullOrWhiteSpace(d.Remarks));
                        }).When(x => x.ProductionPackEntries?.Details != null);
                        break;

                    case "GreaterThan":
                        // Header-level
                        RuleFor(x => x.ProductionPackEntries!.WarehouseId)
                            .GreaterThan(0).WithMessage($"WarehouseId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null);

                        RuleFor(x => x.ProductionPackEntries!.ItemId)
                            .GreaterThan(0).WithMessage($"ItemId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null);

                        RuleFor(x => x.ProductionPackEntries!.BinId)
                            .GreaterThan(0).WithMessage($"BinId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null && x.ProductionPackEntries.BinId.HasValue);

                        RuleFor(x => x.ProductionPackEntries!.QualityStatusId)
                            .GreaterThan(0).WithMessage($"QualityStatusId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null && x.ProductionPackEntries.QualityStatusId.HasValue);

                        // Detail-level
                        RuleForEach(x => x.ProductionPackEntries!.Details).ChildRules(detail =>
                        {
                            detail.RuleFor(d => d.LotId)
                                .GreaterThan(0).WithMessage($"LotId {rule.Error}");

                            detail.RuleFor(d => d.PackTypeId)
                                .GreaterThan(0).WithMessage($"PackTypeId {rule.Error}")
                                .When(d => d.StartPackNo.HasValue && d.PackTypeId.HasValue);

                            detail.RuleFor(d => d.NetWeightPerPack)
                                .GreaterThan(0).WithMessage($"NetWeightPerPack {rule.Error}")
                                .When(d => d.StartPackNo.HasValue && d.NetWeightPerPack.HasValue);

                            detail.RuleFor(d => d.StartPackNo)
                                .GreaterThan(0).WithMessage($"StartPackNo {rule.Error}")
                                .When(d => d.StartPackNo.HasValue);

                            detail.RuleFor(d => d.EndPackNo)
                                .GreaterThan(0).WithMessage($"EndPackNo {rule.Error}")
                                .When(d => d.EndPackNo.HasValue);
                        }).When(x => x.ProductionPackEntries?.Details != null);
                        break;

                    case "FKColumnDelete":
                        // Header-level FKs
                        RuleFor(x => x.ProductionPackEntries!.WarehouseId)
                            .MustAsync(async (id, ct) => await _queryRepository.WarehouseExistsAsync(id))
                            .WithMessage($"WarehouseId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null && x.ProductionPackEntries.WarehouseId > 0);

                        RuleFor(x => x.ProductionPackEntries!.ItemId)
                            .MustAsync(async (id, ct) => await _queryRepository.ItemExistsAsync(id))
                            .WithMessage($"ItemId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null && x.ProductionPackEntries.ItemId > 0);

                        RuleFor(x => x.ProductionPackEntries!.BinId)
                            .MustAsync(async (id, ct) => await _queryRepository.BinExistsAsync(id!.Value))
                            .WithMessage($"BinId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null && x.ProductionPackEntries.BinId.HasValue && x.ProductionPackEntries.BinId > 0);

                        RuleFor(x => x.ProductionPackEntries!.QualityStatusId)
                            .MustAsync(async (id, ct) => await _queryRepository.QualityStatusExistsAsync(id!.Value))
                            .WithMessage($"QualityStatusId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null && x.ProductionPackEntries.QualityStatusId.HasValue && x.ProductionPackEntries.QualityStatusId > 0);

                        // Detail-level FKs
                        RuleForEach(x => x.ProductionPackEntries!.Details).ChildRules(detail =>
                        {
                            detail.RuleFor(d => d.LotId)
                                .MustAsync(async (id, ct) => await _queryRepository.LotExistsAsync(id))
                                .WithMessage($"LotId {rule.Error}")
                                .When(d => d.LotId > 0);

                            detail.RuleFor(d => d.PackTypeId)
                                .MustAsync(async (id, ct) => await _queryRepository.PackTypeExistsAsync(id!.Value))
                                .WithMessage($"PackTypeId {rule.Error}")
                                .When(d => d.PackTypeId.HasValue && d.PackTypeId > 0);

                            detail.RuleFor(d => d.TypeId)
                                .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                                .WithMessage($"TypeId {rule.Error}")
                                .When(d => d.TypeId.HasValue && d.TypeId > 0);
                        }).When(x => x.ProductionPackEntries?.Details != null);
                        break;

                    default:
                        break;
                }
            }

            // Detail-level custom rules (outside switch — always applied)
            RuleForEach(x => x.ProductionPackEntries!.Details).ChildRules(detail =>
            {
                // When bag range is specified, PackTypeId and NetWeightPerPack are required
                detail.RuleFor(d => d.PackTypeId)
                    .NotNull()
                    .WithMessage("PackTypeId is required when a pack range is specified.")
                    .When(d => d.StartPackNo.HasValue);

                detail.RuleFor(d => d.NetWeightPerPack)
                    .NotNull()
                    .WithMessage("NetWeightPerPack is required when a pack range is specified.")
                    .When(d => d.StartPackNo.HasValue);

                // EndPackNo >= StartPackNo (when both supplied)
                detail.RuleFor(d => d.EndPackNo)
                    .GreaterThanOrEqualTo(d => d.StartPackNo)
                    .WithMessage("EndPackNo must be greater than or equal to StartPackNo.")
                    .When(d => d.StartPackNo.HasValue && d.EndPackNo.HasValue);

                // Pack range DB overlap check per detail
                detail.RuleFor(d => d.EndPackNo)
                    .MustAsync(async (detailItem, endPackNo, ct) =>
                        !await _queryRepository.PackOverlapExistsAsync(
                            detailItem.LotId, detailItem.StartPackNo!.Value, endPackNo!.Value))
                    .WithMessage("Pack range overlaps with an existing allocation for the same Lot.")
                    .When(d => d.LotId > 0 && d.StartPackNo.HasValue && d.EndPackNo.HasValue);
            }).When(x => x.ProductionPackEntries?.Details != null);
        }
    }
}
