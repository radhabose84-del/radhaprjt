using FluentValidation;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNPutaway;

namespace PurchaseManagement.Presentation.Validation.GRN.GRNEntry
{
    public class CreateGRNPutawayCommandValidator : AbstractValidator<CreateGRNPutawayCommand>
    {
        public CreateGRNPutawayCommandValidator()
        {
            // Validate that the list is not null or empty
            RuleFor(x => x.PutawayList)
                .NotNull().WithMessage("Putaway list cannot be null.")
                .Must(list => list != null && list.Any())
                .WithMessage("Putaway list cannot be empty.");

            // Validate each item in the PutawayList
            RuleForEach(x => x.PutawayList).SetValidator(new CreateGRNPutawayDtoValidator());
        }
    }

    public class CreateGRNPutawayDtoValidator : AbstractValidator<CreateGRNPutawayDto>
    {
        public CreateGRNPutawayDtoValidator()
        {
            RuleFor(x => x.GrnDetailId)
                .GreaterThan(0)
                .WithMessage("GrnDetailId must be greater than 0.");

            RuleFor(x => x.UnitId)
                .GreaterThan(0)
                .WithMessage("UnitId must be greater than 0.");

            RuleFor(x => x.QcAcceptedQtyPurchaseUom)
                .GreaterThan(0)
                .WithMessage("QcAcceptedQtyPurchaseUom must be greater than 0.");

             RuleFor(x => x.QcAcceptedQtyStockUom)
                .GreaterThan(0)
                .WithMessage("QcAcceptedQtyStockUom must be greater than 0.");

            RuleFor(x => x.GrnId)
                .GreaterThan(0)
                .WithMessage("GrnId must be greater than 0.");

            RuleFor(x => x.PoId)
                .GreaterThan(0)
                .WithMessage("PoId must be greater than 0.");

            RuleFor(x => x.ItemId)
                .GreaterThan(0)
                .WithMessage("ItemId must be greater than 0.");

            RuleFor(x => x.WarehouseId)
                .GreaterThan(0)
                .WithMessage("WarehouseId must be greater than 0.");

            RuleFor(x => x.StorageTypeId)
                .GreaterThan(0)
                .WithMessage("StorageTypeId must be greater than 0.");

            RuleFor(x => x.TargetId)
                .GreaterThan(0)
                .WithMessage("TargetId must be greater than 0.");

            RuleFor(x => x.PriorityId)
                .GreaterThan(0)
                .WithMessage("PriorityId must be greater than 0.");

            RuleFor(x => x.PurchaseUomId)
                .GreaterThan(0)
                .WithMessage("PurchaseUomId must be greater than 0.");

            RuleFor(x => x.StockUomId)
                .GreaterThan(0)
                .WithMessage("StockUomId must be greater than 0.");

            RuleFor(x => x.Override)
                .InclusiveBetween((byte)0, (byte)1)
                .WithMessage("Override must be 0 or 1.");
        }
    }
}