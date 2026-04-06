using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Commands.DeleteService;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Presentation.Validation.ServiceMaster;

namespace PurchaseManagement.UnitTests.Validators.ServiceMaster
{
    public sealed class SoftDeleteServiceCommandValidatorTests
    {
        private readonly Mock<IServiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private SoftDeleteServiceCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupGetById(int id, bool exists = true)
        {
            _mockQueryRepo
                .Setup(r => r.GetServiceMasterByIdAsync(id))
                .ReturnsAsync(exists
                    ? new GetServiceMasterDto
                    {
                        Id = id,
                        IsDeleted = BaseEntity.IsDelete.NotDeleted
                    }
                    : null!);
        }

        private void SetupHasDependencies(int id, bool hasDeps = false)
        {
            _mockQueryRepo
                .Setup(r => r.HasActiveDependenciesAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(hasDeps);
        }

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            SetupGetById(1);
            SetupHasDependencies(1);

            var result = await CreateValidator().TestValidateAsync(new DeleteServiceCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteServiceCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            SetupGetById(99, exists: false);
            SetupHasDependencies(99);

            var result = await CreateValidator().TestValidateAsync(new DeleteServiceCommand { Id = 99 });

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_HasActiveDependencies_FailsValidation()
        {
            SetupGetById(1);
            SetupHasDependencies(1, hasDeps: true);

            var result = await CreateValidator().TestValidateAsync(new DeleteServiceCommand { Id = 1 });

            result.ShouldHaveAnyValidationError();
        }
    }
}
