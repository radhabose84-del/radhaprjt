using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPendingDetails
{
    public class GetGrnPendingDetailsQuery : IRequest<List<GetGrnPendingDetailsDto>>
    {
        public int? GrnId { get; set; }
        public bool? IsGrnGenerated { get; set; }
        public bool? IsQcGenerated { get; set; }
        
    }
}