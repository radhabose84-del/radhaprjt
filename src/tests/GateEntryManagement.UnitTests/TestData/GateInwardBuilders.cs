using Contracts.Common;
using GateEntryManagement.Application.GateInward.Commands.CreateGateInward;
using GateEntryManagement.Application.GateInward.Commands.DeleteGateInward;
using GateEntryManagement.Application.GateInward.Dto;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.UnitTests.TestData
{
    public static class GateInwardBuilders
    {
        public static CreateGateInwardCommand ValidCreateCommand(
            int vehicleMovementRecordId = 1,
            int unitId = 1) =>
            new CreateGateInwardCommand
            {
                VehicleMovementRecordId = vehicleMovementRecordId,
                GrossWeight = 1000m,
                TareWeight = 200m,
                QAInspectionRequired = false,
                QAStatusId = null,
                UnitId = unitId,
                Remarks = "Test gate inward",
                GateInwardDetails = new List<CreateGateInwardDetailDto>
                {
                    new CreateGateInwardDetailDto
                    {
                        ReferenceDocTypeId = 1,
                        ReferenceDocNo = "PO001",
                        PartyName = "Test Party"
                    }
                }
            };

        public static GateInwardHdrDto ValidDto(int id = 1) =>
            new GateInwardHdrDto
            {
                Id = id,
                GateEntryNo = "GE00001",
                VehicleMovementRecordId = 1,
                VehicleMovementId = "VMR00001",
                VehicleNumber = "KA01AB1234",
                DriverName = "John Doe",
                GrossWeight = 1000m,
                TareWeight = 200m,
                NetWeight = 800m,
                QAInspectionRequired = false,
                UnitId = 1,
                UnitName = "Unit 1",
                Remarks = "Test gate inward",
                IsActive = true,
                IsDeleted = false,
                GateInwardDetails = new List<GateInwardDtlDto>
                {
                    new GateInwardDtlDto
                    {
                        Id = 1,
                        GateInwardHdrId = id,
                        ReferenceDocTypeId = 1,
                        ReferenceDocNo = "PO001",
                        PartyName = "Test Party"
                    }
                }
            };

        public static GateInwardAutoCompleteDto ValidAutoCompleteDto(int id = 1) =>
            new GateInwardAutoCompleteDto
            {
                Id = id,
                GateEntryNo = "GE00001",
                VehicleNumber = "KA01AB1234",
                DriverName = "John Doe"
            };

        public static GateEntryManagement.Domain.Entities.GateInwardHdr ValidEntity(int id = 1) =>
            new GateEntryManagement.Domain.Entities.GateInwardHdr
            {
                Id = id,
                GateEntryNo = "GE00001",
                VehicleMovementRecordId = 1,
                GrossWeight = 1000m,
                TareWeight = 200m,
                NetWeight = 800m,
                QAInspectionRequired = false,
                UnitId = 1,
                Remarks = "Test gate inward",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
