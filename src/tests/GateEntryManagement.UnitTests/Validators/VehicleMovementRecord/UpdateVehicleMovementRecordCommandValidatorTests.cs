using FluentValidation.TestHelper;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.UpdateVehicleMovementRecord;
using GateEntryManagement.Presentation.Validation.Common;
using GateEntryManagement.Presentation.Validation.VehicleMovementRecord;

namespace GateEntryManagement.UnitTests.Validators.VehicleMovementRecord
{
    public sealed class UpdateVehicleMovementRecordCommandValidatorTests
    {
        private readonly Mock<IVehicleMovementRecordQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateVehicleMovementRecordCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null), _mockQueryRepo.Object);

        private static UpdateVehicleMovementRecordCommand ValidCommand() =>
            new()
            {
                Id = 1,
                VehicleNumber = "KA01AB1234",
                DriverName = "Test Driver",
                DriverMobileNo = "9876543210",
                DriverLicenseNo = "DL12345",
                PurposeOfVisitId = 1,
                ReferenceDocTypeId = null,
                ReferenceDocNo = null,
                TransporterId = null,
                Remarks = null,
                IsActive = 1
            };

        private void SetupAllAsyncMocks(int id = 1, int purposeOfVisitId = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(purposeOfVisitId)).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks(command.Id, command.PurposeOfVisitId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = ValidCommand();
            command.Id = 999;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public async Task Validate_InvalidIsActive_FailsValidation(int isActive)
        {
            var command = ValidCommand();
            command.IsActive = isActive;
            SetupAllAsyncMocks(command.Id, command.PurposeOfVisitId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_EmptyVehicleNumber_NowPassesValidation()
        {
            // VehicleNumber is now OPTIONAL on Update.
            var command = ValidCommand();
            command.VehicleNumber = null;
            SetupAllAsyncMocks(command.Id, command.PurposeOfVisitId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.VehicleNumber);
        }

        [Fact]
        public async Task Validate_EmptyDriverName_NowPassesValidation()
        {
            // DriverName is now OPTIONAL.
            var command = ValidCommand();
            command.DriverName = null;
            SetupAllAsyncMocks(command.Id, command.PurposeOfVisitId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.DriverName);
        }

        [Fact]
        public async Task Validate_InvalidPurposeOfVisitId_FailsValidation()
        {
            var command = ValidCommand();
            command.PurposeOfVisitId = 999;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PurposeOfVisitId);
        }

        [Theory]
        [InlineData("123")]
        [InlineData("abcdefghij")]
        public async Task Validate_InvalidMobileNumberFormat_FailsValidation(string mobileNo)
        {
            var command = ValidCommand();
            command.DriverMobileNo = mobileNo;
            SetupAllAsyncMocks(command.Id, command.PurposeOfVisitId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DriverMobileNo);
        }
    }
}
