using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using MediatR;
using Microsoft.Extensions.Logging;

// ✅ use your existing custom exceptions namespace (adjust if different)
using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;

namespace PurchaseManagement.Application.Quotations.QuotationEntry.Commands.Update
{
    public class UpdateQuotationHandler : IRequestHandler<UpdateQuotationCommand, Unit>
    {
        private readonly IQuotationCommandRepository _repo;
        private readonly ILogger<UpdateQuotationHandler> _logger;
        private readonly IIPAddressService _ipAddressService;
        private readonly IUnitLookup _unitLookup;
        private readonly ICompanyLookup _companyLookup;

        public UpdateQuotationHandler(
            IQuotationCommandRepository repo,
            ILogger<UpdateQuotationHandler> logger,
            IIPAddressService ipAddressService,
            IUnitLookup unitLookup,
            ICompanyLookup companyLookup)
        {
            _repo = repo;
            _logger = logger;
            _ipAddressService = ipAddressService;
            _unitLookup = unitLookup;
            _companyLookup = companyLookup;
        }

        public async Task<Unit> Handle(UpdateQuotationCommand request, CancellationToken ct)
        {
            // 1) Load existing header + lines
            var header = await _repo.GetWithLinesAsync(request.Id, ct);
            if (header is null)
                throw new EntityNotFoundException(nameof(QuotationHeader), request.Id);

            // 2) Duplicate prevention
            var exists = await _repo.ExistsForSupplierRfqOtherAsync(request.Id, request.SupplierId, request.RfqId, ct);
            if (exists)
                throw new EntityAlreadyExistsException(
                    entity: nameof(QuotationHeader),
                    field: "SupplierId,RfqId",
                    value: $"{request.SupplierId},{request.RfqId}"
                );


            var status = request.IsActive == 1
                ? BaseEntity.Status.Active
                : BaseEntity.Status.Inactive;

            // 3) Update header fields
            header.UnitId          = _ipAddressService.GetUnitId();
            header.SupplierId      = request.SupplierId;
            header.RfqId           = request.RfqId;
            header.QuotationNumber = request.QuotationNumber;
            header.ValidTill       = request.ValidTill;
            header.FreightModeId   = request.FreightModeId;
            header.Freight         = request.Freight;
            header.PaymentTermsId  = request.PaymentTermsId;
            header.IncotermsId     = request.IncotermsId;
            header.InsuranceCharge = request.InsuranceCharge;
            header.TaxableSubtotal = request.TaxableSubtotal;
            header.GstTotal        = request.TaxableGst;
            header.ItemsTotal      = request.TaxableTotal;
            header.GrandTotal      = request.GrandTotal;
            header.QuotationImage  = request.QuotationImage;
            header.IsActive        = status;
            header.ModifiedDate    = DateTimeOffset.UtcNow;

            // 4) Replace lines
            header.Lines.Clear();

            foreach (var l in request.Lines)
            {
                header.Lines.Add(new QuotationDetail
                {
                    ItemId         = l.ItemId,
                    HsnId          = l.HsnId,
                    UomId          = l.UomId,
                    CurrencyId     = l.CurrencyId,
                    Quantity       = l.Quantity,
                    Rate           = l.Rate,
                    DiscountTypeId = l.DiscountTypeId,
                    Discount       = l.Discount,
                    PandFCharge    = l.PandFCharge,
                    GstPercent     = l.GstPercent,
                    Warranty       = l.Warranty,
                    ValidityDays   = l.ValidityDays,
                    DeliveryDays   = l.DeliveryDays,
                    LineSubtotal   = l.LineSubtotal,
                    GstAmount      = l.GstAmount,
                    Total          = l.Total,
                    IsActive       = status
                });
            }

            // 5) Save changes
            await _repo.SaveAsync(ct);

            // 6) Image move/rename (non-blocking)
            if (!string.IsNullOrWhiteSpace(request.QuotationImage))
            {
                try
                {
                    await MoveQuotationImageAsync(
                        quotationId: header.Id,
                        tempFilePath: request.QuotationImage!,
                        quotationNumber: request.QuotationNumber,
                        ct: ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to move/rename quotation image for QuotationId {QuotationId}, TempFile {TempFile}.",
                        header.Id, request.QuotationImage);
                }
            }

            return Unit.Value;
        }

        private async Task MoveQuotationImageAsync(
            int quotationId,
            string tempFilePath,
            string quotationNumber,
            CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tempFilePath))
                    return;

                var baseDirectory = await _repo.GetBaseDirectoryAsync();

                var units = await _unitLookup.GetAllUnitAsync();
                var companies = await _companyLookup.GetAllCompanyAsync();

                var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);
                var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);

                unitLookup.TryGetValue(_ipAddressService.GetUnitId(), out var unitName);
                companyLookup.TryGetValue(_ipAddressService.GetCompanyId(), out var companyName);

                var uploadPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Resources",
                    baseDirectory ?? string.Empty,
                    companyName ?? string.Empty,
                    unitName ?? string.Empty);

                var filePath = Path.Combine(uploadPath, tempFilePath);
                EnsureDirectoryExists(Path.GetDirectoryName(filePath));
                if (File.Exists(filePath))
                {
                    var newFile = $"{quotationNumber}{Path.GetExtension(filePath)}";
                    var newPath = Path.Combine(Path.GetDirectoryName(filePath)!, newFile);

                    File.Move(filePath, newPath);
                    await _repo.UpdateQuotationImageAsync(quotationId, newFile, ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to move/rename quotation image for QuotationId {QuotationId}, TempFile {TempFile}.",
                    quotationId, tempFilePath);
            }
            

         
        }

        private static void EnsureDirectoryExists(string? path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
