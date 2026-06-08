using MediatR;

namespace PurchaseManagement.Application.FreightRfq.Queries.GetNextFreightRfqNumber
{
    public class GetNextFreightRfqNumberQuery : IRequest<string>
    {
        public DateTimeOffset RfqDate { get; set; }
    }
}
