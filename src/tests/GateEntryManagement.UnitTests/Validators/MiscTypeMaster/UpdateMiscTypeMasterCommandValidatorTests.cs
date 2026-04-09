using FluentValidation.TestHelper;
using GateEntryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using GateEntryManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster;
using GateEntryManagement.Presentation.Validation.Common;
using GateEntryManagement.Presentation.Validation.MiscTypeMaster;

namespace GateEntryManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class UpdateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateMiscTypeMasterCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateMiscTypeMasterCommand
            {
                Id = 1,
                Description = "Updated Description",
                IsActive = 1
            };
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyDescription_FailsValidation()
        {
            var command = new UpdateMiscTypeMasterCommand
            {
                Id = 1,
                Description = "",
                IsActive = 1
            };
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new UpdateMiscTypeMasterCommand
            {
                Id = 999,
                Description = "Updated Description",
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
            var command = new UpdateMiscTypeMasterCommand
            {
                Id = 1,
                Description = "Updated Description",
                IsActive = isActive
            };
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }
    }
}
