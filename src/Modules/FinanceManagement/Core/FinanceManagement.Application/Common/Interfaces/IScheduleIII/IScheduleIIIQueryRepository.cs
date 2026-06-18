using FinanceManagement.Application.ScheduleIII.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IScheduleIII
{
    public interface IScheduleIIIQueryRepository
    {
        // Composite reads — a structure is identified by (CompanyId, DivisionId).
        Task<ScheduleIIIHeaderDto?> GetStructureAsync(int companyId, int divisionId);
        Task<Preview03BDto> Get03BPreviewAsync(int companyId, int divisionId);
        Task<List<ScheduleIIISubTotalDto>> GetSubTotalsAsync();
        Task<ScheduleIIISubTotalDto?> GetSubTotalByIdAsync(int id);
        Task<List<SubTotalFormulaOperandDto>> GetSubTotalFormulaOperandsAsync(int? subTotalId);   // Edit-formula picker: P&L lines + sub-total nodes, with current +/− selection
        Task<ScheduleIIISectionItemDto?> GetLineItemByIdAsync(int id);

        // Activity log (Finance.ActivityLog) — Update/Delete change trail
        Task<(List<ActivityLogDto>, int)> GetActivityLogAsync(string? entityName, int? entityId, int pageNumber, int pageSize);

        // Section (standalone master)
        Task<(List<ScheduleIIISectionDto>, int)> GetAllSectionAsync(int pageNumber, int pageSize, string? searchTerm, int? scheduleIIIMasterId);
        Task<ScheduleIIISectionDto?> GetSectionByIdAsync(int id);
        Task<bool> SectionNotFoundAsync(int id);
        Task<bool> SectionNameExistsAsync(string sectionName, int? id = null);   // uniqueness (excludes self on update)

        // LineItem (standalone master)
        Task<(List<ScheduleIIISectionItemDto>, int)> GetAllLineItemAsync(int pageNumber, int pageSize, string? searchTerm, int? scheduleIIIMasterId, int? sectionId);

        // Existence / validation
        Task<bool> LineItemNotFoundAsync(int id);
        Task<bool> SubTotalNotFoundAsync(int id);
        Task<bool> SubTotalNameExistsAsync(string formulaName, int? id = null);          // uniqueness (excludes self on update)
        Task<bool> SubTotalDisplayOrderExistsAsync(int displayOrder, int? id = null);    // uniqueness (excludes self on update)
        Task<bool> DetailNotFoundAsync(int id);
        Task<bool> StructureExistsByCompanyDivisionAsync(int companyId, int divisionId);
        Task<bool> SectionExistsAsync(int sectionId);
        Task<bool> DetailLineExistsAsync(int companyId, int divisionId, int sectionItemId, int? id = null);          // line once per structure
        Task<bool> DetailDisplayOrderExistsAsync(int companyId, int divisionId, int displayOrder, int? id = null);   // order unique per structure
        Task<bool> IsStructureLockedAsync(int companyId, int divisionId);

        // Resolves the MiscMaster Id for S3_OPERAND_TYPE = SUBTOTAL (operand-kind id for the formula picker)
        Task<int> GetSubTotalOperandTypeIdAsync();

        // 03B usage (stubbed to 0/false until US-GL02-03B ships the ScheduleIIIAccountGroupMap table)
        Task<int> GetMappedCountAsync(int lineItemId);
        Task<bool> IsLineMappedAsync(int lineItemId);
    }
}
