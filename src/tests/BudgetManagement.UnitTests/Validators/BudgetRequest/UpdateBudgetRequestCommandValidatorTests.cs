using BudgetManagement.Application.BudgetRequest.Commands.Update;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Presentation.Validation.BudgetRequest;
using BudgetManagement.UnitTests.TestData;
using FluentValidation.TestHelper;

namespace BudgetManagement.UnitTests.Validators.BudgetRequest
{
    public sealed class UpdateBudgetRequestCommandValidatorTests
    {
        private readonly Mock<IBudgetRequestCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IBudgetRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);

        private UpdateBudgetRequestCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object,
                _mockMiscRepo.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(UpdateBudgetRequestCommand command)
        {
            _mockCommandRepo
                .Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BudgetRequestBuilders.ValidEntity(command.Id));

            _mockMiscRepo
                .Setup(r => r.GetByTypeAndCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BudgetManagement.Domain.Entities.MiscMaster?)null);

            _mockQueryRepo
                .Setup(r => r.AllocationExistsAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.ExistsOpexForUpdateAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(),
                    It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.ExistsCapexForUpdateAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(),
                    It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = BudgetRequestBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = BudgetRequestBuilders.ValidUpdateCommand(id: 0);
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_EntityNotFound_FailsValidation()
        {
            var command = BudgetRequestBuilders.ValidUpdateCommand(id: 99);
            _mockCommandRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BudgetManagement.Domain.Entities.BudgetRequest?)null);
            _mockMiscRepo
                .Setup(r => r.GetByTypeAndCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BudgetManagement.Domain.Entities.MiscMaster?)null);
            _mockQueryRepo
                .Setup(r => r.AllocationExistsAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ZeroUnitId_FailsValidation()
        {
            var command = BudgetRequestBuilders.ValidUpdateCommand(unitId: 0);
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId);
        }

        [Fact]
        public async Task Validate_NullFromDate_FailsValidation()
        {
            var command = BudgetRequestBuilders.ValidUpdateCommand();
            command.FromDate = null;
            SetupHappyPath(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.FromDate);
        }
    }
}
