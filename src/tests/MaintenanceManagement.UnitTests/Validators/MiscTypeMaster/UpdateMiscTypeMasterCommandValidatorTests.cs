using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.MiscTypeMaster;

namespace MaintenanceManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class UpdateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLength.Object);

        private void SetupAllMocks(int id = 1, string code = "MT001")
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateMiscTypeMasterCommand { Id = 1, MiscTypeCode = "MT001", Description = "Test Type" };
            SetupAllMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = new UpdateMiscTypeMasterCommand { Id = 1, MiscTypeCode = code, Description = "Test Type" };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = new UpdateMiscTypeMasterCommand { Id = 1, MiscTypeCode = "MT001", Description = "Test Type" };
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("MT001", 1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new UpdateMiscTypeMasterCommand { Id = 99, MiscTypeCode = "MT001", Description = "Test Type" };
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("MT001", 99)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }
    }
}
