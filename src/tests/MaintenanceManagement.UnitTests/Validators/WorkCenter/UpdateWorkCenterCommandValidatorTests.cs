using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Command.UpdateWorkCenter;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.WorkCenter;

namespace MaintenanceManagement.UnitTests.Validators.WorkCenter
{
    public sealed class UpdateWorkCenterCommandValidatorTests
    {
        private readonly Mock<IWorkCenterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IWorkCenterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateWorkCenterCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockCommandRepo.Object, _mockQueryRepo.Object);

        private void SetupAllMocks(int id = 1, string name = "Assembly Line")
        {
            _mockCommandRepo.Setup(r => r.IsNameDuplicateAsync(name, id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(new MaintenanceManagement.Domain.Entities.WorkCenter { Id = id });
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateWorkCenterCommand
            {
                Id = 1,
                WorkCenterName = "Assembly Line",
                UnitId = 1,
                DepartmentId = 1
            };
            SetupAllMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var command = new UpdateWorkCenterCommand
            {
                Id = 1,
                WorkCenterName = name,
                UnitId = 1,
                DepartmentId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            var command = new UpdateWorkCenterCommand
            {
                Id = 1,
                WorkCenterName = "Assembly Line",
                UnitId = 1,
                DepartmentId = 1
            };
            _mockCommandRepo.Setup(r => r.IsNameDuplicateAsync("Assembly Line", 1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new MaintenanceManagement.Domain.Entities.WorkCenter { Id = 1 });

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new UpdateWorkCenterCommand
            {
                Id = 99,
                WorkCenterName = "Assembly Line",
                UnitId = 1,
                DepartmentId = 1
            };
            _mockCommandRepo.Setup(r => r.IsNameDuplicateAsync("Assembly Line", 99)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((MaintenanceManagement.Domain.Entities.WorkCenter?)null);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }
    }
}
