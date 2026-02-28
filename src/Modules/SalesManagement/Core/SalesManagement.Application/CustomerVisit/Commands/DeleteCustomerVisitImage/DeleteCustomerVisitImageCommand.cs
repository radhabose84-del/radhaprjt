using MediatR;

namespace SalesManagement.Application.CustomerVisit.Commands.DeleteCustomerVisitImage
{
    public class DeleteCustomerVisitImageCommand : IRequest<bool>
    {
        public string? ImagePath { get; set; }
    }
}
