using FluentValidation.TestHelper;
using GateEntryManagement.Application.Common.Interfaces.IMiscMaster;
using GateEntryManagement.Application.MiscMaster.Commands.UpdateMiscMaster;
using GateEntryManagement.Presentation.Validation.Common;
using GateEntryManagement.Presentation.Validation.MiscMaster;

namespace GateEntryManagement.UnitTests.Validators.MiscMaster
{
    public sealed class UpdateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateMiscMasterCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateMiscMasterCommand
            {
                Id = 1,
                Description = "Updated Description",
                SortOrder = 1,
                IsActive = 1
            };
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyDescription_FailsValidation()
        {
            var command = new UpdateMiscMasterCommand
            {
                Id = 1,
                Description = "",
                SortOrder = 0,
                IsActive = 1
            };
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new UpdateMiscMasterCommand
            {
                Id = 999,
                Description = "Updated Description",
                SortOrder = 0,
                IsActive = 1
            };
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public async Task Validate_InvalidIsActive_FailsValidation(int isActive)
        {
            var command = new UpdateMiscMasterCommand
            {
                Id = 1,
                Description = "Updated Description",
                SortOrder = 0,
                IsActive = isActive
            };
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }
    }
}
