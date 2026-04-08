using FluentValidation.TestHelper;
using LogisticsManagement.Application.Common.Interfaces.IFreightMaster;
using LogisticsManagement.Application.FreightMaster.Commands.CreateFreightMaster;
using LogisticsManagement.Presentation.Validation.FreightMaster;
using LogisticsManagement.UnitTests.TestData;

namespace LogisticsManagement.UnitTests.Validators.FreightMaster
{
    public sealed class CreateFreightMasterCommandValidatorTests
    {
        private readonly Mock<IFreightMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateFreightMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(
            int freightModeId = 1,
            int rateMethodId = 2,
            int moduleId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.MiscMasterExistsAsync(freightModeId))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.MiscMasterExistsAsync(rateMethodId))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.CompositeKeyExistsAsync(freightModeId, rateMethodId, moduleId, null))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.IsValidModeMethodCombinationAsync(freightModeId, rateMethodId))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = FreightMasterBuilders.ValidCreateCommand();
            SetupAllAsyncMocks(command.FreightModeId, command.RateMethodId, command.ModuleId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroFreightModeId_FailsValidation()
        {
            var command = FreightMasterBuilders.ValidCreateCommand(freightModeId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.FreightModeId);
        }

        [Fact]
        public async Task Validate_ZeroRateMethodId_FailsValidation()
        {
            var command = FreightMasterBuilders.ValidCreateCommand(rateMethodId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RateMethodId);
        }

        [Fact]
        public async Task Validate_ZeroModuleId_FailsValidation()
        {
            var command = FreightMasterBuilders.ValidCreateCommand(moduleId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ModuleId);
        }

        [Fact]
        public async Task Validate_ZeroRate_FailsValidation()
        {
            var command = FreightMasterBuilders.ValidCreateCommand(rate: 0);
            SetupAllAsyncMocks(command.FreightModeId, command.RateMethodId, command.ModuleId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Rate);
        }

        [Fact]
        public async Task Validate_NegativeRate_FailsValidation()
        {
            var command = FreightMasterBuilders.ValidCreateCommand(rate: -10);
            SetupAllAsyncMocks(command.FreightModeId, command.RateMethodId, command.ModuleId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Rate);
        }

        [Fact]
        public async Task Validate_NonExistentFreightModeId_FailsValidation()
        {
            var command = FreightMasterBuilders.ValidCreateCommand(freightModeId: 999);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(999)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(2)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(999, 2, 1, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsValidModeMethodCombinationAsync(999, 2)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.FreightModeId);
        }

        [Fact]
        public async Task Validate_DuplicateCompositeKey_FailsValidation()
        {
            var command = FreightMasterBuilders.ValidCreateCommand();
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(command.FreightModeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(command.RateMethodId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(command.FreightModeId, command.RateMethodId, command.ModuleId, null)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsValidModeMethodCombinationAsync(command.FreightModeId, command.RateMethodId)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("combination of FreightModeId, RateMethodId and ModuleId already exists"));
        }

        [Fact]
        public async Task Validate_InvalidModeMethodCombination_FailsValidation()
        {
            var command = FreightMasterBuilders.ValidCreateCommand();
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(command.FreightModeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(command.RateMethodId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(command.FreightModeId, command.RateMethodId, command.ModuleId, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsValidModeMethodCombinationAsync(command.FreightModeId, command.RateMethodId)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("Invalid FreightMode and RateMethod combination"));
        }
    }
}
