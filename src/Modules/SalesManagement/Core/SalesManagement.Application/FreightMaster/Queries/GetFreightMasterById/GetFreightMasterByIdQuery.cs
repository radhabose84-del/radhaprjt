using MediatR;
using SalesManagement.Application.FreightMaster.Dto;

namespace SalesManagement.Application.FreightMaster.Queries.GetFreightMasterById
{
    public class GetFreightMasterByIdQuery : IRequest<FreightMasterDto?>
    {
        public int Id { get; set; }
    }
}
