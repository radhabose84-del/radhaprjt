using MediatR;

namespace SalesManagement.Application.SalesOrder.Commands.DeleteSalesOrderImage
{
    public class DeleteSalesOrderImageCommand : IRequest<bool>
    {
        public string? FilePath { get; set; }
    }
}
