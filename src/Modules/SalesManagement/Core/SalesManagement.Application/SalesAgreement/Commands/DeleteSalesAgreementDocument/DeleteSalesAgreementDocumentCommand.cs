using MediatR;

namespace SalesManagement.Application.SalesAgreement.Commands.DeleteSalesAgreementDocument
{
    public class DeleteSalesAgreementDocumentCommand : IRequest<bool>
    {
        public string? FilePath { get; set; }
    }
}
