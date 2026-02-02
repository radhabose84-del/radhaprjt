using PurchaseManagement.Application.DutyMaster;
using MediatR;

namespace PurchaseManagement.Application.Purchase.DutyMaster.Command.Update
{
    public class UpdateDutyMasterCommand : IRequest<bool>
    {
        public DutyMasterDto Model { get; set; } = default!;
    }
}