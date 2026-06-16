using FinanceManagement.Application.ScheduleIII.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IScheduleIII
{
    public interface IScheduleIIIQueryRepository
    {
        // Composite reads
        Task<ScheduleIIIStructureDto?> GetByIdAsync(int structureId);                  // full tree (all 5 tables)
        Task<ScheduleIIIStructureDto?> GetStructureAsync(int companyId, int divisionId);
        Task<Preview03BDto> Get03BPreviewAsync(int structureId);
        Task<List<ScheduleIIISubTotalDto>> GetSubTotalsAsync(int structureId);
        Task<ScheduleIIILineItemDto?> GetLineItemByIdAsync(int id);

        // Activity log (Finance.ActivityLog) — Update/Delete change trail
        Task<(List<ActivityLogDto>, int)> GetActivityLogAsync(string? entityName, int? entityId, int pageNumber, int pageSize);

        // Existence / validation
        Task<bool> LineItemNotFoundAsync(int id);
        Task<bool> SubTotalNotFoundAsync(int id);
        Task<bool> StructureNotFoundAsync(int id);
        Task<bool> StructureExistsAsync(int structureId);
        Task<bool> StructureExistsByCompanyDivisionAsync(int companyId, int divisionId);
        Task<bool> SectionExistsAsync(int sectionId, int structureId);
        Task<bool> ParentLineExistsAsync(int parentLineId, int structureId);
        Task<bool> IsStructureLockedAsync(int structureId);

        // Resolves the MiscMaster Id for S3_OPERAND_TYPE = SUBTOTAL (for self-reference checks)
        Task<int> GetSubTotalOperandTypeIdAsync();

        // 03B usage (stubbed to 0/false until US-GL02-03B ships the ScheduleIIIAccountGroupMap table)
        Task<int> GetMappedCountAsync(int lineItemId);
        Task<bool> IsLineMappedAsync(int lineItemId);
    }
}
