using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using MediatR;
using Microsoft.Extensions.Logging;
using Contracts.Interfaces.Lookups.Users;

namespace PurchaseManagement.Application.Quotations.QuotationEntry.Commands.Create;

public class CreateQuotationHandler : IRequestHandler<CreateQuotationCommand, int>
{
    private readonly IQuotationCommandRepository _repo;
    private readonly ILogger<CreateQuotationHandler> _logger;
    private readonly IIPAddressService _ipAddressService;
    private readonly IUnitLookup _unitLookup;
    private readonly ICompanyLookup _companyLookup;

    public CreateQuotationHandler(
        IQuotationCommandRepository repo,
        ILogger<CreateQuotationHandler> logger,
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

    public async Task<int> Handle(CreateQuotationCommand request, CancellationToken ct)
    {
        // Duplicate prevention: Supplier + RFQ
        if (await _repo.ExistsForSupplierRfqAsync(request.SupplierId, request.RfqId, ct))
        {
            // Prefer throwing your standard exception so middleware returns proper error response
            throw new InvalidOperationException("A quotation already exists for this Supplier and RFQ.");
            // OR if you already have: throw new EntityAlreadyExistsException("Quotation", ...);
        }

        var header = new QuotationHeader
        {
            UnitId          = _ipAddressService.GetUnitId(),
            SupplierId      = request.SupplierId,
            RfqId           = request.RfqId,
            QuotationNumber = request.QuotationNumber,
            ValidTill       = request.ValidTill,
            FreightModeId   = request.FreightModeId,
            Freight         = request.Freight,
            PaymentTermsId  = request.PaymentTermsId,
            IncotermsId     = request.IncotermsId,
            InsuranceCharge = request.InsuranceCharge,
            TaxableSubtotal = request.TaxableSubtotal,
            GstTotal        = request.TaxableGst,
            ItemsTotal      = request.TaxableTotal,
            GrandTotal      = request.GrandTotal,
            QuotationImage  = request.QuotationImage
        };

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
                Total          = l.Total
            });
        }

        await _repo.AddAsync(header, ct);
        await _repo.SaveAsync(ct);

        if (header.Id <= 0)
            throw new Exception("Quotation was not created.");

        // Move / rename image (non-blocking)
        if (!string.IsNullOrWhiteSpace(request.QuotationImage))
        {
            try
            {
                await MoveQuotationImageAsync(header.Id, request.QuotationImage!, request.QuotationNumber, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to move/rename quotation image for QuotationId {QuotationId}.", header.Id);
            }
        }

        return header.Id;
    }

    private async Task MoveQuotationImageAsync(int quotationId, string tempFilePath, string quotationNumber, CancellationToken ct)
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

                File.Move(filePath, newPath, overwrite: true);
                await _repo.UpdateQuotationImageAsync(quotationId, newFile, ct);
            }           
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move/rename quotation image for QuotationId {QuotationId}.", quotationId);
        }
    }

    private static void EnsureDirectoryExists(string? path)
    {
        if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}
