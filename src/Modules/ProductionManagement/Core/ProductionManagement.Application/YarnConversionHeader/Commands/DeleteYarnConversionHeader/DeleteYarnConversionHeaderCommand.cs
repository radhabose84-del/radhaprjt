using MediatR;

namespace ProductionManagement.Application.YarnConversionHeader.Commands.DeleteYarnConversionHeader
{
    public sealed record DeleteYarnConversionHeaderCommand(int Id) : IRequest<bool>;
}
