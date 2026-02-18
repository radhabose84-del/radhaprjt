#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.UpdateGRNEntry
{
    public class UpdateGRNEntryCommandHandler : IRequestHandler<UpdateGRNEntryCommand, bool>
    {
        private readonly IGRNEntryCommandRepository _iGrnEntryCommandRepository;
        private readonly IGRNEntryQueryRepository _igrnEntryQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;
        public UpdateGRNEntryCommandHandler(IGRNEntryCommandRepository iGrnEntryCommandRepository, IMapper mapper, IMediator mediator, IGRNEntryQueryRepository igrnEntryQueryRepository, IIPAddressService ipAddressService)
        {
            _iGrnEntryCommandRepository = iGrnEntryCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _igrnEntryQueryRepository = igrnEntryQueryRepository;
            _ipAddressService = ipAddressService;
        }

        public async Task<bool> Handle(UpdateGRNEntryCommand request, CancellationToken cancellationToken)
        {
             var headerDto = request.GrnEntryUpdate;
            var grnEntryHeader = _mapper.Map<GrnHeader>(headerDto);
            var calculatedDetails = new List<CalculatedDetail>();
            //decimal totalMiscCharges = 0;

            foreach (var detail in request.GrnEntryUpdate.UpdateGRNDetailsDtos)
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
               // totalMiscCharges += mischargesvalue;

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
                    UOMId = poVal.UOMId ?? 0,
                    UnitPrice = poVal.UnitPrice ?? 0,
                    GSTPercentage = poVal.GSTPercentage ?? 0,
                    DiscountValue = discountvalue,
                    ItemValue = itemValueoftax,
                    CGST = cgstValue,
                    SGST = sgstValue,
                    IGST = igstValue,
                    TaxableAmount = itemValueoftax + cgstValue + sgstValue + igstValue,
                    QcAcceptedQuantity = detail.QcAcceptedQuantity ?? 0,
                    QcRejectedQuantity = detail.QcRejectedQuantity ?? 0,
                    QcRejectedRemarks = detail.QcRejectedRemarks ?? string.Empty

                };

                // Add to header
                grnEntryHeader.GrnDetails ??= new List<GrnDetail>();
                grnEntryHeader.GrnDetails.Add(grnDetail);
                 // Store calculated fields separately
                calculatedDetails.Add(new CalculatedDetail
                {
                    Id = detail.Id,
                    CGST = cgstValue,
                    SGST = sgstValue,
                    IGST = igstValue,
                    ItemValue = itemValueoftax,
                    TaxableAmount = itemValueoftax + cgstValue + sgstValue + igstValue,
                    DiscountValue = discountvalue
                });
            }

            if (grnEntryHeader.GrnDetails != null && grnEntryHeader.GrnDetails.Any())
                {
                    grnEntryHeader.CGSTTotal = grnEntryHeader.GrnDetails.Sum(x => x.CGST);
                    grnEntryHeader.SGSTTotal = grnEntryHeader.GrnDetails.Sum(x => x.SGST);
                    grnEntryHeader.IGSTTotal = grnEntryHeader.GrnDetails.Sum(x => x.IGST);

                    grnEntryHeader.DiscountTotal = grnEntryHeader.GrnDetails.Sum(x => x.DiscountValue);
                    grnEntryHeader.ItemsTotal = grnEntryHeader.GrnDetails.Count;
                    grnEntryHeader.TaxableAmount = grnEntryHeader.GrnDetails.Sum(x => x.TaxableAmount);
                    //grnEntryHeader.MiscCharges = totalMiscCharges;

             
                    grnEntryHeader.PurchaseValue = grnEntryHeader.GrnDetails.Sum(x => x.ItemValue);
                    
                }

            if (!string.IsNullOrWhiteSpace(grnEntryHeader.RejectedImage))
            {
                string baseDirectory = await _igrnEntryQueryRepository.GetDocumentDirectoryAsync();
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
                EnsureDirectoryExists(uploadPath);

                string oldFilePath = Path.Combine(uploadPath, grnEntryHeader.RejectedImage);
                if (File.Exists(oldFilePath))
                {
                    // New filename format → GateEntryNo.ext
                    string newFileName = $"{grnEntryHeader.GrnNo}{Path.GetExtension(oldFilePath)}";
                    string newFilePath = Path.Combine(uploadPath, newFileName);

                    try
                    {
                        File.Move(oldFilePath, newFilePath, overwrite: true);
                        grnEntryHeader.RejectedImage = newFileName;

                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"File rename failed for '{grnEntryHeader.RejectedImage}' → '{newFileName}': {ex.Message}", ex);
                    }
                }
            }
            var result = await _iGrnEntryCommandRepository.UpdateAsync(grnEntryHeader.Id, grnEntryHeader,calculatedDetails,headerDto.UpdateGRNDetailsDtos);
            if (!result)
                throw new ExceptionRules("GRN update failed.");

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: grnEntryHeader.GrnNo ?? "NULL",
                actionName: grnEntryHeader.GrnDate.ToString() ?? "NULL",
                details: $"GRN Update details was created",
                module: "GRNUpdate");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
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