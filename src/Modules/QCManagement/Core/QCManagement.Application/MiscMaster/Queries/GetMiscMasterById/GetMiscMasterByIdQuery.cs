using MediatR;
using QCManagement.Application.MiscMaster.Dto;

namespace QCManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQuery : IRequest<MiscMasterDto?>
    {
        public int Id { get; set; }
    }
}
