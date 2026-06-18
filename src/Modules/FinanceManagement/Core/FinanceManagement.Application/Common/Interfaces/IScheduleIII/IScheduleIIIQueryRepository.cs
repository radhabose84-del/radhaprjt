using FinanceManagement.Application.ScheduleIII.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IScheduleIII
{
    public interface IScheduleIIIQueryRepository
    {
        // Composite reads — a structure is identified by (CompanyId, DivisionId).
        Task<ScheduleIIIMasterDto?> GetStructureAsync(int companyId, int divisionId);
        Task<Preview03BDto> Get03BPreviewAsync(int companyId, int divisionId);
        Task<List<ScheduleIIISubTotalDto>> GetSubTotalsAsync();
        Task<List<SubTotalFormulaOperandDto>> GetSubTotalFormulaOperandsAsync(int? subTotalId);   // Edit-formula picker: P&L lines + sub-total nodes, with current +/− selection
        Task<ScheduleIIISectionItemDto?> GetLineItemByIdAsync(int id);

        // Activity log (Finance.ActivityLog) — Update/Delete change trail
        Task<(List<ActivityLogDto>, int)> GetActivityLogAsync(string? entityName, int? entityId, int pageNumber, int pageSize);

        // Section (standalone master)
        Task<(List<ScheduleIIISectionDto>, int)> GetAllSectionAsync(int pageNumber, int pageSize, string? searchTerm, int? scheduleIIIMasterId);
        Task<ScheduleIIISectionDto?> GetSectionByIdAsync(int id);
        Task<bool> SectionNotFoundAsync(int id);

        // LineItem (standalone master)
        Task<(List<ScheduleIIISectionItemDto>, int)> GetAllLineItemAsync(int pageNumber, int pageSize, string? searchTerm, int? scheduleIIIMasterId, int? sectionId);

        // Existence / validation
        Task<bool> LineItemNotFoundAsync(int id);
        Task<bool> SubTotalNotFoundAsync(int id);
        Task<bool> MasterNotFoundAsync(int id);
        Task<bool> StructureExistsByCompanyDivisionAsync(int companyId, int divisionId);
        Task<bool> SectionExistsAsync(int sectionId);
        Task<bool> MasterContainsLineAsync(int companyId, int divisionId, int scheduleIIILineItemId);
        Task<bool> IsStructureLockedAsync(int companyId, int divisionId);
        Task<bool> SubTotalTypeExistsAsync(int miscId);   // MiscMaster row under S3_SUBTOTAL_TYPE

        // Resolves the MiscMaster Id for S3_OPERAND_TYPE = SUBTOTAL (for self-reference checks)
        Task<int> GetSubTotalOperandTypeIdAsync();

        // 03B usage (stubbed to 0/false until US-GL02-03B ships the ScheduleIIIAccountGroupMap table)
        Task<int> GetMappedCountAsync(int lineItemId);
        Task<bool> IsLineMappedAsync(int lineItemId);
    }
}
