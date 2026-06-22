using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.IProfitCentre;
using FinanceManagement.Application.ProfitCentre.Commands.CreateProfitCentre;
using FinanceManagement.Presentation.Validation.ProfitCentre;
using FinanceManagement.UnitTests.TestHelpers;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.ProfitCentre
{
    public sealed class CreateProfitCentreCommandValidatorTests
    {
        private readonly Mock<IProfitCentreQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUser = new(MockBehavior.Loose);

        private CreateProfitCentreCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockUser.Object);

        // Everything passes by default; individual tests override one mock to force a failure.
        private void SetupAllPass(int levelSortOrder = 1)
        {
            _mockQueryRepo.Setup(r => r.GetLevelSortOrderAsync(It.IsAny<int>())).ReturnsAsync(levelSortOrder);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByCodeAsync(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ParentValidForLevelAsync(It.IsAny<int?>(), It.IsAny<int>())).ReturnsAsync(true);
            _mockUser.Setup(u => u.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserLookupDto { UserId = 5, FirstName = "Head", LastName = "User" });
        }

        private static CreateProfitCentreCommand ValidL1() =>
            new() { ProfitCentreCode = "PCSPIN", ProfitCentreName = "Spinning", LevelId = 62, ParentProfitCentreId = null };

        [Fact]
        public async Task Validate_ValidL1_Passes()
        {
            SetupAllPass(levelSortOrder: 1);
            var result = await CreateValidator().TestValidateAsync(ValidL1());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_Fails(string? code)
        {
            SetupAllPass();
            var cmd = ValidL1();
            cmd.ProfitCentreCode = code;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.ProfitCentreCode);
        }

        [Theory]
        [InlineData("PC-SPIN")]       // hyphen — allowed for profit-centre codes
        [InlineData("PC-SPIN-001")]   // multiple hyphens — allowed
        public async Task Validate_HyphenatedCode_Passes(string code)
        {
            SetupAllPass();
            var cmd = ValidL1();
            cmd.ProfitCentreCode = code;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldNotHaveValidationErrorFor(x => x.ProfitCentreCode);
        }

        [Theory]
        [InlineData("PC SPIN")]   // space — not allowed
        [InlineData("PC@SPIN")]   // special char — not allowed
        public async Task Validate_InvalidCharCode_Fails(string code)
        {
            SetupAllPass();
            var cmd = ValidL1();
            cmd.ProfitCentreCode = code;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.ProfitCentreCode);
        }

        [Fact]
        public async Task Validate_DuplicateCode_Fails()
        {
            SetupAllPass();
            _mockQueryRepo.Setup(r => r.AlreadyExistsByCodeAsync("PCSPIN", It.IsAny<int?>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidL1());
            result.ShouldHaveValidationErrorFor(x => x.ProfitCentreCode);
        }

        [Fact]
        public async Task Validate_InvalidParentForLevel_Fails()
        {
            SetupAllPass(levelSortOrder: 2);   // L2 requires a valid L1 parent
            _mockQueryRepo.Setup(r => r.ParentValidForLevelAsync(It.IsAny<int?>(), It.IsAny<int>())).ReturnsAsync(false);

            var cmd = new CreateProfitCentreCommand
            {
                ProfitCentreCode = "PCSPIN001",
                ProfitCentreName = "Cotton Spinning",
                LevelId = 63,
                ParentProfitCentreId = 999
            };

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.ParentProfitCentreId);
        }

        [Fact]
        public async Task Validate_InvalidResponsibleHead_Fails()
        {
            SetupAllPass();
            _mockUser.Setup(u => u.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((UserLookupDto?)null);

            var cmd = ValidL1();
            cmd.ResponsibleHeadId = 999;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.ResponsibleHeadId);
        }
    }
}
