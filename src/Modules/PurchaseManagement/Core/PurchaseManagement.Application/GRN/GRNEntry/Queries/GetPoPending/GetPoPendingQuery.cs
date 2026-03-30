using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetPoPending
{
    public class GetPoPendingQuery : IRequest<List<GetPoPendingDto>>
    {
    }
}
