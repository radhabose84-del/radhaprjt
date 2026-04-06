using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Commands.CreateStoTypeMaster;
using SalesManagement.Presentation.Validation.StoTypeMaster;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.StoTypeMaster
{
    public sealed class CreateStoTypeMasterCommandValidatorTests
    {
        private readonly Mock<IStoTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateStoTypeMasterCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private static CreateStoTypeMasterCommand ValidCommand() => new()
        {
            StoTypeCode = "STO01",
            StoTypeName = "Test STO Type",
            PgiMovementTypeId = 1,
            GrMovementTypeId = 2
        };

        private void SetupAllAsyncMocks(string code = "STO01", int pgiId = 1, int grId = 2)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code!, It.IsAny<int?>())).ReturnsAsync(false);
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
        [InlineData(null)]
        [InlineData("")]
        public async Task StoTypeCode_Empty_FailsValidation(string? code)
        {
            var cmd = ValidCommand();
            cmd.StoTypeCode = code;
            _mockQueryRepo.Setup(r => r.MovementTypeExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MovementTypeExistsAsync(2)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.StoTypeCode);
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
        [InlineData(0)]
        [InlineData(-1)]
        public async Task PgiMovementTypeId_ZeroOrNegative_FailsValidation(int pgiId)
        {
            var cmd = ValidCommand();
            cmd.PgiMovementTypeId = pgiId;
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("STO01", null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MovementTypeExistsAsync(2)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.PgiMovementTypeId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GrMovementTypeId_ZeroOrNegative_FailsValidation(int grId)
        {
            var cmd = ValidCommand();
            cmd.GrMovementTypeId = grId;
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("STO01", null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MovementTypeExistsAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.GrMovementTypeId);
        }

        [Fact]
        public async Task DuplicateCode_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("STO01", null)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MovementTypeExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MovementTypeExistsAsync(2)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.StoTypeCode);
        }

        [Fact]
        public async Task PgiMovementTypeNotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("STO01", null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MovementTypeExistsAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MovementTypeExistsAsync(2)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.PgiMovementTypeId);
        }
    }
}
