using Contracts.Common;
using MediatR;
using QCManagement.Application.QcInspection.Dto;

namespace QCManagement.Application.QcInspection.Commands.CreateQcInspection
{
    public class CreateQcInspectionCommand : IRequest<ApiResponseDTO<QcInspectionDto>>
    {
        public int SourceTypeId { get; set; }    // QC.MiscMaster (QP_SOURCE_TYPE): GRN / ARRIVAL
        public int SourceDetailId { get; set; }  // GrnDetailId (GRN) or ArrivalDetailId (Arrival)
    }
}
