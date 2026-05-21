using FluentValidation.TestHelper;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.CreateVehicleMovementRecord;
using GateEntryManagement.Presentation.Validation.Common;
using GateEntryManagement.Presentation.Validation.VehicleMovementRecord;

namespace GateEntryManagement.UnitTests.Validators.VehicleMovementRecord
{
    public sealed class CreateVehicleMovementRecordCommandValidatorTests
    {
        private readonly Mock<IVehicleMovementRecordQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateVehicleMovementRecordCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null), _mockQueryRepo.Object);

        private static CreateVehicleMovementRecordCommand ValidCommand() =>
            new()
            {
                VehicleNumber = "KA01AB1234",
                DriverName = "Test Driver",
                DriverMobileNo = "9876543210",
                DriverLicenseNo = "DL12345",
                PurposeOfVisitId = 1,
                ReceivingTypeId = 9, // Vehicle
                ReferenceDocTypeId = null,
                ReferenceDocNo = null,
                TransporterId = null,
                UnitId = 1,
                Remarks = null
            };

        private void SetupAllAsyncMocks(
            int purposeOfVisitId = 1,
            int unitId = 1,
            string vehicleNumber = "KA01AB1234",
            int receivingTypeId = 9,
            bool isVehicleReceivingType = true)
        {
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(purposeOfVisitId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(receivingTypeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(unitId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.HasOpenVMRForVehicleAsync(vehicleNumber)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsVehicleReceivingTypeAsync(receivingTypeId)).ReturnsAsync(isVehicleReceivingType);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks(command.PurposeOfVisitId, command.UnitId, command.VehicleNumber!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyVehicleNumber_FailsValidation(string? vehicleNumber)
        {
            var command = ValidCommand();
            command.VehicleNumber = vehicleNumber;
            // FKColumnDelete: PurposeOfVisitId > 0 runs, UnitId > 0 runs, ReceivingTypeId = 9 runs
            // ReferenceDocTypeId is null, TransporterId is null — .When() guards skip
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(9)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsVehicleReceivingTypeAsync(9)).ReturnsAsync(true);
            // AlreadyExists .When(x => !string.IsNullOrWhiteSpace(x.VehicleNumber)) skips

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.VehicleNumber);
        }

        [Theory]
        [InlineData("123")]
        [InlineData("abcdefghij")]
        public async Task Validate_InvalidMobileNumber_FailsValidation(string mobileNo)
        {
            var command = ValidCommand();
            command.DriverMobileNo = mobileNo;
            SetupAllAsyncMocks(command.PurposeOfVisitId, command.UnitId, command.VehicleNumber!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DriverMobileNo);
        }

        [Fact]
        public async Task Validate_OpenVMRExists_FailsValidation()
        {
            var command = ValidCommand();
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(9)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsVehicleReceivingTypeAsync(9)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.HasOpenVMRForVehicleAsync(command.VehicleNumber!)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.VehicleNumber);
        }

        [Fact]
        public async Task Validate_InvalidPurposeOfVisitId_FailsValidation()
        {
            var command = ValidCommand();
            command.PurposeOfVisitId = 999;
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(999)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(9)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsVehicleReceivingTypeAsync(9)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.HasOpenVMRForVehicleAsync(command.VehicleNumber!)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PurposeOfVisitId);
        }

        [Fact]
        public async Task Validate_InvalidUnitId_FailsValidation()
        {
            var command = ValidCommand();
            command.UnitId = 999;
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(9)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(999)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsVehicleReceivingTypeAsync(9)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.HasOpenVMRForVehicleAsync(command.VehicleNumber!)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId);
        }

        [Fact]
        public async Task Validate_EmptyReceivingTypeId_FailsValidation()
        {
            var command = ValidCommand();
            command.ReceivingTypeId = null;
            // FK rule .When() skips when null. Conditional VehicleNumber .WhenAsync() skips when null.
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.HasOpenVMRForVehicleAsync(command.VehicleNumber!)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ReceivingTypeId);
        }

        [Fact]
        public async Task Validate_InvalidReceivingTypeId_FailsValidation()
        {
            var command = ValidCommand();
            command.ReceivingTypeId = 9999;
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(9999)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsVehicleReceivingTypeAsync(9999)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.HasOpenVMRForVehicleAsync(command.VehicleNumber!)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ReceivingTypeId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_ReceivingTypeCourier_VehicleNumberOptional_Passes(string? vehicleNumber)
        {
            var command = ValidCommand();
            command.ReceivingTypeId = 10; // Courier
            command.VehicleNumber = vehicleNumber; // empty is OK for non-Vehicle
            // FK ok, conditional VehicleNumber rule skipped (IsVehicleReceivingType=false)
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(10)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsVehicleReceivingTypeAsync(10)).ReturnsAsync(false);
            // HasOpenVMRForVehicleAsync .When() skips when VehicleNumber blank

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.VehicleNumber);
        }

        [Fact]
        public async Task Validate_EmptyDriverName_FailsValidation()
        {
            var command = ValidCommand();
            command.DriverName = "";
            SetupAllAsyncMocks(command.PurposeOfVisitId, command.UnitId, command.VehicleNumber!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DriverName);
        }

        [Fact]
        public async Task Validate_EmptyDriverMobileNo_FailsValidation()
        {
            var command = ValidCommand();
            command.DriverMobileNo = "";
            SetupAllAsyncMocks(command.PurposeOfVisitId, command.UnitId, command.VehicleNumber!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DriverMobileNo);
        }
    }
}
