using MediatR;
using ProductionManagement.Application.CertificationMaster.Dto;

namespace ProductionManagement.Application.CertificationMaster.Queries.GetCertificationMasterById
{
    public class GetCertificationMasterByIdQuery : IRequest<CertificationMasterDto>
    {
        public int Id { get; set; }
    }
}
