using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet;
using FluentValidation;

namespace PurchaseManagement.Presentation.Validation.PurchaseOrder.ServicePo
{
    public class CreateServiceEntrySheetValidator : AbstractValidator<CreateServiceEntrySheetCommand>
    {

        private readonly IServicePurchaseOrderCommandRepository _servicePoCommandRepo;

        public CreateServiceEntrySheetValidator(IServicePurchaseOrderCommandRepository servicePoCommandRepo)
        {

            _servicePoCommandRepo = servicePoCommandRepo;

            RuleFor(x => x.CreateServiceSheet).NotNull();

            RuleFor(x => x.CreateServiceSheet.PurchaseOrderId).GreaterThan(0);
            RuleFor(x => x.CreateServiceSheet.SESDate).NotEmpty();
            RuleFor(x => x.CreateServiceSheet.VendorId).GreaterThan(0);

            RuleForEach(x => x.CreateServiceSheet.Activities).ChildRules(act =>
            {
                act.RuleFor(a => a.PerformedById).GreaterThan(0);
            });

            // 🔁 Duplicate SES check (PO + Schedule)
            RuleFor(x => x.CreateServiceSheet)
                .MustAsync(NotAlreadyExistsAsync)
                .WithMessage("Service Entry Sheet already exists for this Purchase Order and schedule.");



        }
        
        private async Task<bool> NotAlreadyExistsAsync(CreateServiceSheetDto dto, CancellationToken ct)
        {
            if (dto.PurchaseOrderId <= 0 || dto.ScheduleID <= 0)
                return true; // let other rules fail instead

            var exists = await _servicePoCommandRepo
                .ServiceEntrySheetExistsAsync(dto.PurchaseOrderId, dto.ScheduleID, ct);

            return !exists; // validation passes only if it does NOT exist
        }
    }
}