using MediatR;
using ProductionManagement.Application.QualityMaster.Dto;

namespace ProductionManagement.Application.QualityMaster.Queries.GetQualityMasterById
{
    public class GetQualityMasterByIdQuery : IRequest<QualityMasterDto>
    {
        public int Id { get; set; }
    }
}
