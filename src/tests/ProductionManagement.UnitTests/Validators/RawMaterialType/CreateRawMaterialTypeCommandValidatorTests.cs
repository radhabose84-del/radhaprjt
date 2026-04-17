using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Commands.CreateRawMaterialType;
using ProductionManagement.Presentation.Validation.RawMaterialType;
using ProductionManagement.UnitTests.TestHelpers;

namespace ProductionManagement.UnitTests.Validators.RawMaterialType
{
    public sealed class CreateRawMaterialTypeCommandValidatorTests
    {
        private readonly Mock<IRawMaterialTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateRawMaterialTypeCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(string code = "RMT001", string name = "Cotton Raw")
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.NameAlreadyExistsAsync(name, null)).ReturnsAsync(false);
        }

        private static CreateRawMaterialTypeCommand BuildValidCommand() => new()
        {
            RawMaterialTypeCode = "RMT001",
            RawMaterialTypeName = "Cotton Raw",
            Description = "Test description",
            EffectiveFrom = DateTimeOffset.UtcNow,
            EffectiveTo = DateTimeOffset.UtcNow.AddMonths(6)
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = BuildValidCommand();
            SetupAllAsyncMocks(command.RawMaterialTypeCode, command.RawMaterialTypeName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyCode_FailsValidation()
        {
            var command = BuildValidCommand();
            command.RawMaterialTypeCode = string.Empty;
            SetupAllAsyncMocks(string.Empty, command.RawMaterialTypeName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RawMaterialTypeCode);
        }

        [Fact]
        public async Task Validate_EmptyName_FailsValidation()
        {
            var command = BuildValidCommand();
            command.RawMaterialTypeName = string.Empty;
            SetupAllAsyncMocks(command.RawMaterialTypeCode, string.Empty);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RawMaterialTypeName);
        }

        [Theory]
        [InlineData("CODE-01")]
        [InlineData("CODE 01")]
        [InlineData("CODE@01")]
        public async Task Validate_NonAlphanumericCode_FailsValidation(string code)
        {
            var command = BuildValidCommand();
            command.RawMaterialTypeCode = code;
            SetupAllAsyncMocks(code, command.RawMaterialTypeName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RawMaterialTypeCode);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = BuildValidCommand();
            command.RawMaterialTypeCode = "EXIST01";
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("EXIST01", null)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.NameAlreadyExistsAsync(command.RawMaterialTypeName, null)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RawMaterialTypeCode);
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            var command = BuildValidCommand();
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(command.RawMaterialTypeCode, null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.NameAlreadyExistsAsync(command.RawMaterialTypeName, null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RawMaterialTypeName);
        }

        [Fact]
        public async Task Validate_EffectiveToBeforeEffectiveFrom_FailsValidation()
        {
            var command = BuildValidCommand();
            command.EffectiveFrom = DateTimeOffset.UtcNow;
            command.EffectiveTo = DateTimeOffset.UtcNow.AddDays(-1);
            SetupAllAsyncMocks(command.RawMaterialTypeCode, command.RawMaterialTypeName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EffectiveTo);
        }

        [Fact]
        public async Task Validate_NullEffectiveTo_PassesValidation()
        {
            var command = BuildValidCommand();
            command.EffectiveTo = null;
            SetupAllAsyncMocks(command.RawMaterialTypeCode, command.RawMaterialTypeName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyCommand_HasValidationErrors()
        {
            var result = await CreateValidator().TestValidateAsync(new CreateRawMaterialTypeCommand());

            result.ShouldHaveAnyValidationError();
        }
    }
}
