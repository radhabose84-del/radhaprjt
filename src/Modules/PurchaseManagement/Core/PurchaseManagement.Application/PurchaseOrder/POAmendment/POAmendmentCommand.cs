// using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using MediatR;
using PurchaseLocalDetailDto.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Application.PurchaseOrder.POAmendment
{
    public sealed class POAmendmentCommand : IRequest<int>
    {/* 
        public int PoId  { get; set; }
        public string? AmendmentReason                { get; set; } */
        public PurchaseOrderUpdateDto Data { get; set; } = default!;
    }
}