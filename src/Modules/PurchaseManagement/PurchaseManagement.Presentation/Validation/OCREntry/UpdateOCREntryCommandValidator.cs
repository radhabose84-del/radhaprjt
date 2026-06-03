using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Commands.UpdateOCREntry;

namespace PurchaseManagement.Presentation.Validation.OCREntry
{
    public class UpdateOCREntryCommandValidator : AbstractValidator<UpdateOCREntryCommand>
    {
        public UpdateOCREntryCommandValidator(
            IOCREntryQueryRepository queryRepo,
            ISupplierLookup supplierLookup,
            ILocationMasterLookup locationLookup,
            IStationLookup stationLookup,
            IItemLookup itemLookup,
            ICountMasterLookup countLookup)
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid Id is required.")
                .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id))
                .WithMessage("OCR not found.")
                .MustAsync(async (id, ct) => await queryRepo.IsEditableAsync(id))
                .WithMessage("This OCR is approved/converted and can no longer be edited.")
                .When(x => x.Id > 0);

            RuleFor(x => x.OcrDate).NotEmpty().WithMessage("OcrDate is required.");

            RuleFor(x => x.ProcurementSourceId)
                .GreaterThan(0).WithMessage("ProcurementSourceId is required.")
                .MustAsync(async (id, ct) => await queryRepo.MiscMasterExistsAsync(id))
                .WithMessage("ProcurementSourceId is inactive/deleted.")
                .When(x => x.ProcurementSourceId > 0);

            RuleFor(x => x.ProcurementTypeId)
                .GreaterThan(0).WithMessage("ProcurementTypeId is required.")
                .MustAsync(async (id, ct) => await queryRepo.MiscMasterExistsAsync(id))
                .WithMessage("ProcurementTypeId is inactive/deleted.")
                .When(x => x.ProcurementTypeId > 0);

            RuleFor(x => x.BrokerDirectId)
                .GreaterThan(0).WithMessage("BrokerDirectId is required.")
                .MustAsync(async (id, ct) => await queryRepo.MiscMasterExistsAsync(id))
                .WithMessage("BrokerDirectId is inactive/deleted.")
                .When(x => x.BrokerDirectId > 0);

            RuleFor(x => x.GradeId!.Value)
                .MustAsync(async (id, ct) => await queryRepo.MiscMasterExistsAsync(id))
                .WithMessage("GradeId is inactive/deleted.")
                .When(x => x.GradeId.HasValue && x.GradeId.Value > 0);

            RuleFor(x => x.PaymentTermId)
                .GreaterThan(0).WithMessage("PaymentTermId is required.")
                .MustAsync(async (id, ct) => await queryRepo.PaymentTermExistsAsync(id))
                .WithMessage("PaymentTermId is inactive/deleted.")
                .When(x => x.PaymentTermId > 0);

            RuleFor(x => x.SupplierId)
                .GreaterThan(0).WithMessage("SupplierId is required.")
                .MustAsync(async (id, ct) => await supplierLookup.GetActiveSupplierByIdAsync(id, ct) != null)
                .WithMessage("SupplierId is inactive/deleted.")
                .When(x => x.SupplierId > 0);

            RuleFor(x => x.LocationId)
                .GreaterThan(0).WithMessage("LocationId is required.")
                .MustAsync(async (id, ct) => await locationLookup.GetByIdAsync(id, ct) != null)
                .WithMessage("LocationId is inactive/deleted.")
                .When(x => x.LocationId > 0);

            RuleFor(x => x.StationId)
                .GreaterThan(0).WithMessage("StationId is required.")
                .MustAsync(async (id, ct) => await stationLookup.GetByIdAsync(id, ct) != null)
                .WithMessage("StationId is inactive/deleted.")
                .When(x => x.StationId > 0);

            RuleFor(x => x.ItemId)
                .GreaterThan(0).WithMessage("ItemId is required.")
                .MustAsync(async (id, ct) => (await itemLookup.GetByIdsAsync(new[] { id }, ct)).Any())
                .WithMessage("ItemId is inactive/deleted.")
                .When(x => x.ItemId > 0);

            RuleFor(x => x.CountId)
                .GreaterThan(0).WithMessage("CountId is required.")
                .MustAsync(async (id, ct) => (await countLookup.GetByIdsAsync(new[] { id }, ct)).Any())
                .WithMessage("CountId is inactive/deleted.")
                .When(x => x.CountId > 0);

            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
            RuleFor(x => x.Rate).GreaterThan(0).WithMessage("Rate must be greater than zero.");

            RuleFor(x => x.IsActive)
                .InclusiveBetween(0, 1).WithMessage("IsActive must be either 0 or 1.");
        }
    }
}
