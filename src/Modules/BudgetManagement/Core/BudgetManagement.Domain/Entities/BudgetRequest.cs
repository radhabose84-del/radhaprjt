using BudgetManagement.Domain.Common;

namespace BudgetManagement.Domain.Entities;

public class BudgetRequest : BaseEntity, IActivityTracked
{
	public int UnitId { get; set; }
 	public int FinancialYearId { get; set; }	
	public int CurrencyId { get; set; }
	public string? RequestCode { get; set; }
	public int RequestTypeId { get; set; }
	public MiscMaster? MiscRequestType { get; set; }	
	public int? RequestById { get; set; }
  	public MiscMaster? MiscRequestBy { get; set; }  
  	public int? RequestMonthId { get; set; }
  	public MiscMaster? MiscRequestMonth { get; set; }
  	public int? RevisionNumber { get; set; }
	 // OPEX fields
	public DateOnly? FromDate { get; set; }
	public DateOnly? ToDate { get; set; }
	public int? BudgetGroupId { get; set; }
	public BudgetGroup? BudgetGroupType { get; set; }	
	// CAPEX fields	
	public int? ProjectId { get; set; }
	public int? WBSId { get; set; }

	// Common fields
	public decimal RequestAmount { get; set; }
	public string? Remarks { get; set; }
	public string? ImagePath { get; set; }
	public int StatusId { get; set; }
  	public ICollection<BudgetAllocation>? BudgetAllocationRequestType { get; set; } 
}