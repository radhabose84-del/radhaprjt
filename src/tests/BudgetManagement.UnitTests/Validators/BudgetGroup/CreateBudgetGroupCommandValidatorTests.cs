using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Application.BudgetGroups.Commands.CreateBudgetGroup;
using BudgetManagement.Domain.Common;
using BudgetManagement.Presentation.Validation.BudgetGroup;
using BudgetManagement.UnitTests.TestData;
using FluentValidation.TestHelper;

namespace BudgetManagement.UnitTests.Validators.BudgetGroup
{
    public sealed class CreateBudgetGroupCommandValidatorTests
    {
        private readonly Mock<IBudgetGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IBudgetGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscMasterRepo = new(MockBehavior.Strict);

        private CreateBudgetGroupCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMiscMasterRepo.Object);

        private void SetupMiscMasterDefaults(
            int? percentageId = 5,
            int? spindleId = 6,
            int? requestId = 7,
            int? annualId = 10,
            int? monthlyId = 11)
        {
            _mockMiscMasterRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypePercentage))
                .ReturnsAsync(percentageId.HasValue
                    ? new BudgetManagement.Domain.Entities.MiscMaster { Id = percentageId.Value }
                    : null);

            _mockMiscMasterRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypeSpindle))
                .ReturnsAsync(spindleId.HasValue
                    ? new BudgetManagement.Domain.Entities.MiscMaster { Id = spindleId.Value }
                    : null);

            _mockMiscMasterRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypeRequest))
                .ReturnsAsync(requestId.HasValue
                    ? new BudgetManagement.Domain.Entities.MiscMaster { Id = requestId.Value }
                    : null);

            _mockMiscMasterRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.BudgetType, MiscEnumEntity.BudgetTypeAnnual))
                .ReturnsAsync(annualId.HasValue
                    ? new BudgetManagement.Domain.Entities.MiscMaster { Id = annualId.Value }
                    : null);

            _mockMiscMasterRepo
                .Setup(r => r.GetMiscMasterByName(MiscEnumEntity.BudgetType, MiscEnumEntity.BudgetTypeMonthly))
                .ReturnsAsync(monthlyId.HasValue
                    ? new BudgetManagement.Domain.Entities.MiscMaster { Id = monthlyId.Value }
                    : null);
        }

        private void SetupIsNameDuplicateNotDuplicate(string name = "Test Budget Group", int unitId = 1, int departmentId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.IsNameDuplicateAsync(name, 0, unitId, departmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupMiscMasterDefaults();
            var command = BudgetGroupBuilders.ValidCreateCommand(budgetTypeId: 10);
            SetupIsNameDuplicateNotDuplicate(command.Name, command.UnitId, command.DepartmentId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string name)
        {
            SetupMiscMasterDefaults();
            var command = BudgetGroupBuilders.ValidCreateCommand(name: name, budgetTypeId: 10);

            // FluentValidation runs all rules regardless; IsNameDuplicateAsync fires even for empty names
            _mockCommandRepo
                .Setup(r => r.IsNameDuplicateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public async Task Validate_ZeroUnitId_FailsValidation()
        {
            SetupMiscMasterDefaults();
            var command = BudgetGroupBuilders.ValidCreateCommand(unitId: 0, budgetTypeId: 10);
            SetupIsNameDuplicateNotDuplicate(command.Name, 0, command.DepartmentId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId)
                .WithErrorMessage("Unit is required.");
        }

        [Fact]
        public async Task Validate_ZeroDepartmentId_FailsValidation()
        {
            SetupMiscMasterDefaults();
            var command = BudgetGroupBuilders.ValidCreateCommand(departmentId: 0, budgetTypeId: 10);
            SetupIsNameDuplicateNotDuplicate(command.Name, command.UnitId, 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DepartmentId)
                .WithErrorMessage("Department is required.");
        }

        [Fact]
        public async Task Validate_ZeroCostCenterId_FailsValidation()
        {
            SetupMiscMasterDefaults();
            var command = BudgetGroupBuilders.ValidCreateCommand(costCenterId: 0, budgetTypeId: 10);
            SetupIsNameDuplicateNotDuplicate(command.Name, command.UnitId, command.DepartmentId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CostCenterId)
                .WithErrorMessage("Cost Center is required.");
        }

        [Fact]
        public async Task Validate_ZeroCurrencyId_FailsValidation()
        {
            SetupMiscMasterDefaults();
            var command = BudgetGroupBuilders.ValidCreateCommand(currencyId: 0, budgetTypeId: 10);
            SetupIsNameDuplicateNotDuplicate(command.Name, command.UnitId, command.DepartmentId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CurrencyId)
                .WithErrorMessage("Currency is required.");
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            SetupMiscMasterDefaults();
            var command = BudgetGroupBuilders.ValidCreateCommand(budgetTypeId: 10);

            _mockCommandRepo
                .Setup(r => r.IsNameDuplicateAsync(command.Name, 0, command.UnitId, command.DepartmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Budget Group name already exists for this Unit and Department.");
        }

        [Fact]
        public async Task Validate_BothAllocationFieldsProvided_FailsValidation()
        {
            SetupMiscMasterDefaults();
            var command = new CreateBudgetGroupCommand
            {
                Name = "Test Group",
                UnitId = 1,
                DepartmentId = 1,
                CostCenterId = 1,
                CurrencyId = 1,
                BudgetTypeId = 10,
                AllocatedPercentage = 50m,
                AllocatedSpindleCost = 100m,
                IsActive = true
            };

            _mockCommandRepo
                .Setup(r => r.IsNameDuplicateAsync(command.Name, 0, command.UnitId, command.DepartmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }
    }
}
