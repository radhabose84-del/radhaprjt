using MediatR;

namespace QCManagement.Application.QualitySpecification.Commands.DeleteQualitySpecification
{
    public sealed record DeleteQualitySpecificationCommand(int Id) : IRequest<bool>;
}
