using FluentValidation.TestHelper;
using GateEntryManagement.Application.Common.Interfaces.IMiscMaster;
using GateEntryManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using GateEntryManagement.Presentation.Validation.Common;
using GateEntryManagement.Presentation.Validation.MiscMaster;

namespace GateEntryManagement.UnitTests.Validators.MiscMaster
{
    public sealed class CreateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateMiscMasterCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(string code = "CODE001", int miscTypeId = 1)
        {
            _mockQueryRepo.Setup(r => r.MiscTypeExistsAsync(miscTypeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, miscTypeId, null)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new CreateMiscMasterCommand
            {
                Code = "CODE001",
                Description = "Test Description",
                MiscTypeId = 1
            };
            SetupAllAsyncMocks(command.Code!, command.MiscTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = new CreateMiscMasterCommand
            {
                Code = code,
                Description = "Test Description",
                MiscTypeId = 1
            };
            // FKColumnDelete still runs for MiscTypeId > 0
            _mockQueryRepo.Setup(r => r.MiscTypeExistsAsync(1)).ReturnsAsync(true);
            // AlreadyExists .When() guard skips when code is null/empty

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public async Task Validate_EmptyDescription_FailsValidation()
        {
            var command = new CreateMiscMasterCommand
            {
                Code = "CODE001",
                Description = "",
                MiscTypeId = 1
            };
            SetupAllAsyncMocks(command.Code!, command.MiscTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = new CreateMiscMasterCommand
            {
                Code = "EXIST001",
                Description = "Test Description",
                MiscTypeId = 1
            };
            _mockQueryRepo.Setup(r => r.MiscTypeExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("EXIST001", 1, null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public async Task Validate_InvalidMiscTypeId_FailsValidation()
        {
            var command = new CreateMiscMasterCommand
            {
                Code = "CODE001",
                Description = "Test Description",
                MiscTypeId = 999
            };
            _mockQueryRepo.Setup(r => r.MiscTypeExistsAsync(999)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("CODE001", 999, null)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeId);
        }

        [Theory]
        [InlineData("CODE-01")]
        [InlineData("CODE 01")]
        [InlineData("CODE@01")]
        public async Task Validate_NonAlphanumericCode_FailsValidation(string code)
        {
            var command = new CreateMiscMasterCommand
            {
                Code = code,
                Description = "Test Description",
                MiscTypeId = 1
            };
            _mockQueryRepo.Setup(r => r.MiscTypeExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, 1, null)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }
    }
}
