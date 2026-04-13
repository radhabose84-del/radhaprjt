using FluentValidation.TestHelper;
using LogisticsManagement.Application.Common.Interfaces.IMiscMaster;
using LogisticsManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using LogisticsManagement.Presentation.Validation.Common;
using LogisticsManagement.Presentation.Validation.MiscMaster;
using LogisticsManagement.UnitTests.TestData;

namespace LogisticsManagement.UnitTests.Validators.MiscMaster
{
    public sealed class CreateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateMiscMasterCommandValidator CreateValidator()
        {
            var maxLengthProvider = new MaxLengthProvider(null!);
            return new CreateMiscMasterCommandValidator(maxLengthProvider, _mockQueryRepo.Object);
        }

        private void SetupAllValid()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.MiscTypeExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllValid();
            var command = MiscMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            SetupAllValid();
            var command = MiscMasterBuilders.ValidCreateCommand(code: code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            SetupAllValid();
            var command = MiscMasterBuilders.ValidCreateCommand(description: description);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_ZeroMiscTypeId_FailsValidation()
        {
            SetupAllValid();
            var command = MiscMasterBuilders.ValidCreateCommand(miscTypeId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeId);
        }

        [Theory]
        [InlineData("CODE-01")]
        [InlineData("CODE 01")]
        [InlineData("CODE@01")]
        public async Task Validate_NonAlphanumericCode_FailsValidation(string code)
        {
            SetupAllValid();
            var command = MiscMasterBuilders.ValidCreateCommand(code: code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("EXIST001", It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(true);
            var command = MiscMasterBuilders.ValidCreateCommand(code: "EXIST001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public async Task Validate_NonExistentMiscTypeId_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.MiscTypeExistsAsync(999)).ReturnsAsync(false);
            var command = MiscMasterBuilders.ValidCreateCommand(miscTypeId: 999);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeId);
        }
    }
}
