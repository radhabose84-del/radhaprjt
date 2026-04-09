using FluentValidation.TestHelper;
using LogisticsManagement.Application.Common.Interfaces.IMiscMaster;
using LogisticsManagement.Application.MiscMaster.Commands.UpdateMiscMaster;
using LogisticsManagement.Presentation.Validation.Common;
using LogisticsManagement.Presentation.Validation.MiscMaster;
using LogisticsManagement.UnitTests.TestData;

namespace LogisticsManagement.UnitTests.Validators.MiscMaster
{
    public sealed class UpdateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateMiscMasterCommandValidator CreateValidator()
        {
            var maxLengthProvider = new MaxLengthProvider(null!);
            return new UpdateMiscMasterCommandValidator(maxLengthProvider, _mockQueryRepo.Object);
        }

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand();
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(id: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NonExistentId_FailsValidation()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(id: 99);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(description: description);
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_InvalidIsActive_FailsValidation()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(isActive: 5);
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_NegativeSortOrder_FailsValidation()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(sortOrder: -1);
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SortOrder);
        }

        [Fact]
        public async Task Validate_ZeroSortOrder_PassesValidation()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(sortOrder: 0);
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.SortOrder);
        }
    }
}
