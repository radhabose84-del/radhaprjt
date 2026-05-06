using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.CreateMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestCommand;
using MaintenanceManagement.Presentation.Validation.MaintenanceRequest;

namespace MaintenanceManagement.UnitTests.Validators.MaintenanceRequest
{
    public sealed class CreateMaintenanceRequestCommandValidatorTests
    {
        private readonly Mock<IMaintenanceRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private CreateMaintenanceRequestCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockDeptLookup.Object);

        // Sets up every async lookup to return a "found" result so only the rule under test fails.
        private void SetupAllLookupsAsValid()
        {
            _mockQueryRepo.Setup(r => r.MachineExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockDeptLookup.Setup(l => l.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DepartmentLookupDto { DepartmentId = 1, DepartmentName = "Dept" });
        }

        [Fact]
        public async Task Validate_ZeroRequestTypeId_FailsValidation()
        {
            var command = new CreateMaintenanceRequestCommand { RequestTypeId = 0 };
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NonExistentMachineId_FailsOnMachineId()
        {
            SetupAllLookupsAsValid();
            // Override: machine not found
            _mockQueryRepo.Setup(r => r.MachineExistsAsync(99)).ReturnsAsync(false);

            var command = new CreateMaintenanceRequestCommand
            {
                MaintenanceTypeId = 1,
                RequestTypeId = 1,
                MachineId = 99,
                ProductionDepartmentId = 1,
                MaintenanceDepartmentId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.MachineId);
        }

        [Fact]
        public async Task Validate_NonExistentProductionDepartment_FailsOnProductionDepartmentId()
        {
            SetupAllLookupsAsValid();
            // Override: production dept not found
            _mockDeptLookup.Setup(l => l.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DepartmentLookupDto?)null);

            var command = new CreateMaintenanceRequestCommand
            {
                MaintenanceTypeId = 1,
                RequestTypeId = 1,
                MachineId = 1,
                ProductionDepartmentId = 99,
                MaintenanceDepartmentId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ProductionDepartmentId);
        }

        [Fact]
        public async Task Validate_NonExistentMaintenanceDepartment_FailsOnMaintenanceDepartmentId()
        {
            SetupAllLookupsAsValid();
            // Override: maintenance dept not found
            _mockDeptLookup.Setup(l => l.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DepartmentLookupDto?)null);

            var command = new CreateMaintenanceRequestCommand
            {
                MaintenanceTypeId = 1,
                RequestTypeId = 1,
                MachineId = 1,
                ProductionDepartmentId = 1,
                MaintenanceDepartmentId = 99
            };

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.MaintenanceDepartmentId);
        }

        [Fact]
        public async Task Validate_AllValid_PassesFKChecks()
        {
            SetupAllLookupsAsValid();

            var command = new CreateMaintenanceRequestCommand
            {
                MaintenanceTypeId = 1,
                RequestTypeId = 1,
                MachineId = 1,
                ProductionDepartmentId = 1,
                MaintenanceDepartmentId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(x => x.MachineId);
            result.ShouldNotHaveValidationErrorFor(x => x.ProductionDepartmentId);
            result.ShouldNotHaveValidationErrorFor(x => x.MaintenanceDepartmentId);
        }
    }

    public sealed class UpdateMaintenanceRequestCommandValidatorTests
    {
        private readonly Mock<IMaintenanceRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var validator = new UpdateMaintenanceRequestCommandValidator(_mockQueryRepo.Object);
            var command = new UpdateMaintenanceRequestCommand { Id = 0 };
            var result = await validator.TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }
}
