using MediatR;

namespace ProductionManagement.Application.QualityMaster.Commands.DeleteQualityMaster
{
    public sealed record DeleteQualityMasterCommand(int Id) : IRequest<bool>;
}
