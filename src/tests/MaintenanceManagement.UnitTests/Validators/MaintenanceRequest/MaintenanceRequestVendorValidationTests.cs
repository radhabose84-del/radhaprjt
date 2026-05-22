using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.CreateMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestCommand;
using MaintenanceManagement.Presentation.Validation.MaintenanceRequest;

namespace MaintenanceManagement.UnitTests.Validators.MaintenanceRequest
{
    public sealed class MaintenanceRequestVendorValidationTests
    {
        private const int ExternalTypeId = 99;
        private const int InternalTypeId = 1;

        private readonly Mock<IMaintenanceRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<ISupplierLookup> _mockSupplierLookup = new(MockBehavior.Loose);

        public MaintenanceRequestVendorValidationTests()
        {
            _mockQueryRepo.Setup(r => r.GetMaintenanceExternalRequestTypeAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>
                {
                    new() { Id = ExternalTypeId }
                });
        }

        private CreateMaintenanceRequestCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockDeptLookup.Object, _mockSupplierLookup.Object);

        private UpdateMaintenanceRequestCommandValidator UpdateValidator() =>
            new(_mockQueryRepo.Object, _mockSupplierLookup.Object);

        [Fact]
        public async Task Create_External_NoVendor_FailsWithExactMessage()
        {
            var command = new CreateMaintenanceRequestCommand
            {
                RequestTypeId = ExternalTypeId,
                MaintenanceTypeId = 1,
                MachineId = 10,
                VendorId = null
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.VendorId)
                  .WithErrorMessage("Please select a vendor.");
        }

        [Fact]
        public async Task Create_External_InvalidVendor_FailsWithSupplierMessage()
        {
            _mockSupplierLookup
                .Setup(s => s.GetActiveSupplierByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync((SupplierLookupDto?)null);

            var command = new CreateMaintenanceRequestCommand
            {
                RequestTypeId = ExternalTypeId,
                MaintenanceTypeId = 1,
                MachineId = 10,
                VendorId = 5
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.VendorId)
                  .WithErrorMessage("Selected vendor is not a valid active supplier.");
        }

        [Fact]
        public async Task Create_External_ValidVendor_NoVendorError()
        {
            _mockSupplierLookup
                .Setup(s => s.GetActiveSupplierByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SupplierLookupDto { Id = 5, VendorCode = "V5", VendorName = "Valid Vendor" });

            var command = new CreateMaintenanceRequestCommand
            {
                RequestTypeId = ExternalTypeId,
                MaintenanceTypeId = 1,
                MachineId = 10,
                VendorId = 5
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.VendorId);
        }

        [Fact]
        public async Task Create_Internal_NoVendor_NoVendorError()
        {
            var command = new CreateMaintenanceRequestCommand
            {
                RequestTypeId = InternalTypeId,
                MaintenanceTypeId = 1,
                MachineId = 10,
                VendorId = null
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.VendorId);
        }

        [Fact]
        public async Task Update_External_NoVendor_FailsWithExactMessage()
        {
            var command = new UpdateMaintenanceRequestCommand
            {
                Id = 1,
                RequestTypeId = ExternalTypeId,
                MaintenanceTypeId = 1,
                MachineId = 10,
                VendorId = null
            };

            var result = await UpdateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.VendorId)
                  .WithErrorMessage("Please select a vendor.");
        }

        [Fact]
        public async Task Update_Internal_NoVendor_NoVendorError()
        {
            var command = new UpdateMaintenanceRequestCommand
            {
                Id = 1,
                RequestTypeId = InternalTypeId,
                MaintenanceTypeId = 1,
                MachineId = 10,
                VendorId = null
            };

            var result = await UpdateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.VendorId);
        }
    }
}
