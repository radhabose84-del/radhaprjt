using PurchaseManagement.Application.FreightRfq.Commands.CreateFreightRfq;
using PurchaseManagement.Application.FreightRfq.Commands.SaveFreightRfqQuotations;
using PurchaseManagement.Application.FreightRfq.Commands.UpdateFreightRfq;
using PurchaseManagement.Application.FreightRfq.Dto;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class FreightRfqBuilders
    {
        public static readonly DateTimeOffset RfqDate = new(2026, 6, 6, 0, 0, 0, TimeSpan.Zero);

        public static CreateFreightRfqCommand ValidCreateCommand(
            int rfqTypeId = 1,
            int? poReferenceId = 10,
            int? supplierId = 5,
            decimal totalQuantity = 120.5m,
            int totalBaleCount = 700) =>
            new()
            {
                RfqDate = RfqDate,
                RfqTypeId = rfqTypeId,
                PoReferenceId = poReferenceId,
                SupplierId = supplierId,
                SourceLocation = "Adilabad",
                SourceStation = "Adilabad Yard",
                DestinationLocation = "Dindigul",
                DestinationStation = "Dindigul Mill Gate",
                TotalQuantity = totalQuantity,
                TotalBaleCount = totalBaleCount
            };

        public static UpdateFreightRfqCommand ValidUpdateCommand(
            int id = 1,
            int rfqTypeId = 1,
            int isActive = 1) =>
            new()
            {
                Id = id,
                RfqTypeId = rfqTypeId,
                PoReferenceId = 10,
                SupplierId = 5,
                SourceLocation = "Adilabad",
                SourceStation = "Adilabad Yard",
                DestinationLocation = "Dindigul",
                DestinationStation = "Dindigul Mill Gate",
                TotalQuantity = 120.5m,
                TotalBaleCount = 700,
                IsActive = isActive
            };

        public static SaveFreightRfqQuotationsCommand ValidSaveQuotationsCommand(int rfqId = 1) =>
            new()
            {
                FreightRfqId = rfqId,
                Quotations = new List<FreightRfqQuotationInputDto>
                {
                    new() { TransporterId = 101, RateBasisId = 21, QuotedRate = 5000m, Remarks = "Open trucks" },
                    new() { TransporterId = 102, RateBasisId = 21, QuotedRate = 2000m, Remarks = "Cheapest" }
                }
            };

        public static PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                FreightRfqNumber = "FRFQ-2026-0005",
                RfqDate = RfqDate,
                RfqTypeId = 1,
                PoReferenceId = 10,
                SupplierId = 5,
                SourceLocation = "Adilabad",
                SourceStation = "Adilabad Yard",
                DestinationLocation = "Dindigul",
                DestinationStation = "Dindigul Mill Gate",
                TotalQuantity = 120.5m,
                TotalBaleCount = 700,
                StatusId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static FreightRfqDto ValidDto(int id = 1) =>
            new()
            {
                Id = id,
                FreightRfqNumber = "FRFQ-2026-0005",
                RfqDate = RfqDate,
                RfqTypeId = 1,
                RfqTypeName = "PO Based",
                TotalQuantity = 120.5m,
                TotalBaleCount = 700,
                StatusId = 1,
                StatusName = "Draft",
                IsActive = true,
                Quotations = new List<FreightRfqQuotationDto>()
            };

        public static FreightRfqListDto ValidListDto(int id = 1) =>
            new()
            {
                Id = id,
                FreightRfqNumber = "FRFQ-2026-0005",
                RfqDate = RfqDate,
                RfqTypeName = "PO Based",
                Route = "Adilabad Yard → Dindigul Mill Gate",
                QuotesCount = 3,
                StatusId = 1,
                StatusName = "Draft"
            };
    }
}
