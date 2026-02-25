using MediatR;

namespace PurchaseManagement.Application.TnCTemplateMaster.Command.DeleteTnCTemplateMasterCommand
{
    public class DeleteTnCTemplateMasterCommand   : IRequest<bool>
    {
        public int Id { get; set; }
    }
}