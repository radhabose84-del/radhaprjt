#nullable disable
using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Domain.Entities.PriceMaster;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using System.Text.Json;
using Contracts.Commands.Workflow;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.Application.PriceMaster.Command.CreatePriceMaster;
using Contracts.Events.Notifications;
using Contracts.Interfaces.Lookups.Common;
using Microsoft.Extensions.Logging;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;

namespace PurchaseManagement.Application.PriceMaster.Commands.Create
{
    public sealed class CreatePriceMasterCommandHandler : IRequestHandler<CreatePriceMasterCommand, int>
    {
        private readonly IPriceMasterCommandRepository _repo;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddress;
        private readonly IMiscMasterQueryRepository _miscRepo;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IMediator _mediator;
        private readonly ILogger<CreatePriceMasterCommandHandler> _logger;
        private readonly IItemLookup _itemLookup;
        private readonly IPartyLookup _partyLookup;
        private readonly IAppDataMiscMasterLookup _appDataMiscLookup;

        public CreatePriceMasterCommandHandler(
            IPriceMasterCommandRepository repo,
            IMapper mapper,
            IIPAddressService ipAddress,
            IMiscMasterQueryRepository miscRepo,
            IOutboxEventPublisher outboxEventPublisher,
            IMediator mediator,
            ILogger<CreatePriceMasterCommandHandler> logger,
            IItemLookup itemLookup,
            IPartyLookup partyLookup,
            IAppDataMiscMasterLookup appDataMiscLookup)
        {
            _repo = repo;
            _mapper = mapper;
            _ipAddress = ipAddress;
            _miscRepo = miscRepo;
            _outboxEventPublisher = outboxEventPublisher;
            _mediator = mediator;
            _logger = logger;
            _itemLookup = itemLookup;
            _partyLookup = partyLookup;
            _appDataMiscLookup = appDataMiscLookup;
        }

        public async Task<int> Handle(CreatePriceMasterCommand request, CancellationToken ct)
        {
            var d = request.Data;

            if (await _repo.HasOverlappingHeaderAsync(d.ItemId, d.VendorId, d.ValidFrom, d.ValidTo, ct))
                throw new InvalidOperationException("Another PriceMaster overlaps the given validity.");

            var header = _mapper.Map<PriceMasterHeader>(d);

            foreach (var tier in d.Details
                                  .OrderBy(x => x.ScaleQtyFrom)
                                  .ThenBy(x => x.ScaleQtyTo ?? decimal.MaxValue))
            {
                var detail = _mapper.Map<PriceMasterDetail>(tier);
                detail.IsActive = BaseEntity.Status.Active;
                detail.IsDeleted = BaseEntity.IsDelete.NotDeleted;
                header.Details.Add(detail);
            }

            var src = await _miscRepo.GetMiscMasterByName(
                MiscEnumEntity.SourceFrom,
                MiscEnumEntity.SourceFromDirect);

            var status = await _miscRepo.GetMiscMasterByName(
                MiscEnumEntity.ApprovalStatus,
                MiscEnumEntity.Pending);

            header.StatusId = status.Id;
            header.SourceFromId = src.Id;
            header.IsActive = BaseEntity.Status.Active;
            header.UnitId = _ipAddress.GetUnitId() ?? 0;

            await _repo.AddAsync(header, ct);
            await _repo.SaveChangesAsync(ct); // header.Id now set

            // ---- Reload aggregate with details for payload ----
            var agg = await _repo.LoadAggregateAsync(header.Id, ct)
                      ?? throw new InvalidOperationException($"PriceMaster {header.Id} not found after create.");
            try
            {
                var itemTask = _itemLookup.GetByIdsAsync(new[] { header.ItemId }, ct);
                var partyTask = _partyLookup.GetByIdsAsync(new[] { header.VendorId }, ct);

                await Task.WhenAll(itemTask, partyTask);

                var itemDto = itemTask.Result.FirstOrDefault();
                var partyDto = partyTask.Result.FirstOrDefault();

                // Prefer names, fall back to codes, then to the id string
                var itemName = !string.IsNullOrWhiteSpace(itemDto?.ItemName)
                               ? itemDto!.ItemName!
                               : (itemDto?.ItemCode ?? header.ItemId.ToString());

                var vendorName = !string.IsNullOrWhiteSpace(partyDto?.PartyName)
                                 ? partyDto!.PartyName!
                                 : (partyDto?.PartyCode ?? header.VendorId.ToString());

                // ---- Map to payload and publish workflow event ----
                var reversePayload = _mapper.Map<CreatePriceMasterReverseDto>(agg);
                var serializedPayload = JsonSerializer.Serialize(reversePayload);

               var correlationId = Guid.NewGuid();
               var wfEvent = new CreateApprovalRequestCommand
                {
                    CorrelationId = correlationId,
                    ModuleTypeName = MiscEnumEntity.PriceMaster,
                    ModuleTransactionId = header.Id,
                    Payload = serializedPayload
                }; 
                //Notification
                var notifEvent = await _appDataMiscLookup.GetMiscMasterByNameAsync(
                    NotificationEnum.NotificationEvent, NotificationEnum.Create);

                var notification = new NotificationCreatedEvent
                {
                    CorrelationId = correlationId,
                    CreatedByName = header.CreatedByName,
                    UnitId = _ipAddress.GetUnitId() ?? 0,
                    ModuleName = "PriceMaster",
                    EventTypeId = notifEvent.Id,
                    param1 = header.Id.ToString(),
                    param2 = itemName,
                    param3 = new DateTimeOffset(header.ValidFrom.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)),
                    param4 =vendorName
                };
                await _outboxEventPublisher.ScheduleAsync(wfEvent, correlationId, ct);
                await _outboxEventPublisher.ScheduleAsync(notification, correlationId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish NotificationCreatedEvent(s) for PriceMaster {Id}", header.Id);
            }
            // Optional audit
            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "PriceMaster.Create",
                actionName: header.Id.ToString(),
                details: $"PriceMaster created with {agg.Details.Count} tier(s). ItemId={agg.ItemId}, VendorId={agg.VendorId}, ValidFrom={agg.ValidFrom:yyyy-MM-dd}.",
                module: "PriceMaster"), ct);
            return header.Id;
        }
    }
}
