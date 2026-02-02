    // UpdateRfqCommand.cs
    using System.Text.Json.Serialization;
    using MediatR;

    public class UpdateRfqCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public int InitiationTypeId { get; set; }
        public int? IndentId { get; set; }
        public int IsActive { get; set; }
        public List<RfqItemUpsertDto> Items { get; set; } = new();
        public List<RfqSupplierUpsertDto> Suppliers { get; set; } = new();
    }

    public class RfqItemUpsertDto
    {
        public int? Id { get; set; }
        public int ItemId { get; set; }
        public int HsnId { get; set; }

        // <-- binds the incoming JSON field "qty"
        [JsonPropertyName("qty")]
        public decimal Quantity { get; set; }

        public int UomId { get; set; }
    }

    public class RfqSupplierUpsertDto
    {
        public int? Id { get; set; }
        public int? SupplierId { get; set; }
        public string Name { get; set; } = default!;
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string? Gst { get; set; }
    }
