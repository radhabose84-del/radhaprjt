using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Commands.UpdateRawMaterialType;
using ProductionManagement.Presentation.Validation.RawMaterialType;
using ProductionManagement.UnitTests.TestHelpers;

namespace ProductionManagement.UnitTests.Validators.RawMaterialType
{
    public sealed class UpdateRawMaterialTypeCommandValidatorTests
    {
        private readonly Mock<IRawMaterialTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateRawMaterialTypeCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1, string name = "Cotton Raw")
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.NameAlreadyExistsAsync(name, id)).ReturnsAsync(false);
        }

        private static UpdateRawMaterialTypeCommand BuildValidCommand(int id = 1) => new()
        {
            Id = id,
            RawMaterialTypeName = "Cotton Raw",
            Description = "Updated description",
            EffectiveFrom = DateTimeOffset.UtcNow,
            EffectiveTo = DateTimeOffset.UtcNow.AddMonths(6),
            IsActive = 1
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = BuildValidCommand();
            SetupAllAsyncMocks(command.Id, command.RawMaterialTypeName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = BuildValidCommand(0);
            SetupAllAsyncMocks(0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            SetupAllAsyncMocks(999);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(BuildValidCommand(999));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_EmptyName_FailsValidation()
        {
            var command = BuildValidCommand();
            command.RawMaterialTypeName = string.Empty;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RawMaterialTypeName);
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            var command = BuildValidCommand();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(command.Id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.NameAlreadyExistsAsync(command.RawMaterialTypeName, command.Id)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RawMaterialTypeName);
        }

        [Fact]
        public async Task Validate_InvalidIsActive_FailsValidation()
        {
            var command = BuildValidCommand();
            command.IsActive = 5;
            SetupAllAsyncMocks(command.Id, command.RawMaterialTypeName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_EffectiveToBeforeEffectiveFrom_FailsValidation()
        {
            var command = BuildValidCommand();
            command.EffectiveFrom = DateTimeOffset.UtcNow;
            command.EffectiveTo = DateTimeOffset.UtcNow.AddDays(-1);
            SetupAllAsyncMocks(command.Id, command.RawMaterialTypeName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EffectiveTo);
        }

        [Fact]
        public async Task Validate_EmptyCommand_HasValidationErrors()
        {
            var result = await CreateValidator().TestValidateAsync(new UpdateRawMaterialTypeCommand { Id = 1 });

            result.ShouldHaveAnyValidationError();
        }
    }
}
