using MediatR;

namespace UserManagement.Application.CustomFields.Commands.DeleteCustomField
{
    public class DeleteCustomFieldCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}