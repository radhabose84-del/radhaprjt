// InventoryManagement.Application/Item/PutAway/Commands/UpdatePutAwayRule/UpdatePutAwayRuleCommandHandler.cs
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Domain.Entities.Item.PutAway;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Application.Item.PutAway.Commands.UpdatePutAwayRule
{
    public sealed class UpdatePutAwayRuleCommandHandler : IRequestHandler<UpdatePutAwayRuleCommand, Unit>
        {
            private readonly IPutAwayRuleCommandRepository _repo;
            private readonly IPutAwayRuleQueryRepository _queryRepo;

            public UpdatePutAwayRuleCommandHandler(IPutAwayRuleCommandRepository repo, IPutAwayRuleQueryRepository queryRepo)
            {
                _repo = repo;
                _queryRepo = queryRepo;
            }

            public async Task<Unit> Handle(UpdatePutAwayRuleCommand request, CancellationToken ct)
            {
                var b = request.Body;

                if (b.IsActive == 0)
                {
                    var isLinked = await _queryRepo.IsPutAwayRuleLinkedAsync(request.Id);
                    if (isLinked)
                        throw new ExceptionRules(
                            "This master is linked with other records. You cannot inactivate this record.");
                }

                // 1) Load existing entity (tracked, with children)
                var e = await _repo.GetByIdAsync(request.Id, track: true, ct);
                if (e is null)
                    throw new EntityNotFoundException("PutAwayRule", request.Id);

                // 2) Enforce unique scope (exclude current Id)
                if (await _repo.ExistsScopeAsync(b.UnitId, b.WarehouseId, b.ItemGroupId, b.ItemCategoryId, b.ItemId, excludeId: e.Id, ct))
                    throw new EntityAlreadyExistsException("PutAwayRule already exists for this scope.");

                // 3) Update scalars
                e.UnitId         = b.UnitId;
                e.ItemGroupId    = b.ItemGroupId;
                e.ItemCategoryId = b.ItemCategoryId;
                e.ItemId         = b.ItemId;
                e.WarehouseId    = b.WarehouseId;
                 e.IsActive       = b.IsActive == 1 ? Status.Active : Status.Inactive;

                // 4) Merge strategies by PriorityId (ACTIVE ONLY)
                var incoming = (b.Strategies ?? new()).OrderBy(s => s.PriorityId).ToList();

                // Use only active strategies to build the key map
                var existingActiveByPriority = e.Strategies
                    .Where(s => s.IsDeleted == IsDelete.NotDeleted)
                    .OrderByDescending(s => s.Id)           // if duplicates ever existed, pick the latest
                    .GroupBy(s => s.PriorityId)
                    .ToDictionary(g => g.Key, g => g.First());

                foreach (var s in incoming)
                {
                    var incomingStatus = s.IsActive == 1 ? Status.Active : Status.Inactive;

                    if (existingActiveByPriority.TryGetValue(s.PriorityId, out var ex))
                    {
                        var changed = ex.StorageTypeId != s.StorageTypeId || ex.TargetId != s.TargetId;

                        if (changed)
                        {
                            // Soft-delete the old row
                            ex.IsDeleted = IsDelete.Deleted;
                            ex.IsActive = Status.Inactive;

                            // Insert a new active row with same PriorityId
                            e.Strategies.Add(new PutAwayStrategy
                            {
                                PutAwayRuleId = e.Id,
                                StorageTypeId = s.StorageTypeId,
                                TargetId = s.TargetId,
                                PriorityId = s.PriorityId,
                                IsDeleted = IsDelete.NotDeleted,
                                IsActive = incomingStatus
                            });
                        }
                        else
                        {
                            // Keep it active if no changes
                            ex.IsDeleted = IsDelete.NotDeleted;
                            ex.IsActive = incomingStatus;
                        }
                    }
                    else
                    {
                        // Brand new priority → insert
                        e.Strategies.Add(new PutAwayStrategy
                        {
                            PutAwayRuleId = e.Id,
                            StorageTypeId = s.StorageTypeId,
                            TargetId = s.TargetId,
                            PriorityId = s.PriorityId,
                            IsDeleted = IsDelete.NotDeleted,
                            IsActive = incomingStatus
                        });
                    }
                }

                // 5) Soft-delete any ACTIVE priorities omitted from the request
                var incomingPriorities = incoming.Select(x => x.PriorityId).ToHashSet();
                foreach (var st in e.Strategies
                                    .Where(x => x.IsDeleted == IsDelete.NotDeleted && !incomingPriorities.Contains(x.PriorityId))
                                    .ToList())
                {
                    st.IsDeleted = IsDelete.Deleted;
                    st.IsActive  = Status.Inactive;
                }

                try
                {
                    await _repo.SaveAsync(ct);
                    return Unit.Value;
                }
                catch (DbUpdateException)
                {
                    // If you didn’t add the filtered unique index ([IsDeleted] = 0), you can still hit a collision
                    throw new EntityAlreadyExistsException("PutAwayStrategy already exists for this rule/priority.");
                }
            }
        }
    }
