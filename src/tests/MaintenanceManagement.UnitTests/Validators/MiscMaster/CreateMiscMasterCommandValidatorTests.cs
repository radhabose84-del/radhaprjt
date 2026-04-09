using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.MiscMaster.Command.CreateMiscMaster;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.MiscMaster;

namespace MaintenanceManagement.UnitTests.Validators.MiscMaster
{
    public sealed class CreateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLength.Object);

        private void SetupNotFound(int miscTypeId, bool notFound = false)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(miscTypeId)).ReturnsAsync(notFound);
        }

        private void SetupAlreadyExists(string code, int miscTypeId, bool exists = false)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, miscTypeId, null)).ReturnsAsync(exists);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new CreateMiscMasterCommand { Code = "C001", Description = "Test", MiscTypeId = 1 };
            SetupNotFound(1);
            SetupAlreadyExists("C001", 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = new CreateMiscMasterCommand { Code = code, Description = "Test", MiscTypeId = 1 };
            SetupNotFound(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = new CreateMiscMasterCommand { Code = "C001", Description = "Test", MiscTypeId = 1 };
            SetupNotFound(1);
            SetupAlreadyExists("C001", 1, exists: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
