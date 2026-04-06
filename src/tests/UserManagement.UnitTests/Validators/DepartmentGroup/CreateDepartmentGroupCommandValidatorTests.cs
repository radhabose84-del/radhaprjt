using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IDepartmentGroup;
using UserManagement.Application.DepartmentGroup.Command.CreateDepartmentGroup;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.DepartmentGroup;

namespace UserManagement.UnitTests.Validators.DepartmentGroup
{
    public sealed class CreateDepartmentGroupCommandValidatorTests
    {
        private readonly Mock<IDepartmentGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"DepartmentGroupDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private CreateDepartmentGroupCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider(), _mockQueryRepo.Object);

        private static CreateDepartmentGroupCommand ValidCommand() =>
            new CreateDepartmentGroupCommand
            {
                DepartmentGroupName = "Engineering Group",
                DepartmentGroupCode = "ENG",
                IsActive = 1
            };

        private void SetupAllAsyncMocks(string name = "Engineering Group")
        {
            _mockQueryRepo
                .Setup(r => r.GetByDepartmentGroupNameAsync(name))
                .ReturnsAsync((UserManagement.Domain.Entities.DepartmentGroup?)null);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks(command.DepartmentGroupName!);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDepartmentGroupName_FailsValidation(string? name)
        {
            _mockQueryRepo
                .Setup(r => r.GetByDepartmentGroupNameAsync(It.IsAny<string>()))
                .ReturnsAsync((UserManagement.Domain.Entities.DepartmentGroup?)null);

            var command = ValidCommand();
            command.DepartmentGroupName = name;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.DepartmentGroupName);
        }

        [Fact]
        public async Task Validate_DepartmentGroupNameExceedsMaxLength_FailsValidation()
        {
            var longName = new string('A', 51);
            _mockQueryRepo
                .Setup(r => r.GetByDepartmentGroupNameAsync(longName))
                .ReturnsAsync((UserManagement.Domain.Entities.DepartmentGroup?)null);

            var command = ValidCommand();
            command.DepartmentGroupName = longName;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.DepartmentGroupName);
        }

        [Fact]
        public async Task Validate_DuplicateDepartmentGroupName_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.GetByDepartmentGroupNameAsync("Engineering Group"))
                .ReturnsAsync(new UserManagement.Domain.Entities.DepartmentGroup { Id = 1, DepartmentGroupName = "Engineering Group" });

            var command = ValidCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }
    }
}
