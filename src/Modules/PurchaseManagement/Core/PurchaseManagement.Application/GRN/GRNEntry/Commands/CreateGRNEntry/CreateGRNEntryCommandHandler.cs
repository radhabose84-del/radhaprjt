#nullable disable
using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands
{
    public class CreateGRNEntryCommandHandler : IRequestHandler<CreateGRNEntryCommand, int>
    {
        private readonly IGRNEntryCommandRepository _iGrnEntryCommandRepository;
        private readonly IGRNEntryQueryRepository _igrnEntryQueryRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;

        public CreateGRNEntryCommandHandler(
            IGRNEntryCommandRepository iGrnEntryCommandRepository,
            IMapper mapper,
            IMediator mediator,
            IGRNEntryQueryRepository igrnEntryQueryRepository,
            IIPAddressService ipAddressService,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _iGrnEntryCommandRepository = iGrnEntryCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _igrnEntryQueryRepository = igrnEntryQueryRepository;
            _ipAddressService = ipAddressService;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> Handle(CreateGRNEntryCommand request, CancellationToken cancellationToken)
        {
            var grnEntryHeader = _mapper.Map<GrnHeader>(request.GrnEntryCreate);

            // Resolve the Finance.TransactionTypeMaster row for this unit so we can both produce
            // the next GrnNo and advance the DocNo atomically inside the repo's transaction.
            var unitId = _ipAddressService.GetUnitId() ?? request.GrnEntryCreate.UnitId;
            var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeGRN, MiscEnumEntity.ModulePurchase, unitId);

            if (!transactionTypeId.HasValue)
                throw new ExceptionRules($"Transaction Type '{MiscEnumEntity.TransactionTypeGRN}' not configured for Purchase module (Unit {unitId}).");

            // Generate GrnNo via Finance.DocumentSequence (per-unit, per-financial-year)
            // — only when the caller hasn't supplied one (Gate Inward bridge sends a blank).
            if (string.IsNullOrWhiteSpace(grnEntryHeader.GrnNo))
            {
                var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId.Value);
                if (sequences.Count == 0)
                    throw new ExceptionRules($"No document sequence configured for '{MiscEnumEntity.TransactionTypeGRN}'.");

                grnEntryHeader.GrnNo = sequences[^1];
                grnEntryHeader.GrnDate = DateTime.Now;
                grnEntryHeader.CreatedBy = _ipAddressService.GetUserId();
                grnEntryHeader.CreatedDate = DateTime.Now;
                grnEntryHeader.CreatedByName = _ipAddressService.GetUserName();
                grnEntryHeader.CreatedIP = _ipAddressService.GetSystemIPAddress();
            }
              decimal totalMiscCharges = 0;

            foreach (var detail in request.GrnEntryCreate.GRNDetailsDtos)
            {
                // Fetch PO values for this item
                var poList = await _igrnEntryQueryRepository.GetPoOtherDetails(
                                    detail.PoId,
                                    detail.PoSlNoLocal ?? 0,
                                    detail.PoCategoryId,
                                    detail.PoMethodId,
                                    detail.ItemId);

                var poVal = poList.FirstOrDefault();
                if (poVal == null)
                    continue;

                // ========== GET VALUES ==========
                decimal qty = detail.DcQuantity;
                decimal unitPrice = poVal.UnitPrice ?? 0;
                decimal itemvalue = qty * unitPrice;
                decimal discount = poVal.DiscountValue ?? 0;
                decimal mischarges = poVal.MiscCharges ?? 0;
                decimal perQtyDiscount =
                     (detail.DcQuantity > 0)
                         ? decimal.Round(discount / detail.OrderQuantity, 2)
                         : 0;

                decimal perQtyMischarges =
                     (detail.DcQuantity > 0)
                         ? decimal.Round(mischarges / detail.OrderQuantity, 2)
                         : 0;

                decimal discountvalue = perQtyDiscount * detail.DcQuantity;
                decimal mischargesvalue = perQtyMischarges * detail.DcQuantity;

                // Accumulate into total
                totalMiscCharges += mischargesvalue;

                decimal CGSTPercentage = poVal.CGSTPercentage ?? 0;

                decimal SGSTPercentage = poVal.SGSTPercentage ?? 0;
                decimal IGSTPercentage = poVal.IGSTPercentage ?? 0;

                // ItemValue after discount
                decimal itemValueoftax = itemvalue - discountvalue;

                decimal cgstValue = CalcTax(itemValueoftax, CGSTPercentage);
                decimal sgstValue = CalcTax(itemValueoftax, SGSTPercentage);
                decimal igstValue = CalcTax(itemValueoftax, IGSTPercentage);
                var grnDetail = new GrnDetail
                {
                    PoId = detail.PoId,
                    PoSlNoLocal = detail.PoSlNoLocal ?? 0,
                    PoCategoryId = detail.PoCategoryId,
                    PoMethodId = detail.PoMethodId,
                    ItemId = detail.ItemId,
                    OrderQuantity = detail.OrderQuantity,
                    DcQuantity = detail.DcQuantity,
                    ReceivedQuantity = detail.ReceivedQuantity,
                    ExpiryDate = detail.ExpiryDate,
                    BatchNumber = detail.BatchNumber,
                    GrnDetailImage = detail.GrnDetailImage,
                    UOMId = poVal.UOMId ?? 0,
                    UnitPrice = poVal.UnitPrice ?? 0,
                    GSTPercentage = poVal.GSTPercentage ?? 0,
                    DiscountValue = discountvalue,
                    ItemValue = itemValueoftax,
                    CGST = cgstValue,
                    SGST = sgstValue,
                    IGST = igstValue,
                    TaxableAmount = itemValueoftax + cgstValue + sgstValue + igstValue
                };

                // Add to header
                grnEntryHeader.GrnDetails ??= new List<GrnDetail>();
                grnEntryHeader.GrnDetails.Add(grnDetail);
            }
                
                 if (grnEntryHeader.GrnDetails != null && grnEntryHeader.GrnDetails.Any())
                {
                    grnEntryHeader.CGSTTotal = grnEntryHeader.GrnDetails.Sum(x => x.CGST);
                    grnEntryHeader.SGSTTotal = grnEntryHeader.GrnDetails.Sum(x => x.SGST);
                    grnEntryHeader.IGSTTotal = grnEntryHeader.GrnDetails.Sum(x => x.IGST);

                    grnEntryHeader.DiscountTotal = grnEntryHeader.GrnDetails.Sum(x => x.DiscountValue);
                    grnEntryHeader.ItemsTotal = grnEntryHeader.GrnDetails.Count;
                    grnEntryHeader.TaxableAmount = grnEntryHeader.GrnDetails.Sum(x => x.TaxableAmount);
                    grnEntryHeader.MiscCharges = totalMiscCharges;
                    grnEntryHeader.PurchaseValue =grnEntryHeader.GrnDetails.Sum(x => x.ItemValue);
                       
                    
                }



                if (!string.IsNullOrWhiteSpace(grnEntryHeader.GrnReceivedImage))
            {
                string baseDirectory = await _igrnEntryQueryRepository.GetDocumentDirectoryAsync();
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
                EnsureDirectoryExists(uploadPath);

                string oldFilePath = Path.Combine(uploadPath, grnEntryHeader.GrnReceivedImage);
                if (File.Exists(oldFilePath))
                {
                    // New filename format → GateEntryNo.ext
                    string newFileName = $"{grnEntryHeader.GrnNo}{Path.GetExtension(oldFilePath)}";
                    string newFilePath = Path.Combine(uploadPath, newFileName);

                    try
                    {
                        File.Move(oldFilePath, newFilePath, overwrite: true);
                        grnEntryHeader.GrnReceivedImage = newFileName;

                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"File rename failed for '{grnEntryHeader.GrnReceivedImage}' → '{newFileName}': {ex.Message}", ex);
                    }
                }
            }

            // ✅ Finalize per-line-item images → <GrnNo>_L<lineNo>.<ext>
            if (grnEntryHeader.GrnDetails != null
                && grnEntryHeader.GrnDetails.Any(d => !string.IsNullOrWhiteSpace(d.GrnDetailImage)))
            {
                string detailBaseDirectory = await _igrnEntryQueryRepository.GetDocumentDirectoryAsync();
                string detailUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", detailBaseDirectory);
                EnsureDirectoryExists(detailUploadPath);

                int lineNo = 0;
                foreach (var detail in grnEntryHeader.GrnDetails)
                {
                    lineNo++;
                    if (string.IsNullOrWhiteSpace(detail.GrnDetailImage)
                        || !detail.GrnDetailImage.StartsWith("TEMP_"))
                        continue;

                    string oldDetailFilePath = Path.Combine(detailUploadPath, detail.GrnDetailImage);
                    if (!File.Exists(oldDetailFilePath))
                        continue;

                    string newDetailFileName = $"{grnEntryHeader.GrnNo}_L{lineNo}{Path.GetExtension(oldDetailFilePath)}";
                    string newDetailFilePath = Path.Combine(detailUploadPath, newDetailFileName);

                    try
                    {
                        File.Move(oldDetailFilePath, newDetailFilePath, overwrite: true);
                        detail.GrnDetailImage = newDetailFileName;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"File rename failed for '{detail.GrnDetailImage}' → '{newDetailFileName}': {ex.Message}", ex);
                    }
                }
            }

            var result = await _iGrnEntryCommandRepository.CreateAsync(grnEntryHeader, transactionTypeId.Value, cancellationToken);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: grnEntryHeader.GrnNo ?? "NULL",
                actionName: grnEntryHeader.GrnDate.ToString() ?? "NULL",
                details: $"GRN details was created",
                module: "GRNEntry");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result > 0 ? result : throw new ExceptionRules("GRNEntry Creation Failed.");
        }
        private void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        decimal CalcTax(decimal baseValue, decimal per)
        {
            if (baseValue <= 0 || per <= 0)
                return 0;

            return Math.Round((baseValue * per) / 100, 2, MidpointRounding.AwayFromZero);
        }
       
  
    }
}