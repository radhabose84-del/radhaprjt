using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Application.ProductionPack.Commands.UpdateProduction;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.Production
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

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.ProductionPackDetail>("Remarks") ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.ProductionPackDetails)
                            .NotNull()
                            .WithMessage($"ProductionPackDetails {rule.Error}");

                        RuleFor(x => x.ProductionPackDetails!.WarehouseId)
                            .NotEmpty()
                            .WithMessage($"WarehouseId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null);

                        RuleFor(x => x.ProductionPackDetails!.ItemId)
                            .NotEmpty()
                            .WithMessage($"ItemId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null);

                        RuleFor(x => x.ProductionPackDetails!.LotId)
                            .NotEmpty()
                            .WithMessage($"LotId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null);
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ProductionPackDetails!.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"Remarks {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => x.ProductionPackDetails != null
                                       && !string.IsNullOrWhiteSpace(x.ProductionPackDetails.Remarks));
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
                        RuleFor(x => x.ProductionPackDetails!.WarehouseId)
                            .GreaterThan(0).WithMessage($"WarehouseId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null);

                        RuleFor(x => x.ProductionPackDetails!.ItemId)
                            .GreaterThan(0).WithMessage($"ItemId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null);

                        RuleFor(x => x.ProductionPackDetails!.LotId)
                            .GreaterThan(0).WithMessage($"LotId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null);

                        // PackTypeId and NetWeightPerPack only required when a bag range is specified
                        RuleFor(x => x.ProductionPackDetails!.PackTypeId)
                            .GreaterThan(0).WithMessage($"PackTypeId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null
                                       && x.ProductionPackDetails.StartPackNo.HasValue
                                       && x.ProductionPackDetails.PackTypeId.HasValue);

                        RuleFor(x => x.ProductionPackDetails!.NetWeightPerPack)
                            .GreaterThan(0).WithMessage($"NetWeightPerPack {rule.Error}")
                            .When(x => x.ProductionPackDetails != null
                                       && x.ProductionPackDetails.StartPackNo.HasValue
                                       && x.ProductionPackDetails.NetWeightPerPack.HasValue);

                        RuleFor(x => x.ProductionPackDetails!.StartPackNo)
                            .GreaterThan(0).WithMessage($"StartPackNo {rule.Error}")
                            .When(x => x.ProductionPackDetails != null && x.ProductionPackDetails.StartPackNo.HasValue);

                        RuleFor(x => x.ProductionPackDetails!.EndPackNo)
                            .GreaterThan(0).WithMessage($"EndPackNo {rule.Error}")
                            .When(x => x.ProductionPackDetails != null && x.ProductionPackDetails.EndPackNo.HasValue);

                        RuleFor(x => x.ProductionPackDetails!.BinId)
                            .GreaterThan(0).WithMessage($"BinId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null && x.ProductionPackDetails.BinId.HasValue);

                        RuleFor(x => x.ProductionPackDetails!.QualityStatusId)
                            .GreaterThan(0).WithMessage($"QualityStatusId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null && x.ProductionPackDetails.QualityStatusId.HasValue);
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ProductionPackDetails!.WarehouseId)
                            .MustAsync(async (id, ct) => await _queryRepository.WarehouseExistsAsync(id))
                            .WithMessage($"WarehouseId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null && x.ProductionPackDetails.WarehouseId > 0);

                        RuleFor(x => x.ProductionPackDetails!.LotId)
                            .MustAsync(async (id, ct) => await _queryRepository.LotExistsAsync(id))
                            .WithMessage($"LotId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null && x.ProductionPackDetails.LotId > 0);

                        RuleFor(x => x.ProductionPackDetails!.ItemId)
                            .MustAsync(async (id, ct) => await _queryRepository.ItemExistsAsync(id))
                            .WithMessage($"ItemId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null && x.ProductionPackDetails.ItemId > 0);

                        RuleFor(x => x.ProductionPackDetails!.PackTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.PackTypeExistsAsync(id!.Value))
                            .WithMessage($"PackTypeId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null
                                       && x.ProductionPackDetails.PackTypeId.HasValue
                                       && x.ProductionPackDetails.PackTypeId > 0);

                        RuleFor(x => x.ProductionPackDetails!.BinId)
                            .MustAsync(async (id, ct) => await _queryRepository.BinExistsAsync(id!.Value))
                            .WithMessage($"BinId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null && x.ProductionPackDetails.BinId.HasValue && x.ProductionPackDetails.BinId > 0);

                        RuleFor(x => x.ProductionPackDetails!.QualityStatusId)
                            .MustAsync(async (id, ct) => await _queryRepository.QualityStatusExistsAsync(id!.Value))
                            .WithMessage($"QualityStatusId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null && x.ProductionPackDetails.QualityStatusId.HasValue && x.ProductionPackDetails.QualityStatusId > 0);
                        break;

                    default:
                        break;
                }
            }

            // When bag range is specified, PackTypeId and NetWeightPerPack are required
            RuleFor(x => x.ProductionPackDetails!.PackTypeId)
                .NotNull()
                .WithMessage("PackTypeId is required when a pack range is specified.")
                .When(x => x.ProductionPackDetails != null && x.ProductionPackDetails.StartPackNo.HasValue);

            RuleFor(x => x.ProductionPackDetails!.NetWeightPerPack)
                .NotNull()
                .WithMessage("NetWeightPerPack is required when a pack range is specified.")
                .When(x => x.ProductionPackDetails != null && x.ProductionPackDetails.StartPackNo.HasValue);

            // EndPackNo >= StartPackNo (when both supplied)
            RuleFor(x => x.ProductionPackDetails!.EndPackNo)
                .GreaterThanOrEqualTo(x => x.ProductionPackDetails!.StartPackNo)
                .WithMessage("EndPackNo must be greater than or equal to StartPackNo.")
                .When(x => x.ProductionPackDetails != null
                            && x.ProductionPackDetails.StartPackNo.HasValue
                            && x.ProductionPackDetails.EndPackNo.HasValue);

            // Pack range DB overlap check (exclude current record)
            RuleFor(x => x.ProductionPackDetails!)
                .MustAsync(async (dto, ct) =>
                    !await _queryRepository.PackOverlapExistsAsync(
                        dto.LotId, dto.StartPackNo!.Value, dto.EndPackNo!.Value, dto.Id > 0 ? dto.Id : null))
                .WithMessage("Pack range overlaps with an existing allocation for the same Lot.")
                .When(x => x.ProductionPackDetails != null
                            && x.ProductionPackDetails.LotId > 0
                            && x.ProductionPackDetails.StartPackNo.HasValue
                            && x.ProductionPackDetails.EndPackNo.HasValue);
        }
    }
}
