using MediatR;
using QCManagement.Application.QcInspection.Dto;

namespace QCManagement.Application.QcInspection.Queries.ResolveSpec
{
    public class ResolveSpecQuery : IRequest<ResolveSpecPreviewDto>
    {
        public int GrnDetailId { get; set; }
    }
}
