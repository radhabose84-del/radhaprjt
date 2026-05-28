using MediatR;

namespace QCManagement.Application.QualityTemplate.Commands.DeleteQualityTemplate
{
    public sealed record DeleteQualityTemplateCommand(int Id) : IRequest<bool>;
}
