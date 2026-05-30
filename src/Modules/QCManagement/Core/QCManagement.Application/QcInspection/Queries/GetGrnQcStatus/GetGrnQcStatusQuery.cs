using MediatR;
using QCManagement.Application.QcInspection.Dto;

namespace QCManagement.Application.QcInspection.Queries.GetGrnQcStatus
{
    public class GetGrnQcStatusQuery : IRequest<GrnQcStatusDto>
    {
        public int GrnHeaderId { get; set; }
    }
}
