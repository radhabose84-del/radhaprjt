using MediatR;

namespace QCManagement.Application.QualityParameter.Commands.DeleteQualityParameter
{
    public sealed record DeleteQualityParameterCommand(int Id) : IRequest<bool>;
}
