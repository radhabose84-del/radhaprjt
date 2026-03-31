using BudgetManagement.Application.BudgetRequest.Commands.Create;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Presentation.Validation.BudgetRequest;
using BudgetManagement.UnitTests.TestData;
using Contracts.Interfaces;
using FluentValidation.TestHelper;

namespace BudgetManagement.UnitTests.Validators.BudgetRequest
{
    public sealed class CreateBudgetRequestCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IBudgetRequestCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IBudgetRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateBudgetRequestCommandValidator CreateValidator() =>
            new(_mockMiscRepo.Object, _mockIp.Object,
                _mockCommandRepo.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(CreateBudgetRequestCommand command)
        {
            _mockIp.Setup(s => s.GetUnitId()).Returns(1);

            _mockMiscRepo
                .Setup(r => r.GetByTypeAndCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BudgetManagement.Domain.Entities.MiscMaster?)null);

            _mockQueryRepo
                .Setup(r => r.AllocationExistsAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockCommandRepo
                .Setup(r => r.ExistsOpexAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.ExistsCapexAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = BudgetRequestBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroUnitId_FailsValidation()
        {
            var command = BudgetRequestBuilders.ValidCreateCommand(unitId: 0);
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId);
        }

        [Fact]
        public async Task Validate_ZeroCurrencyId_FailsValidation()
        {
            var command = BudgetRequestBuilders.ValidCreateCommand(currencyId: 0);
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CurrencyId);
        }

        [Fact]
        public async Task Validate_ZeroFinancialYearId_FailsValidation()
        {
            var command = BudgetRequestBuilders.ValidCreateCommand(financialYearId: 0);
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.FinancialYearId);
        }

        [Fact]
        public async Task Validate_ZeroRequestAmount_FailsValidation()
        {
            var command = BudgetRequestBuilders.ValidCreateCommand(requestAmount: 0m);
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RequestAmount);
        }

        [Fact]
        public async Task Validate_UnitNotResolvedFromIp_FailsValidation()
        {
            var command = BudgetRequestBuilders.ValidCreateCommand();
            SetupHappyPath(command);
            _mockIp.Setup(s => s.GetUnitId()).Returns(0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.IsValid.Should().BeFalse();
        }
    }
}
