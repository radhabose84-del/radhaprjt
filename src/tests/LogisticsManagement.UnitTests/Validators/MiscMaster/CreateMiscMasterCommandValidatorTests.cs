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

        private void SetupAllAsyncMocks(
            string code = "CODE001",
            int miscTypeId = 1)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, miscTypeId, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscTypeExistsAsync(miscTypeId)).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupAllAsyncMocks(command.Code!, command.MiscTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = MiscMasterBuilders.ValidCreateCommand(code: code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            var command = MiscMasterBuilders.ValidCreateCommand(description: description);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_ZeroMiscTypeId_FailsValidation()
        {
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
            var command = MiscMasterBuilders.ValidCreateCommand(code: code);
            SetupAllAsyncMocks(code, command.MiscTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = MiscMasterBuilders.ValidCreateCommand(code: "EXIST001");
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("EXIST001", command.MiscTypeId, null)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscTypeExistsAsync(command.MiscTypeId)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public async Task Validate_NonExistentMiscTypeId_FailsValidation()
        {
            var command = MiscMasterBuilders.ValidCreateCommand(miscTypeId: 999);
            _mockQueryRepo.Setup(r => r.MiscTypeExistsAsync(999)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(command.Code!, 999, null)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeId);
        }
    }
}
