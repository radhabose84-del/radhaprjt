using MediatR;

namespace InventoryManagement.Application.Issue.Queries.GetApprovedMrsById
{
    public class GetApprovedMrsByIdQuery : IRequest<List<GetApprovedMrsByIdDto>>
    {
        public string? SearchPattern { get; set; }
    }
}