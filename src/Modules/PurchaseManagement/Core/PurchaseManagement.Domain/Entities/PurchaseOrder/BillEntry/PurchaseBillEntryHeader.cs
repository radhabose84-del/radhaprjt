using PurchaseManagement.Domain.Common;
namespace PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;

public class PurchaseBillEntryHeader : BaseEntity
	{
		public int UnitId { get; set; }
		public string BillNumber { get; set; } = default!;
		public DateOnly BillDate { get; set; }
		public int PartyId { get; set; }
		public int? PoId { get; set; }
		public int? GrnId { get; set; }
        public int POCategoryId{ get; set; } 
		public int POMethodId { get; set; } 		
		public decimal SubTotal { get; set; }
		public decimal DiscountTotal { get; set; }
		public decimal TaxableAmount { get; set; }
		public decimal CgstAmount { get; set; }
		public decimal SgstAmount { get; set; }
		public decimal IgstAmount { get; set; }
		public decimal OtherCharges { get; set; }
		public decimal RoundOff { get; set; }
		public decimal GrandTotal { get; set; }
		public string? AttachmentPath { get; set; }
		
		public string? Remarks { get; set; }		
		public bool IsBillAccounted  { get; set; }
		public ICollection<PurchaseBillEntryDetail> Lines { get; set; } 
			= new List<PurchaseBillEntryDetail>();
	}
