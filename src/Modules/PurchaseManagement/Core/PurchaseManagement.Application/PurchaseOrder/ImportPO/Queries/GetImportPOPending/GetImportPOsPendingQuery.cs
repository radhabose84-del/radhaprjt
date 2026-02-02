using MediatR;
namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Queries.GetImportPOPending;

  public sealed class GetImportPOsPendingQuery
        : IRequest<(List<GetPOImportPendingGroupDto> Items, int TotalCount)>
    {
        public int? PageNumber { get; set; } = 1;
        public int? PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
        public int? PoId { get; set; }          
    }
