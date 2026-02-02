using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO
{
    public class PurchaseOrderServiceHeader : BaseEntity
    {
        public int PurchaseOrderId { get; set; }
         public PurchaseOrderHeader? PurchaseOrder { get; set; }
        public int ServiceCategoryId { get; set; }
        public MiscMaster? MiscServiceCategory { get; set; }
        public int? ContractTypeId { get; set; }
        public MiscMaster? MiscContractType { get; set; }
        public int? FrequencyId { get; set; }
        public MiscMaster? MiscFrequency { get; set; }
        public DateTimeOffset? ValidityFrom { get; set; }
        public DateTimeOffset? ValidityTo { get; set; }                
        public int? TotalOccurrences { get; set; }
        public decimal? OverallLimit { get; set; }
        public int? CostCenterId { get; set; }  
        public int? ModeOfDispatchId { get; set; }
        public MiscMaster? MiscModeOfDispatch { get; set; }
        public decimal? FreightCharges { get; set; }
        public int? TermsId { get; set; }
        public string? TermDescription { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? BillingAddress { get; set; }
        public string? POImage { get; set; }     
        
         public ICollection<PurchaseOrderServiceLine> Items { get; set; } = new List<PurchaseOrderServiceLine>();

    }
}