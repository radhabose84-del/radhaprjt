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
                ReferenceDocTypeId = null,
                ReferenceDocNo = null,
                TransporterId = null,
                UnitId = 1,
                Remarks = null
            };

        private void SetupAllAsyncMocks(
            int purposeOfVisitId = 1,
            int unitId = 1,
            string vehicleNumber = "KA01AB1234")
        {
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(purposeOfVisitId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(unitId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.HasOpenVMRForVehicleAsync(vehicleNumber)).ReturnsAsync(false);
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
        public async Task Validate_EmptyVehicleNumber_NowPassesValidation(string? vehicleNumber)
        {
            // VehicleNumber is now OPTIONAL — user fills it only when receiving type = Vehicle (UI-driven).
            var command = ValidCommand();
            command.VehicleNumber = vehicleNumber;
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);
            // HasOpenVMRForVehicleAsync .When() guard skips when blank.

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.VehicleNumber);
        }

        [Fact]
        public async Task Validate_EmptyDriverName_NowPassesValidation()
        {
            // DriverName is now OPTIONAL.
            var command = ValidCommand();
            command.DriverName = null;
            SetupAllAsyncMocks(command.PurposeOfVisitId, command.UnitId, command.VehicleNumber!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.DriverName);
        }

        [Fact]
        public async Task Validate_EmptyDriverMobileNo_NowPassesValidation()
        {
            // DriverMobileNo is now OPTIONAL.
            var command = ValidCommand();
            command.DriverMobileNo = null;
            SetupAllAsyncMocks(command.PurposeOfVisitId, command.UnitId, command.VehicleNumber!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.DriverMobileNo);
        }

        [Theory]
        [InlineData("123")]
        [InlineData("abcdefghij")]
        public async Task Validate_InvalidMobileNumberFormat_FailsValidation(string mobileNo)
        {
            // Format rule fires only when DriverMobileNo is non-empty (its own .When() guard).
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
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);
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
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);
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
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(999)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.HasOpenVMRForVehicleAsync(command.VehicleNumber!)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId);
        }
    }
}
