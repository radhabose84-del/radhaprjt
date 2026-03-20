using AutoMapper;
using Contracts.Dtos.Inventory;
using Contracts.Dtos.Stock;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IIssue;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Issue;
using PurchaseManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Contracts.Interfaces.Lookups.Inventory;

namespace PurchaseManagement.Application.Issue.Command.CreateIssueEntry
{
    public class CreateIssueEntryCommandHandler : IRequestHandler<CreateIssueEntryCommand, int>
    {
        private readonly IIssueEntryCommandRepository _iissueEntryCommandRepository;
        private readonly IIssueQueryCommandRepository _iissueQueryCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;
        private readonly IPutawayRuleLookup _putawayRuleLookup;
        private readonly IStockLedgerLookup _stockLedgerLookup;
        private readonly ILogger<CreateIssueEntryCommandHandler> _logger;

        public CreateIssueEntryCommandHandler(IIssueEntryCommandRepository iissueEntryCommandRepository,
            IIssueQueryCommandRepository iissueQueryCommandRepository, IMapper mapper, IMediator mediator,
            IIPAddressService ipAddressService, IPutawayRuleLookup putawayRuleLookup,
            IStockLedgerLookup stockLedgerLookup, ILogger<CreateIssueEntryCommandHandler> logger)
        {
            _iissueEntryCommandRepository = iissueEntryCommandRepository;
            _iissueQueryCommandRepository = iissueQueryCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
            _putawayRuleLookup = putawayRuleLookup;
            _stockLedgerLookup = stockLedgerLookup;
            _logger = logger;
        }
        

            public async Task<int> Handle(
                CreateIssueEntryCommand request,
                CancellationToken cancellationToken)
            {
                // -----------------------------
                // HEADER
                // -----------------------------
                var issueEntryHeader = _mapper.Map<IssueHeader>(request.IssueEntry);

                if (string.IsNullOrWhiteSpace(issueEntryHeader.IssueNo))
                {
                    issueEntryHeader.IssueNo = await _iissueEntryCommandRepository.GenerateNextCodeAsync();
                    issueEntryHeader.IssueDate = DateTime.Today;
                    issueEntryHeader.IssuedBy = _ipAddressService.GetUserId();
                    issueEntryHeader.IssuedDate = DateTime.Now;
                    issueEntryHeader.IssuedByName = _ipAddressService.GetUserName();
                    issueEntryHeader.IssuedIp = _ipAddressService.GetSystemIPAddress();
                }

                // -----------------------------
                // CATEGORY CHECK
                // -----------------------------
                var miscDescription = await _iissueQueryCommandRepository
                    .GetDescriptionByIdAsync(request.IssueEntry.RequestCategoryId);

                List<PutawayRuleDto> putawayRules = new();

                if (string.Equals(
                    miscDescription,
                    MiscEnumEntity.SubStores,
                    StringComparison.OrdinalIgnoreCase))
                {
                    var itemIds = request.IssueEntry.IssueDetails
                        .Select(x => x.ItemId)
                        .Distinct()
                        .ToList();

                    var warehouseIds = new List<int>
                    {
                        request.IssueEntry.SubStoresWarehouseId ?? 0
                    };

                    putawayRules = await _putawayRuleLookup
                        .GetPutAwayRuleDetailsByWarehouseAsync(
                            itemIds,
                            warehouseIds,
                            cancellationToken);
                }

                int issueId;

                try
                {
                    // -----------------------------
                    // SAVE ISSUE HEADER ONLY
                    // -----------------------------
                    issueId = await _iissueEntryCommandRepository
                        .CreateIssueAsync(issueEntryHeader);
                }
                catch (Exception ex)
                {
                    
                    _logger.LogError(ex, "Issue header creation failed");
                    throw;
                }

                

                // -----------------------------
            // BUILD STOCK LEDGER DTOs
            // -----------------------------
            var stockLedgerDtos = new List<StockLedgerDto>();
                var subStoreLedgerDtos = new List<SubStoreStockLedgerDto>();

                foreach (var detail in request.IssueEntry.IssueDetails)
                {
                    // ---------- STOCK LEDGER (ISSUE) ----------
                    stockLedgerDtos.Add(new StockLedgerDto
                    {
                        UnitId = issueEntryHeader.UnitId,
                        DocType = "ISS",
                        DocNo = issueId,
                        DocSlNo = detail.Sno,
                        DocDate = DateTime.Today,
                        ItemId = detail.ItemId,
                        UomId = detail.UomId,
                        WarehouseId = detail.WarehouseStockId,
                        StorageTypeId = detail.StorageTypeId,
                        TargetId = detail.TargetId,
                        ReceivedQty = 0,
                        ReceivedValue = 0,
                        IssueQty = detail.IssueQuantity ?? 0,
                        IssueValue = (detail.IssueQuantity ?? 0) * detail.AvgRate
                    });

                    // ---------- SUBSTORE LEDGER (RECEIVE) ----------
                    if (string.Equals(
                        miscDescription,
                        MiscEnumEntity.SubStores,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        var rule = putawayRules
                            .FirstOrDefault(r => r.ItemId == detail.ItemId);

                        subStoreLedgerDtos.Add(new SubStoreStockLedgerDto
                        {
                            UnitId = issueEntryHeader.UnitId,
                            DocType = "REC",
                            DocNo = issueId,
                            DocSlNo = detail.Sno,
                            DocDate = DateTime.Today,
                            DepartmentId = request.IssueEntry.DepartmentId,
                            ItemId = detail.ItemId,
                            UomId = detail.UomId,
                            WarehouseId = issueEntryHeader.SubStoresWarehouseId ?? 0,
                            StorageTypeId = rule?.StorageTypeId ?? 0,
                            TargetId = rule?.TargetId ?? 0,
                            ReceivedQty = detail.IssueQuantity ?? 0,
                            ReceivedValue = (detail.IssueQuantity ?? 0) * detail.AvgRate,
                            IssueQty = 0,
                            IssueValue = 0
                        });
                    }
                }

                // -----------------------------
                // STOCK LEDGER INSERT
                // -----------------------------
                var stockResult = await _stockLedgerLookup
                    .InsertStockLedgerAsync(stockLedgerDtos, cancellationToken);

                if (!stockResult)
                    throw new ApplicationException("StockLedger insert failed");

                if (subStoreLedgerDtos.Any())
                {
                    var subStoreResult = await _stockLedgerLookup
                        .InsertSubStoreStockLedgerAsync(subStoreLedgerDtos, cancellationToken);

                    if (!subStoreResult)
                        throw new ApplicationException("SubStoreStockLedger insert failed");
                }

                // -----------------------------
                // AUDIT EVENT
                // -----------------------------
                await _mediator.Publish(
                    new AuditLogsDomainEvent(
                        "Create",
                        issueId.ToString(),
                        "Issue Entry Created",
                        "Issue header saved and ledgers inserted successfully",
                        "Issue"),
                    cancellationToken);

                return issueId;
            }
    }
}

