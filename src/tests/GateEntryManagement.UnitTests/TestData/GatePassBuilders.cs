using Contracts.Common;
using GateEntryManagement.Application.GatePass.Commands.CreateGatePass;
using GateEntryManagement.Application.GatePass.Commands.DeleteGatePass;
using GateEntryManagement.Application.GatePass.Dto;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.UnitTests.TestData
{
    public static class GatePassBuilders
    {
        public static CreateGatePassCommand ValidCreateCommand(
            int vehicleMovementRecordId = 1,
            int unitId = 1) =>
            new CreateGatePassCommand
            {
                GatePassDate = DateOnly.FromDateTime(DateTime.Today),
                VehicleMovementRecordId = vehicleMovementRecordId,
                VehicleNumber = "KA01AB1234",
                DriverName = "John Doe",
                DriverMobile = "9876543210",
                TransporterName = "Test Transporter",
                UnitId = unitId,
                TotalItems = 5,
                TotalDocumentQty = 100m,
                TotalDispatchQty = 100m,
                ReturnableItems = 0,
                TotalValue = 50000m,
                Remarks = "Test gate pass",
                GatePassDetails = new List<CreateGatePassDetailDto>
                {
                    new CreateGatePassDetailDto
                    {
                        DocTypeId = 1,
                        DocId = 1,
                        DocNo = "PO001",
                        PartyName = "Test Party",
                        PartyCode = "P001",
                        DocDate = DateOnly.FromDateTime(DateTime.Today),
                        TotalQty = 100m
                    }
                }
            };

        public static GatePassHdrDto ValidDto(int id = 1) =>
            new GatePassHdrDto
            {
                Id = id,
                GatePassNo = "GP00001",
                GatePassDate = DateOnly.FromDateTime(DateTime.Today),
                VehicleMovementRecordId = 1,
                VehicleMovementId = "VMR00001",
                VehicleNumber = "KA01AB1234",
                DriverName = "John Doe",
                DriverMobile = "9876543210",
                TransporterName = "Test Transporter",
                UnitId = 1,
                UnitName = "Unit 1",
                TotalItems = 5,
                TotalDocumentQty = 100m,
                TotalDispatchQty = 100m,
                ReturnableItems = 0,
                TotalValue = 50000m,
                Remarks = "Test gate pass",
                IsActive = true,
                IsDeleted = false,
                GatePassDetails = new List<GatePassDtlDto>
                {
                    new GatePassDtlDto
                    {
                        Id = 1,
                        GatePassHdrId = id,
                        DocTypeId = 1,
                        DocId = 1,
                        DocNo = "PO001",
                        PartyName = "Test Party",
                        PartyCode = "P001",
                        DocDate = DateOnly.FromDateTime(DateTime.Today),
                        TotalQty = 100m
                    }
                }
            };

        public static GatePassAutoCompleteDto ValidAutoCompleteDto(int id = 1) =>
            new GatePassAutoCompleteDto
            {
                Id = id,
                GatePassNo = "GP00001",
                GatePassDate = DateOnly.FromDateTime(DateTime.Today),
                VehicleNumber = "KA01AB1234",
                DriverName = "John Doe"
            };

        public static GateEntryManagement.Domain.Entities.GatePassHdr ValidEntity(int id = 1) =>
            new GateEntryManagement.Domain.Entities.GatePassHdr
            {
                Id = id,
                GatePassNo = "GP00001",
                GatePassDate = DateOnly.FromDateTime(DateTime.Today),
                VehicleMovementRecordId = 1,
                VehicleNumber = "KA01AB1234",
                DriverName = "John Doe",
                DriverMobile = "9876543210",
                TransporterName = "Test Transporter",
                UnitId = 1,
                TotalItems = 5,
                TotalDocumentQty = 100m,
                TotalDispatchQty = 100m,
                ReturnableItems = 0,
                TotalValue = 50000m,
                Remarks = "Test gate pass",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
