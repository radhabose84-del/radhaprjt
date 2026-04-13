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

        private void SetupAllValid()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
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
            var command = FreightMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            SetupAllValid();
            var command = FreightMasterBuilders.ValidUpdateCommand(id: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NonExistentId_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            var command = FreightMasterBuilders.ValidUpdateCommand(id: 99);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ZeroRate_FailsValidation()
        {
            SetupAllValid();
            var command = FreightMasterBuilders.ValidUpdateCommand(rate: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Rate);
        }

        [Fact]
        public async Task Validate_InvalidIsActive_FailsValidation()
        {
            SetupAllValid();
            var command = FreightMasterBuilders.ValidUpdateCommand(isActive: 5);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_DuplicateCompositeKey_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo
                .Setup(r => r.CompositeKeyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(true);
            var command = FreightMasterBuilders.ValidUpdateCommand();

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
            var command = FreightMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("Invalid FreightMode and RateMethod combination"));
        }
    }
}
