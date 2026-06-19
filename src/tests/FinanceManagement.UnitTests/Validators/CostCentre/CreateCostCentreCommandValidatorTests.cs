using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.ICostCentre;
using FinanceManagement.Application.CostCentre.Commands.CreateCostCentre;
using FinanceManagement.Presentation.Validation.CostCentre;
using FinanceManagement.UnitTests.TestHelpers;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.CostCentre
{
    public sealed class CreateCostCentreCommandValidatorTests
    {
        private readonly Mock<ICostCentreQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentGroupLookup> _mockDeptGroup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDept = new(MockBehavior.Loose);

        private CreateCostCentreCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockIp.Object,
                _mockDeptGroup.Object, _mockDept.Object);

        // Everything passes by default; individual tests override one mock to force a failure.
        private void SetupAllPass(int levelSortOrder = 1)
        {
            _mockIp.Setup(s => s.GetUnitId()).Returns(1);
            _mockQueryRepo.Setup(r => r.GetCentreLevelSortOrderAsync(It.IsAny<int>())).ReturnsAsync(levelSortOrder);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByCodeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.PlantExistsForUnitAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ParentValidForLevelAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
            _mockDeptGroup.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DepartmentGroupLookupDto { DepartmentGroupId = 13, DepartmentGroupName = "Production" });
            _mockDept.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DepartmentLookupDto { DepartmentId = 80, DepartmentName = "Blow Room" });
        }

        private static CreateCostCentreCommand ValidL1() =>
            new() { CostCentreCode = "STP", CostCentreName = "Plant", CentreLevelId = 59, ParentCostCentreId = null };

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
            cmd.CostCentreCode = code;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.CostCentreCode);
        }

        [Theory]
        [InlineData("STP-01")]   // hyphen
        [InlineData("STP 01")]   // space
        [InlineData("STP@01")]   // special char
        public async Task Validate_NonAlphanumericCode_Fails(string code)
        {
            SetupAllPass();
            var cmd = ValidL1();
            cmd.CostCentreCode = code;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.CostCentreCode);
        }

        [Fact]
        public async Task Validate_DuplicateCodeInUnit_Fails()
        {
            SetupAllPass();
            _mockQueryRepo.Setup(r => r.AlreadyExistsByCodeAsync("STP", 1, It.IsAny<int?>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidL1());
            result.ShouldHaveValidationErrorFor(x => x.CostCentreCode);
        }

        [Fact]
        public async Task Validate_PlantAlreadyExistsForUnit_Fails()
        {
            SetupAllPass(levelSortOrder: 1);
            _mockQueryRepo.Setup(r => r.PlantExistsForUnitAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidL1());
            result.ShouldHaveValidationErrorFor(x => x.CentreLevelId);
        }

        [Fact]
        public async Task Validate_InvalidParentForLevel_Fails()
        {
            SetupAllPass(levelSortOrder: 2);   // L2 requires a valid L1 parent
            _mockQueryRepo.Setup(r => r.ParentValidForLevelAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);

            var cmd = new CreateCostCentreCommand
            {
                CostCentreCode = "STPPROD",
                CostCentreName = "Production",
                CentreLevelId = 60,
                ParentCostCentreId = 999,
                DepartmentGroupId = 13
            };

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.ParentCostCentreId);
        }
    }
}
