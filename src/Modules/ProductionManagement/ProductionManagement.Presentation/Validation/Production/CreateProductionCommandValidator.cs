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

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.ProductionPackEntry>("Remarks") ?? 500;

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

                        RuleFor(x => x.ProductionPackEntries!.WarehouseId)
                            .NotEmpty()
                            .WithMessage($"WarehouseId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null);

                        RuleFor(x => x.ProductionPackEntries!.ItemId)
                            .NotEmpty()
                            .WithMessage($"ItemId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null);

                        RuleFor(x => x.ProductionPackEntries!.LotId)
                            .NotEmpty()
                            .WithMessage($"LotId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null);
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ProductionPackEntries!.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"Remarks {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => x.ProductionPackEntries != null
                                       && !string.IsNullOrWhiteSpace(x.ProductionPackEntries.Remarks));
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.ProductionPackEntries!.WarehouseId)
                            .GreaterThan(0).WithMessage($"WarehouseId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null);

                        RuleFor(x => x.ProductionPackEntries!.ItemId)
                            .GreaterThan(0).WithMessage($"ItemId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null);

                        RuleFor(x => x.ProductionPackEntries!.LotId)
                            .GreaterThan(0).WithMessage($"LotId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null);

                        // PackTypeId and NetWeightPerPack only required when a bag range is specified
                        RuleFor(x => x.ProductionPackEntries!.PackTypeId)
                            .GreaterThan(0).WithMessage($"PackTypeId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null
                                       && x.ProductionPackEntries.StartPackNo.HasValue
                                       && x.ProductionPackEntries.PackTypeId.HasValue);

                        RuleFor(x => x.ProductionPackEntries!.NetWeightPerPack)
                            .GreaterThan(0).WithMessage($"NetWeightPerPack {rule.Error}")
                            .When(x => x.ProductionPackEntries != null
                                       && x.ProductionPackEntries.StartPackNo.HasValue
                                       && x.ProductionPackEntries.NetWeightPerPack.HasValue);

                        RuleFor(x => x.ProductionPackEntries!.StartPackNo)
                            .GreaterThan(0).WithMessage($"StartPackNo {rule.Error}")
                            .When(x => x.ProductionPackEntries != null && x.ProductionPackEntries.StartPackNo.HasValue);

                        RuleFor(x => x.ProductionPackEntries!.EndPackNo)
                            .GreaterThan(0).WithMessage($"EndPackNo {rule.Error}")
                            .When(x => x.ProductionPackEntries != null && x.ProductionPackEntries.EndPackNo.HasValue);

                        RuleFor(x => x.ProductionPackEntries!.BinId)
                            .GreaterThan(0).WithMessage($"BinId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null && x.ProductionPackEntries.BinId.HasValue);

                        RuleFor(x => x.ProductionPackEntries!.QualityStatusId)
                            .GreaterThan(0).WithMessage($"QualityStatusId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null && x.ProductionPackEntries.QualityStatusId.HasValue);
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ProductionPackEntries!.WarehouseId)
                            .MustAsync(async (id, ct) => await _queryRepository.WarehouseExistsAsync(id))
                            .WithMessage($"WarehouseId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null && x.ProductionPackEntries.WarehouseId > 0);

                        RuleFor(x => x.ProductionPackEntries!.LotId)
                            .MustAsync(async (id, ct) => await _queryRepository.LotExistsAsync(id))
                            .WithMessage($"LotId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null && x.ProductionPackEntries.LotId > 0);

                        RuleFor(x => x.ProductionPackEntries!.ItemId)
                            .MustAsync(async (id, ct) => await _queryRepository.ItemExistsAsync(id))
                            .WithMessage($"ItemId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null && x.ProductionPackEntries.ItemId > 0);

                        RuleFor(x => x.ProductionPackEntries!.PackTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.PackTypeExistsAsync(id!.Value))
                            .WithMessage($"PackTypeId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null
                                       && x.ProductionPackEntries.PackTypeId.HasValue
                                       && x.ProductionPackEntries.PackTypeId > 0);

                        RuleFor(x => x.ProductionPackEntries!.BinId)
                            .MustAsync(async (id, ct) => await _queryRepository.BinExistsAsync(id!.Value))
                            .WithMessage($"BinId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null && x.ProductionPackEntries.BinId.HasValue && x.ProductionPackEntries.BinId > 0);

                        RuleFor(x => x.ProductionPackEntries!.QualityStatusId)
                            .MustAsync(async (id, ct) => await _queryRepository.QualityStatusExistsAsync(id!.Value))
                            .WithMessage($"QualityStatusId {rule.Error}")
                            .When(x => x.ProductionPackEntries != null && x.ProductionPackEntries.QualityStatusId.HasValue && x.ProductionPackEntries.QualityStatusId > 0);
                        break;

                    default:
                        break;
                }
            }

            // When bag range is specified, PackTypeId and NetWeightPerPack are required
            RuleFor(x => x.ProductionPackEntries!.PackTypeId)
                .NotNull()
                .WithMessage("PackTypeId is required when a pack range is specified.")
                .When(x => x.ProductionPackEntries != null && x.ProductionPackEntries.StartPackNo.HasValue);

            RuleFor(x => x.ProductionPackEntries!.NetWeightPerPack)
                .NotNull()
                .WithMessage("NetWeightPerPack is required when a pack range is specified.")
                .When(x => x.ProductionPackEntries != null && x.ProductionPackEntries.StartPackNo.HasValue);

            // EndPackNo >= StartPackNo (when both supplied)
            RuleFor(x => x.ProductionPackEntries!.EndPackNo)
                .GreaterThanOrEqualTo(x => x.ProductionPackEntries!.StartPackNo)
                .WithMessage("EndPackNo must be greater than or equal to StartPackNo.")
                .When(x => x.ProductionPackEntries != null
                            && x.ProductionPackEntries.StartPackNo.HasValue
                            && x.ProductionPackEntries.EndPackNo.HasValue);

            // Pack range DB overlap check
            RuleFor(x => x.ProductionPackEntries!)
                .MustAsync(async (dto, ct) =>
                    !await _queryRepository.PackOverlapExistsAsync(
                        dto.LotId, dto.StartPackNo!.Value, dto.EndPackNo!.Value))
                .WithMessage("Pack range overlaps with an existing allocation for the same Lot.")
                .When(x => x.ProductionPackEntries != null
                            && x.ProductionPackEntries.LotId > 0
                            && x.ProductionPackEntries.StartPackNo.HasValue
                            && x.ProductionPackEntries.EndPackNo.HasValue);
        }
    }
}
