using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.EInvoiceHeader
{
    public class EInvoiceHeaderCommandRepository : IEInvoiceHeaderCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public EInvoiceHeaderCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.EInvoiceHeader entity)
        {
            await _dbContext.EInvoiceHeader.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.EInvoiceHeader entity)
        {
            var existing = await _dbContext.EInvoiceHeader
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.UnitId = entity.UnitId;
            existing.DocType = entity.DocType;
            existing.SupplyType = entity.SupplyType;
            existing.InvoiceNo = entity.InvoiceNo;
            existing.InvoiceDate = entity.InvoiceDate;
            existing.PlaceOfSupply = entity.PlaceOfSupply;
            existing.IrnNumber = entity.IrnNumber;
            existing.AckNo = entity.AckNo;
            existing.AckDate = entity.AckDate;
            existing.SignInvoice = entity.SignInvoice;
            existing.SignQrCode = entity.SignQrCode;
            existing.IrnStatus = entity.IrnStatus;
            existing.ErrorCode = entity.ErrorCode;
            existing.ErrorMessage = entity.ErrorMessage;
            existing.PartyId = entity.PartyId;
            existing.GstNo = entity.GstNo;
            existing.ReverseCharge = entity.ReverseCharge;
            existing.CGST = entity.CGST;
            existing.SGST = entity.SGST;
            existing.IGST = entity.IGST;
            existing.Cess = entity.Cess;
            existing.StateCess = entity.StateCess;
            existing.TCS = entity.TCS;
            existing.Discount = entity.Discount;
            existing.OtherCharges = entity.OtherCharges;
            existing.RoundOff = entity.RoundOff;
            existing.InvoiceAmount = entity.InvoiceAmount;
            existing.Remarks = entity.Remarks;
            existing.StatusId = entity.StatusId;
            existing.EWaybillCreated = entity.EWaybillCreated;
            existing.IsActive = entity.IsActive;

            _dbContext.EInvoiceHeader.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.EInvoiceHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.EInvoiceHeader.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> UpdateIrnDetailsAsync(
            int id,
            string? irn,
            string? ackNo,
            DateTimeOffset? ackDate,
            string? signInvoice,
            string? signQrCode,
            string irnStatus,
            string? errorCode,
            string? errorMessage,
            CancellationToken ct)
        {
            var existing = await _dbContext.EInvoiceHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IrnNumber = irn;
            existing.AckNo = ackNo;
            existing.AckDate = ackDate;
            existing.SignInvoice = signInvoice;
            existing.SignQrCode = signQrCode;
            existing.IrnStatus = irnStatus;
            existing.ErrorCode = errorCode;
            existing.ErrorMessage = errorMessage;

            _dbContext.EInvoiceHeader.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
        public async Task<bool> UpdateIrnStatusAsync(
            int id, string irnStatus, string? errorCode, string? errorMessage, CancellationToken ct)
        {
            var existing = await _dbContext.EInvoiceHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IrnStatus = irnStatus;
            existing.ErrorCode = errorCode;
            existing.ErrorMessage = errorMessage;

            _dbContext.EInvoiceHeader.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> UpdateEwbDetailsAsync(
            int id,
            long? ewbNo,
            string? ewbDate,
            string? ewbValidTill,
            CancellationToken ct)
        {
            var existing = await _dbContext.EInvoiceHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.EWaybillCreated = true;

            // Create EWaybillHeader record with NIC response data
            var ewbHeader = new Domain.Entities.EWaybillHeader
            {
                EInvoiceHeaderId = id,
                UnitId = existing.UnitId,
                EWBNumber = ewbNo?.ToString(),
                InvoiceNo = existing.InvoiceNo,
                InvoiceDate = existing.InvoiceDate,
                InvoiceValue = existing.InvoiceAmount,
                FromGSTIN = existing.GstNo,
                EwbStatus = "Generated",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            // Parse EWB dates from NIC response format
            if (!string.IsNullOrEmpty(ewbDate) && DateTimeOffset.TryParse(ewbDate, out var genDate))
                ewbHeader.GeneratedDate = genDate;

            if (!string.IsNullOrEmpty(ewbValidTill) && DateTimeOffset.TryParse(ewbValidTill, out var validDate))
                ewbHeader.ValidUpto = validDate;

            await _dbContext.EWaybillHeader.AddAsync(ewbHeader, ct);
            _dbContext.EInvoiceHeader.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
