using MediatR;
using ProductionManagement.Application.MiscMaster.Dto;

namespace ProductionManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQuery : IRequest<MiscMasterDto?>
    {
        public int Id { get; set; }
    }
}
