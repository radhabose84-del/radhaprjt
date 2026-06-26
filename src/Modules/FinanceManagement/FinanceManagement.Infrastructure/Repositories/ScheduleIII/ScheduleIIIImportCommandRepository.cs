using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Infrastructure.Repositories.ScheduleIII
{
    public class ScheduleIIIImportCommandRepository : IScheduleIIIImportCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ScheduleIIIImportCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(int SectionsCreated, int ItemsCreated, List<int> CreatedSectionIds)> CommitAsync(
            IReadOnlyList<(ScheduleIIISection Section, List<ScheduleIIISectionItem> Items)> graph, CancellationToken ct)
        {
            // DbContext uses EnableRetryOnFailure → a user transaction must run inside the execution strategy.
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);

                var sections = graph.Select(g => g.Section).ToList();
                await _dbContext.ScheduleIIISection.AddRangeAsync(sections, ct);
                await _dbContext.SaveChangesAsync(ct);   // section ids assigned

                var itemsCreated = 0;
                foreach (var (section, items) in graph)
                {
                    foreach (var item in items)
                        item.SectionId = section.Id;
                    if (items.Count > 0)
                    {
                        await _dbContext.ScheduleIIISectionItem.AddRangeAsync(items, ct);
                        itemsCreated += items.Count;
                    }
                }
                await _dbContext.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);

                return (sections.Count, itemsCreated, sections.Select(s => s.Id).ToList());
            });
        }
    }
}
