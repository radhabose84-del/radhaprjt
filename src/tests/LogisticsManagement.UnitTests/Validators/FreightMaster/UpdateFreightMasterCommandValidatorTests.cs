using FluentValidation.TestHelper;
using LogisticsManagement.Application.Common.Interfaces.IFreightMaster;
using LogisticsManagement.Application.FreightMaster.Commands.UpdateFreightMaster;
using LogisticsManagement.Presentation.Validation.FreightMaster;
using LogisticsManagement.UnitTests.TestData;

namespace LogisticsManagement.UnitTests.Validators.FreightMaster
{
    public sealed class UpdateFreightMasterCommandValidatorTests
    {
        private readonly Mock<IFreightMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateFreightMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(
            int id = 1,
            int freightModeId = 1,
            int rateMethodId = 2,
            int moduleId = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(freightModeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(rateMethodId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(freightModeId, rateMethodId, moduleId, id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsValidModeMethodCombinationAsync(freightModeId, rateMethodId)).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = FreightMasterBuilders.ValidUpdateCommand();
            SetupAllAsyncMocks(command.Id, command.FreightModeId, command.RateMethodId, command.ModuleId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = FreightMasterBuilders.ValidUpdateCommand(id: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NonExistentId_FailsValidation()
        {
            var command = FreightMasterBuilders.ValidUpdateCommand(id: 99);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(command.FreightModeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(command.RateMethodId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(command.FreightModeId, command.RateMethodId, command.ModuleId, 99)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsValidModeMethodCombinationAsync(command.FreightModeId, command.RateMethodId)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ZeroRate_FailsValidation()
        {
            var command = FreightMasterBuilders.ValidUpdateCommand(rate: 0);
            SetupAllAsyncMocks(command.Id, command.FreightModeId, command.RateMethodId, command.ModuleId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Rate);
        }

        [Fact]
        public async Task Validate_InvalidIsActive_FailsValidation()
        {
            var command = FreightMasterBuilders.ValidUpdateCommand(isActive: 5);
            SetupAllAsyncMocks(command.Id, command.FreightModeId, command.RateMethodId, command.ModuleId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_DuplicateCompositeKey_FailsValidation()
        {
            var command = FreightMasterBuilders.ValidUpdateCommand();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(command.Id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(command.FreightModeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(command.RateMethodId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(command.FreightModeId, command.RateMethodId, command.ModuleId, command.Id)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsValidModeMethodCombinationAsync(command.FreightModeId, command.RateMethodId)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("combination of FreightModeId, RateMethodId and ModuleId already exists"));
        }

        [Fact]
        public async Task Validate_InvalidModeMethodCombination_FailsValidation()
        {
            var command = FreightMasterBuilders.ValidUpdateCommand();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(command.Id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(command.FreightModeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(command.RateMethodId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(command.FreightModeId, command.RateMethodId, command.ModuleId, command.Id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsValidModeMethodCombinationAsync(command.FreightModeId, command.RateMethodId)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("Invalid FreightMode and RateMethod combination"));
        }
    }
}
