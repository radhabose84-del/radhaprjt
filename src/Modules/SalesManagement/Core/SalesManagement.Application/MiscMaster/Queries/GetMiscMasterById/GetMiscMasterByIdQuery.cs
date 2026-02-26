using MediatR;
using SalesManagement.Application.MiscMaster.Dto;

namespace SalesManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQuery : IRequest<MiscMasterDto?>
    {
        public int Id { get; set; }
    }
}
