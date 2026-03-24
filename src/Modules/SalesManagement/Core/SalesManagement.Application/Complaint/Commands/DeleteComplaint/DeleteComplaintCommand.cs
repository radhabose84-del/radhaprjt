using MediatR;

namespace SalesManagement.Application.Complaint.Commands.DeleteComplaint
{
    public sealed record DeleteComplaintCommand(int Id) : IRequest<bool>;
}
