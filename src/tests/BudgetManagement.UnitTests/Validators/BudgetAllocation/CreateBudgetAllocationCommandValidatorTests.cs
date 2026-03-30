using BudgetManagement.Application.BudgetAllocation.Command.Create;
using BudgetManagement.Application.Common.Interfaces.IBudgetAllocation;
using BudgetManagement.Presentation.Validation.BudgetAllocation;
using BudgetManagement.Presentation.Validation.Common;
using BudgetManagement.UnitTests.TestData;
using BudgetManagement.UnitTests.TestHelpers;
using Contracts.Interfaces;
using FluentValidation.TestHelper;

namespace BudgetManagement.UnitTests.Validators.BudgetAllocation
{
    public sealed class CreateBudgetAllocationCommandValidatorTests
    {
        private readonly Mock<IBudgetAllocationQueryRepository> _mockAllocationRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateBudgetAllocationCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(),
                _mockAllocationRepo.Object,
                _mockIp.Object);

        private void SetupNoDuplicate()
        {
            _mockIp.Setup(s => s.GetUnitId()).Returns(1);
            _mockAllocationRepo
                .Setup(r => r.ExistsAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupNoDuplicate();
            var command = BudgetAllocationBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyList_FailsValidation()
        {
            SetupNoDuplicate();
            var command = BudgetAllocationBuilders.ValidCreateCommand(new List<CreateBudgetAllocationDto>());

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.createBudgetAllocations);
        }

        [Fact]
        public async Task Validate_DuplicateAllocation_FailsValidation()
        {
            _mockIp.Setup(s => s.GetUnitId()).Returns(1);
            _mockAllocationRepo
                .Setup(r => r.ExistsAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = BudgetAllocationBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Validate_ZeroFinancialYearId_FailsValidation()
        {
            SetupNoDuplicate();
            var dto = BudgetAllocationBuilders.ValidCreateDto(financialYearId: 0);
            var command = BudgetAllocationBuilders.ValidCreateCommand(new List<CreateBudgetAllocationDto> { dto });

            var result = await CreateValidator().TestValidateAsync(command);

            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Validate_ZeroApprovedAmount_FailsValidation()
        {
            SetupNoDuplicate();
            var dto = BudgetAllocationBuilders.ValidCreateDto(approvedAmount: 0m);
            var command = BudgetAllocationBuilders.ValidCreateCommand(new List<CreateBudgetAllocationDto> { dto });

            var result = await CreateValidator().TestValidateAsync(command);

            result.IsValid.Should().BeFalse();
        }
    }
}
