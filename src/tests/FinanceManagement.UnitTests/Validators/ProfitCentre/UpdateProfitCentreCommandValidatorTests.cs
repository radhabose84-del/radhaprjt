using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.IProfitCentre;
using FinanceManagement.Application.ProfitCentre.Commands.UpdateProfitCentre;
using FinanceManagement.Presentation.Validation.ProfitCentre;
using FinanceManagement.UnitTests.TestHelpers;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.ProfitCentre
{
    public sealed class UpdateProfitCentreCommandValidatorTests
    {
        private readonly Mock<IProfitCentreQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUser = new(MockBehavior.Loose);

        private UpdateProfitCentreCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockUser.Object);

        private void SetupHappyPath()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
        }

        private static UpdateProfitCentreCommand ValidCommand() =>
            new() { Id = 1, ProfitCentreName = "Spinning", IsActive = 1 };

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_Fails(string? name)
        {
            SetupHappyPath();
            var cmd = ValidCommand();
            cmd.ProfitCentreName = name;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.ProfitCentreName);
        }

        [Fact]
        public async Task Validate_NotFound_Fails()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task Validate_IsActiveOutOfRange_Fails(int isActive)
        {
            SetupHappyPath();
            var cmd = ValidCommand();
            cmd.IsActive = isActive;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }
    }
}
