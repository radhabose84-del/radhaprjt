using PurchaseManagement.Application.DutyMaster;
using MediatR;

namespace PurchaseManagement.Application.DutyMaster.Command.Create
{
    public class CreateDutyMasterCommand : IRequest<int>
    {
        public DutyMasterDto Model { get; set; } = default!;
    }
}