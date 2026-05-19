using FluentValidation.TestHelper;
using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Application.GateInward.Commands.CreateGateInward;
using GateEntryManagement.Application.GateInward.Dto;
using GateEntryManagement.Presentation.Validation.Common;
using GateEntryManagement.Presentation.Validation.GateInward;

namespace GateEntryManagement.UnitTests.Validators.GateInward
{
    public sealed class CreateGateInwardCommandValidatorTests
    {
        private readonly Mock<IGateInwardQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateGateInwardCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null), _mockQueryRepo.Object);

        private static CreateGateInwardCommand ValidCommand() =>
            new()
            {
                VehicleMovementRecordId = 1,
                UnitId = 1,
                GrossWeight = 100,
                TareWeight = 50,
                QAInspectionRequired = false,
                QAStatusId = null,
                Remarks = "Test remarks",
                GateInwardDetails = new List<CreateGateInwardDetailDto>
                {
                    new() { ReferenceDocTypeId = 1, ReferenceDocNo = "DOC001", PartyName = "Test Party" }
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
            // QAStatusId is null so .When() guard skips

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.VehicleMovementRecordId);
        }

        [Fact]
        public async Task Validate_EmptyDetails_FailsValidation()
        {
            var command = ValidCommand();
            command.GateInwardDetails = null;
            SetupAllAsyncMocks(command.VehicleMovementRecordId, command.UnitId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.GateInwardDetails);
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

        [Fact]
        public async Task Validate_NegativeGrossWeight_FailsValidation()
        {
            var command = ValidCommand();
            command.GrossWeight = -1;
            SetupAllAsyncMocks(command.VehicleMovementRecordId, command.UnitId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.GrossWeight);
        }

        [Fact]
        public async Task Validate_NegativeTareWeight_FailsValidation()
        {
            var command = ValidCommand();
            command.TareWeight = -1;
            SetupAllAsyncMocks(command.VehicleMovementRecordId, command.UnitId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TareWeight);
        }
    }
}
