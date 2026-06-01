using MediatR;
using QCManagement.Application.QcInspection.Dto;

namespace QCManagement.Application.QcInspection.Queries.GetQcInspectionById
{
    public class GetQcInspectionByIdQuery : IRequest<QcInspectionDto?>
    {
        public int Id { get; set; }
    }
}
