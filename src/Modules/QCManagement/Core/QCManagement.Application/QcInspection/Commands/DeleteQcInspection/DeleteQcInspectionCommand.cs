using MediatR;

namespace QCManagement.Application.QcInspection.Commands.DeleteQcInspection
{
    public sealed record DeleteQcInspectionCommand(int Id) : IRequest<bool>;
}
