using MediatR;
using LogisticsManagement.Application.MiscMaster.Dto;

namespace LogisticsManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQuery : IRequest<MiscMasterDto?>
    {
        public int Id { get; set; }
    }
}
