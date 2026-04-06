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

                        RuleFor(x => x.ProductionPackDetails!.PackTypeId)
                            .NotEmpty()
                            .WithMessage($"PackTypeId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null);
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ProductionPackDetails!.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"Remarks {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => x.ProductionPackDetails != null
                                       && !string.IsNullOrWhiteSpace(x.ProductionPackDetails.Remarks));
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

                        RuleFor(x => x.ProductionPackDetails!.PackTypeId)
                            .GreaterThan(0).WithMessage($"PackTypeId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null);

                        RuleFor(x => x.ProductionPackDetails!.NetWeightPerPack)
                            .GreaterThan(0).WithMessage($"NetWeightPerPack {rule.Error}")
                            .When(x => x.ProductionPackDetails != null);

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
                            .MustAsync(async (id, ct) => await _queryRepository.PackTypeExistsAsync(id))
                            .WithMessage($"PackTypeId {rule.Error}")
                            .When(x => x.ProductionPackDetails != null && x.ProductionPackDetails.PackTypeId > 0);

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

            // EndPackNo >= StartPackNo (when both supplied)
            RuleFor(x => x.ProductionPackDetails!.EndPackNo)
                .GreaterThanOrEqualTo(x => x.ProductionPackDetails!.StartPackNo)
                .WithMessage("EndPackNo must be greater than or equal to StartPackNo.")
                .When(x => x.ProductionPackDetails != null
                            && x.ProductionPackDetails.StartPackNo.HasValue
                            && x.ProductionPackDetails.EndPackNo.HasValue);

            // Pack range DB overlap check
            RuleFor(x => x.ProductionPackDetails!)
                .MustAsync(async (dto, ct) =>
                    !await _queryRepository.PackOverlapExistsAsync(
                        dto.LotId, dto.StartPackNo!.Value, dto.EndPackNo!.Value))
                .WithMessage("Pack range overlaps with an existing allocation for the same Lot.")
                .When(x => x.ProductionPackDetails != null
                            && x.ProductionPackDetails.LotId > 0
                            && x.ProductionPackDetails.StartPackNo.HasValue
                            && x.ProductionPackDetails.EndPackNo.HasValue);
        }
    }
}
