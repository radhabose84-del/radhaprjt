using FinanceManagement.Domain.Entities;

namespace FinanceManagement.Application.Common.Interfaces.IScheduleIII
{
    public interface IScheduleIIIImportCommandRepository
    {
        // Atomically create the sections (each with its line items) in ONE transaction — all-or-nothing.
        // Returns (sectionsCreated, itemsCreated, createdSectionIds).
        Task<(int SectionsCreated, int ItemsCreated, List<int> CreatedSectionIds)> CommitAsync(
            IReadOnlyList<(ScheduleIIISection Section, List<ScheduleIIISectionItem> Items)> graph,
            CancellationToken ct);
    }
}
