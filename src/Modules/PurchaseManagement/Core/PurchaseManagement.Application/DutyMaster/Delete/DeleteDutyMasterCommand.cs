using MediatR;

namespace PurchaseManagement.Application.DutyMaster.Delete
{
    public record DeleteDutyMasterCommand(int Id) : IRequest<bool>;
}