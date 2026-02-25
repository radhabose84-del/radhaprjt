using MediatR;

namespace UserManagement.Application.CustomFields.Queries.GetCustomFieldById
{
    public class GetCustomFieldByIdQuery : IRequest<CustomFieldByIdDTO>
    {
        public int Id { get; set; }
    }
}