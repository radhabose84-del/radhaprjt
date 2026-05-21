using Contracts.Common;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.CreateVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.UpdateVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.DeleteVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Dto;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.UnitTests.TestData
{
    public static class VehicleMovementRecordBuilders
    {
        public static CreateVehicleMovementRecordCommand ValidCreateCommand(
            string vehicleNumber = "KA01AB1234",
            string driverName = "John Doe",
            string driverMobileNo = "9876543210",
            int purposeOfVisitId = 1,
            int unitId = 1) =>
            new CreateVehicleMovementRecordCommand
            {
                VehicleNumber = vehicleNumber,
                DriverName = driverName,
                DriverLicenseNo = "DL1234567890",
                DriverMobileNo = driverMobileNo,
                TransporterId = 1,
                PurposeOfVisitId = purposeOfVisitId,
                ReceivingTypeId = 9, // Vehicle
                ReferenceDocTypeId = 1,
                ReferenceDocNo = "REF001",
                UnitId = unitId,
                Remarks = "Test vehicle entry"
            };

        public static UpdateVehicleMovementRecordCommand ValidUpdateCommand(
            int id = 1,
            string vehicleNumber = "KA01AB1234",
            string driverName = "John Doe Updated",
            int isActive = 1) =>
            new UpdateVehicleMovementRecordCommand
            {
                Id = id,
                VehicleNumber = vehicleNumber,
                DriverName = driverName,
                DriverLicenseNo = "DL1234567890",
                DriverMobileNo = "9876543210",
                TransporterId = 1,
                PurposeOfVisitId = 1,
                ReceivingTypeId = 9, // Vehicle
                ReferenceDocTypeId = 1,
                ReferenceDocNo = "REF001",
                Remarks = "Updated remarks",
                IsActive = isActive
            };

        public static VehicleMovementRecordDto ValidDto(
            int id = 1,
            string vehicleNumber = "KA01AB1234") =>
            new VehicleMovementRecordDto
            {
                Id = id,
                VehicleMovementId = "VMR00001",
                VehicleNumber = vehicleNumber,
                DriverName = "John Doe",
                DriverLicenseNo = "DL1234567890",
                DriverMobileNo = "9876543210",
                TransporterId = 1,
                TransporterName = "Test Transporter",
                PurposeOfVisitId = 1,
                PurposeOfVisitName = "Delivery",
                ReceivingTypeId = 9,
                ReceivingTypeName = "Vehicle",
                ReferenceDocTypeId = 1,
                ReferenceDocTypeName = "PO",
                ReferenceDocNo = "REF001",
                GateInTime = DateTimeOffset.UtcNow,
                GateInBy = "admin",
                StatusId = 1,
                StatusName = "Inside Premises",
                UnitId = 1,
                UnitName = "Unit 1",
                Remarks = "Test entry",
                IsActive = true,
                IsDeleted = false
            };

        public static VehicleMovementRecordAutoCompleteDto ValidAutoCompleteDto(int id = 1) =>
            new VehicleMovementRecordAutoCompleteDto
            {
                Id = id,
                VehicleMovementId = "VMR00001",
                VehicleNumber = "KA01AB1234",
                DriverName = "John Doe",
                StatusName = "Inside Premises"
            };

        public static PendingVehicleDto ValidPendingVehicleDto(int id = 1) =>
            new PendingVehicleDto
            {
                Id = id,
                VehicleMovementId = "VMR00001",
                VehicleNumber = "KA01AB1234",
                DriverName = "John Doe",
                DriverMobileNo = "9876543210",
                TransporterName = "Test Transporter",
                GateInTime = DateTimeOffset.UtcNow,
                StatusName = "Inside Premises"
            };

        public static GateEntryManagement.Domain.Entities.VehicleMovementRecord ValidEntity(int id = 1) =>
            new GateEntryManagement.Domain.Entities.VehicleMovementRecord
            {
                Id = id,
                VehicleMovementId = "VMR00001",
                VehicleNumber = "KA01AB1234",
                DriverName = "John Doe",
                DriverLicenseNo = "DL1234567890",
                DriverMobileNo = "9876543210",
                TransporterId = 1,
                PurposeOfVisitId = 1,
                ReceivingTypeId = 9,
                ReferenceDocTypeId = 1,
                ReferenceDocNo = "REF001",
                GateInTime = DateTimeOffset.UtcNow,
                GateInBy = "admin",
                StatusId = 1,
                UnitId = 1,
                Remarks = "Test entry",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
