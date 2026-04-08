using FluentValidation.TestHelper;
using GateEntryManagement.Application.Common.Interfaces.IGatePass;
using GateEntryManagement.Application.GatePass.Commands.CreateGatePass;
using GateEntryManagement.Application.GatePass.Dto;
using GateEntryManagement.Presentation.Validation.Common;
using GateEntryManagement.Presentation.Validation.GatePass;

namespace GateEntryManagement.UnitTests.Validators.GatePass
{
    public sealed class CreateGatePassCommandValidatorTests
    {
        private readonly Mock<IGatePassQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateGatePassCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null), _mockQueryRepo.Object);

        private static CreateGatePassCommand ValidCommand() =>
            new()
            {
                GatePassDate = DateOnly.FromDateTime(DateTime.Today),
                VehicleMovementRecordId = 1,
                VehicleNumber = "KA01AB1234",
                DriverName = "Test Driver",
                DriverMobile = "9876543210",
                TransporterName = "Test Transporter",
                UnitId = 1,
                TotalItems = 5,
                TotalDocumentQty = 100m,
                TotalDispatchQty = 100m,
                ReturnableItems = 0,
                TotalValue = 5000m,
                Remarks = "Test remarks",
                GatePassDetails = new List<CreateGatePassDetailDto>
                {
                    new()
                    {
                        DocTypeId = 1,
                        DocId = 1,
                        DocNo = "DOC001",
                        PartyName = "Test Party",
                        PartyCode = "P001",
                        DocDate = DateOnly.FromDateTime(DateTime.Today),
                        TotalQty = 100m
                    }
                }
            };

        private void SetupAllAsyncMocks(int vmrId = 1, int unitId = 1)
        {
            _mockQueryRepo.Setup(r => r.VehicleMovementRecordExistsAsync(vmrId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(unitId)).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks(command.VehicleMovementRecordId, command.UnitId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyVMRId_FailsValidation()
        {
            var command = ValidCommand();
            command.VehicleMovementRecordId = 0;
            // VMR is 0 so FKColumnDelete .When(x => x.VehicleMovementRecordId > 0) skips
            // UnitId is 1 so FKColumnDelete .When(x => x.UnitId > 0) runs
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.VehicleMovementRecordId);
        }

        [Fact]
        public async Task Validate_EmptyVehicleNumber_FailsValidation()
        {
            var command = ValidCommand();
            command.VehicleNumber = "";
            SetupAllAsyncMocks(command.VehicleMovementRecordId, command.UnitId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.VehicleNumber);
        }

        [Fact]
        public async Task Validate_EmptyUnitId_FailsValidation()
        {
            var command = ValidCommand();
            command.UnitId = 0;
            // UnitId is 0 so FKColumnDelete .When(x => x.UnitId > 0) skips
            // VMR is 1 so FKColumnDelete .When(x => x.VehicleMovementRecordId > 0) runs
            _mockQueryRepo.Setup(r => r.VehicleMovementRecordExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId);
        }

        [Fact]
        public async Task Validate_EmptyDetails_FailsValidation()
        {
            var command = ValidCommand();
            command.GatePassDetails = null;
            SetupAllAsyncMocks(command.VehicleMovementRecordId, command.UnitId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.GatePassDetails);
        }

        [Fact]
        public async Task Validate_InvalidVMRId_FailsValidation()
        {
            var command = ValidCommand();
            command.VehicleMovementRecordId = 999;
            _mockQueryRepo.Setup(r => r.VehicleMovementRecordExistsAsync(999)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.VehicleMovementRecordId);
        }

        [Fact]
        public async Task Validate_InvalidUnitId_FailsValidation()
        {
            var command = ValidCommand();
            command.UnitId = 999;
            _mockQueryRepo.Setup(r => r.VehicleMovementRecordExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId);
        }
    }
}
