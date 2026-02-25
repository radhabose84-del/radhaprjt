using MediatR;

namespace PurchaseManagement.Application.ServiceMaster.Commands.DeleteService
{
    public class DeleteServiceCommand : IRequest<bool>
    {
        public int Id { get; set; }
        
    }
}