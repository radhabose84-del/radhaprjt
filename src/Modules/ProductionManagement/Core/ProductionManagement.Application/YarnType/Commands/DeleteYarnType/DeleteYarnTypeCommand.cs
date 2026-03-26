using MediatR;

namespace ProductionManagement.Application.YarnType.Commands.DeleteYarnType
{
    public sealed record DeleteYarnTypeCommand(int Id) : IRequest<bool>;
}
