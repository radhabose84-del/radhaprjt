using MediatR;
using SalesManagement.Application.StoTypeMaster.Dto;

namespace SalesManagement.Application.StoTypeMaster.Queries.GetStoTypeMasterById
{
    public class GetStoTypeMasterByIdQuery : IRequest<StoTypeMasterDto?>
    {
        public int Id { get; set; }
    }
}
