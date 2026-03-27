using MediatR;

namespace ProductionManagement.Application.CertificationMaster.Commands.DeleteCertificationMaster
{
    public sealed record DeleteCertificationMasterCommand(int Id) : IRequest<bool>;
}
