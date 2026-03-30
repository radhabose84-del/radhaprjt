using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.CertificationMaster.Commands.CreateCertificationMaster
{
    public class CreateCertificationMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? CertificationName { get; set; }
        public string? Description { get; set; }
    }
}
