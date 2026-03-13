using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Commands.CreateEInvoiceHeader
{
    public class CreateEInvoiceHeaderCommand : IRequest<ApiResponseDTO<int>>
    {
        public int UnitId { get; set; }
        public string? DocType { get; set; }
        public string? SupplyType { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public string? PlaceOfSupply { get; set; }
        public string? IrnNumber { get; set; }
        public string? AckNo { get; set; }
        public DateTimeOffset? AckDate { get; set; }
        public string? SignInvoice { get; set; }
        public string? SignQrCode { get; set; }
        public string? IrnStatus { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public int PartyId { get; set; }
        public string? GstNo { get; set; }
        public bool ReverseCharge { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal Cess { get; set; }
        public decimal StateCess { get; set; }
        public decimal TCS { get; set; }
        public decimal Discount { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal RoundOff { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string? Remarks { get; set; }
        public int? StatusId { get; set; }
    }
}
