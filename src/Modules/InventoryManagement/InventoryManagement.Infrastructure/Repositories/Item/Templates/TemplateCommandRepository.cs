// TemplateCommandRepository.cs
using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.item.ItemDetail.Templates;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

internal sealed class TemplateCommandRepository : ITemplateCommandRepository
{
    private readonly ApplicationDbContext _db;
    public TemplateCommandRepository(ApplicationDbContext db) => _db = db;

    public async Task<int> CreateAsync(InspectionTemplate entity, CancellationToken ct)
    {
        _db.InspectionTemplate.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task UpdateWithParametersAsync(InspectionTemplate entity, IEnumerable<InspectionParameter> parameters, CancellationToken ct)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                _db.InspectionTemplate.Update(entity);

                var current = await _db.InspectionParameter
                    .Where(p => p.TemplateId == entity.Id)
                    .ToListAsync(ct);

                var incomingById = parameters.ToDictionary(p => p.Id);

                var toRemove = current.Where(c => !incomingById.ContainsKey(c.Id)).ToList();
                if (toRemove.Count > 0)
                    _db.InspectionParameter.RemoveRange(toRemove);

                foreach (var p in parameters)
                {
                    if (p.Id == 0)
                    {
                        p.TemplateId = entity.Id;
                        _db.InspectionParameter.Add(p);
                    }
                    else
                    {
                        var existing = current.First(c => c.Id == p.Id);
                        existing.Parameter = p.Parameter;
                        existing.AcceptanceCriteriaValue = p.AcceptanceCriteriaValue;
                        existing.Numeric = p.Numeric;
                        existing.MinimumValue = p.MinimumValue;
                        existing.MaximumValue = p.MaximumValue;
                    }
                }

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }

    public async Task SoftDeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _db.InspectionTemplate.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (entity is null) return;

        entity.IsDeleted = BaseEntity.IsDelete.Deleted;
        _db.InspectionTemplate.Update(entity);
        await _db.SaveChangesAsync(ct);
    }
}
