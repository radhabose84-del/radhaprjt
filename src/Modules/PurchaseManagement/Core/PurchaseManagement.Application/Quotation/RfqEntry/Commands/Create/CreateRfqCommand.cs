using MediatR;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Commands.Create
{
    public class CreateRfqCommand : IRequest<int>
    {
        public int InitiationTypeId { get; set; }
        public int? IndentId { get; set; }
        public DateOnly LastSubmitDate { get; set; }
        public List<int>? IndentDetailIds { get; init; } = new();
        public List<RfqItemCreateDto> Items { get; set; } = new();
        public List<RfqSupplierCreateDto> Suppliers { get; set; } = new();
        public List<RfqAttachmentStageRef> Attachments { get; set; } = new();
    }
    public class RfqItemCreateDto
    {
        public int ItemId { get; set; }
        public int HsnId { get; set; }
        public decimal Qty { get; set; }
        public int UomId { get; set; }
    }

    public class RfqSupplierCreateDto
    {
        public int? SupplierId { get; set; }
        public string? Name { get; set; } = default!;
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string? Gst { get; set; }
    }

    public class RfqAttachmentStageRef
    {
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? FileType { get; set; }
    }
}