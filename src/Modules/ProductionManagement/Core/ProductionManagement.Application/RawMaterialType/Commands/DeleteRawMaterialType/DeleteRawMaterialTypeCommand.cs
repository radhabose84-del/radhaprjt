using MediatR;

namespace ProductionManagement.Application.RawMaterialType.Commands.DeleteRawMaterialType
{
    public sealed record DeleteRawMaterialTypeCommand(int Id) : IRequest<bool>;
}
