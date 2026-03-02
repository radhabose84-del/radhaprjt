using MediatR;

namespace SalesManagement.Application.SalesOrder.Commands.DeleteSalesOrderDocument
{
    public class DeleteSalesOrderDocumentCommand : IRequest<bool>
    {
        public string? FilePath { get; set; }
    }
}
