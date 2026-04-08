using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.MiscMaster;

namespace MaintenanceManagement.UnitTests.Validators.MiscMaster
{
    public sealed class UpdateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLength.Object);

        private void SetupAllMocks(int id = 1, string code = "C001", int miscTypeId = 1)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, miscTypeId, id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateMiscMasterCommand { Id = 1, Code = "C001", Description = "Test", MiscTypeId = 1 };
            SetupAllMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = new UpdateMiscMasterCommand { Id = 1, Code = code, MiscTypeId = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = new UpdateMiscMasterCommand { Id = 1, Code = "C001", Description = "Test", MiscTypeId = 1 };
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("C001", 1, 1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
