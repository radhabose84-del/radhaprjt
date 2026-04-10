using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Commands.UpdateStoTypeMaster;
using SalesManagement.Presentation.Validation.StoTypeMaster;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.StoTypeMaster
{
    public sealed class UpdateStoTypeMasterCommandValidatorTests
    {
        private readonly Mock<IStoTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateStoTypeMasterCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private static UpdateStoTypeMasterCommand ValidCommand() => new()
        {
            Id = 1,
            StoTypeName = "Updated STO Type",
            PgiMovementTypeId = 1,
            GrMovementTypeId = 2,
            IsActive = 1
        };

        private void SetupAllAsyncMocks(int id = 1, int pgiId = 1, int grId = 2)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MovementTypeExistsAsync(pgiId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MovementTypeExistsAsync(grId)).ReturnsAsync(true);
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            var cmd = ValidCommand();
            cmd.Id = id;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MovementTypeExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MovementTypeExistsAsync(2)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MovementTypeExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MovementTypeExistsAsync(2)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task StoTypeName_Empty_FailsValidation(string? name)
        {
            var cmd = ValidCommand();
            cmd.StoTypeName = name;
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.StoTypeName);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            var cmd = ValidCommand();
            cmd.IsActive = isActive;
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task PgiMovementTypeNotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MovementTypeExistsAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MovementTypeExistsAsync(2)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.PgiMovementTypeId);
        }
    }
}
