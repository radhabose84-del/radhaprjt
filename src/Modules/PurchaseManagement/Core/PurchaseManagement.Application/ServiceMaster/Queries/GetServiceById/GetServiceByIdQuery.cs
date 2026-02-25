using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using MediatR;

namespace PurchaseManagement.Application.ServiceMaster.Queries.GetServiceById
{
    public class GetServiceByIdQuery   : IRequest<GetServiceMasterDto>
    {
        public int Id { get; set; }
    }
}