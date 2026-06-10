using PurchaseManagement.Application.Arrival.Commands.CreateArrival;
using PurchaseManagement.Application.Arrival.Commands.UpdateArrival;
using PurchaseManagement.Application.Arrival.Dto;
using PurchaseManagement.Domain.Entities.Arrival;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class ArrivalBuilders
    {
        public static CreateArrivalCommand ValidCreateCommand(
            int rawMaterialPoId = 1,
            string vehicleNumber = "TN-38-BC-4521",
            long baleFrom = 100001,
            long baleTo = 100150) =>
            new()
            {
                RawMaterialPOId = rawMaterialPoId,
                ArrivalDate = DateTimeOffset.UtcNow,
                VehicleNumber = vehicleNumber,
                SupplierId = 1,
                StationId = 1,
                GodownId = 1,
                TransporterId = 1,
                FreightRate = 1200m,
                GrossWeight = 30000m,
                TareWeight = 10000m,
                NetWeight = 20000m,
                PartyWeight = 19900m,
                WeightDifference = -100m,
                MoisturePercentage = 7.5m,
                Details = new List<CreateArrivalDetailDto>
                {
                    new()
                    {
                        ItemId = 1, HsnId = 1, PackTypeId = 1, MixCodeId = 1, UomId = 1,
                        Rate = 68500m, OrderedQty = 500m, ArrivedQty = 150m, CancelledQty = 0m,
                        BatchNumber = "BATCH-0012-A",
                        BaleNumberFrom = baleFrom, BaleNumberTo = baleTo,
                        TotalBaleCount = (int)(baleTo - baleFrom + 1)
                    }
                }
            };

        public static UpdateArrivalCommand ValidUpdateCommand(int id = 1, int isActive = 1) =>
            new()
            {
                Id = id,
                RawMaterialPOId = 1,
                ArrivalDate = DateTimeOffset.UtcNow,
                VehicleNumber = "TN-38-BC-4521",
                SupplierId = 1,
                StationId = 1,
                GodownId = 1,
                TransporterId = 1,
                GrossWeight = 30000m,
                TareWeight = 10000m,
                NetWeight = 20000m,
                PartyWeight = 19900m,
                WeightDifference = -100m,
                QcStatusId = 1,
                IsActive = isActive,
                Details = new List<UpdateArrivalDetailDto>
                {
                    new()
                    {
                        ItemId = 1, HsnId = 1, PackTypeId = 1, MixCodeId = 1, UomId = 1,
                        Rate = 68500m, OrderedQty = 500m, ArrivedQty = 150m, CancelledQty = 0m,
                        BatchNumber = "BATCH-0012-A",
                        BaleNumberFrom = 100001, BaleNumberTo = 100150, TotalBaleCount = 150
                    }
                }
            };

        public static ArrivalHeader ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                UnitId = 1,
                ArrivalNumber = "ARV-2025-0006",
                ArrivalDate = DateTimeOffset.UtcNow,
                RawMaterialPOId = 1,
                VehicleNumber = "TN-38-BC-4521",
                SupplierId = 1,
                StationId = 1,
                GodownId = 1,
                TransporterId = 1,
                GrossWeight = 30000m,
                TareWeight = 10000m,
                NetWeight = 20000m,
                PartyWeight = 19900m,
                WeightDifference = -100m,
                QcStatusId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                ArrivalDetails = new List<ArrivalDetail>
                {
                    new()
                    {
                        ItemId = 1, HsnId = 1, PackTypeId = 1, MixCodeId = 1, UomId = 1,
                        Rate = 68500m, OrderedQty = 500m, ArrivedQty = 150m, CancelledQty = 0m, BalanceQty = 350m,
                        BatchNumber = "BATCH-0012-A",
                        BaleNumberFrom = 100001, BaleNumberTo = 100150, TotalBaleCount = 150
                    }
                }
            };

        public static ArrivalDto ValidDto(int id = 1) =>
            new()
            {
                Id = id,
                UnitId = 1,
                ArrivalNumber = "ARV-2025-0006",
                ArrivalDate = DateTimeOffset.UtcNow,
                RawMaterialPOId = 1,
                PONumber = "PO-2025-0012",
                VehicleNumber = "TN-38-BC-4521",
                NetWeight = 20000m,
                PartyWeight = 19900m,
                WeightDifference = -100m,
                IsActive = true,
                IsDeleted = false,
                Details = new List<ArrivalDetailDto>
                {
                    new() { Id = 1, ArrivalHeaderId = id, ItemId = 1, BatchNumber = "BATCH-0012-A", TotalBaleCount = 150 }
                }
            };

        public static IReadOnlyList<ArrivalLookupDto> ValidLookupList() =>
            new List<ArrivalLookupDto>
            {
                new() { Id = 1, ArrivalNumber = "ARV-2025-0006" }
            };
    }
}
