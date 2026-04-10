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

        private void SetupAllValid()
        {
            _mockQueryRepo
                .Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.CompositeKeyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.IsValidModeMethodCombinationAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllValid();
            var command = FreightMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroFreightModeId_FailsValidation()
        {
            SetupAllValid();
            var command = FreightMasterBuilders.ValidCreateCommand(freightModeId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.FreightModeId);
        }

        [Fact]
        public async Task Validate_ZeroRateMethodId_FailsValidation()
        {
            SetupAllValid();
            var command = FreightMasterBuilders.ValidCreateCommand(rateMethodId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RateMethodId);
        }

        [Fact]
        public async Task Validate_ZeroModuleId_FailsValidation()
        {
            SetupAllValid();
            var command = FreightMasterBuilders.ValidCreateCommand(moduleId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ModuleId);
        }

        [Fact]
        public async Task Validate_ZeroRate_FailsValidation()
        {
            SetupAllValid();
            var command = FreightMasterBuilders.ValidCreateCommand(rate: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Rate);
        }

        [Fact]
        public async Task Validate_NegativeRate_FailsValidation()
        {
            SetupAllValid();
            var command = FreightMasterBuilders.ValidCreateCommand(rate: -10);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Rate);
        }

        [Fact]
        public async Task Validate_NonExistentFreightModeId_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(999)).ReturnsAsync(false);
            var command = FreightMasterBuilders.ValidCreateCommand(freightModeId: 999);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.FreightModeId);
        }

        [Fact]
        public async Task Validate_DuplicateCompositeKey_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo
                .Setup(r => r.CompositeKeyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(true);
            var command = FreightMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("combination of FreightModeId, RateMethodId and ModuleId already exists"));
        }

        [Fact]
        public async Task Validate_InvalidModeMethodCombination_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo
                .Setup(r => r.IsValidModeMethodCombinationAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);
            var command = FreightMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("Invalid FreightMode and RateMethod combination"));
        }
    }
}
