using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.ICustomField;
using UserManagement.Application.Common.Interfaces.IMenu;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Application.CustomFields.Commands.UpdateCustomField;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.CustomFields;

namespace UserManagement.UnitTests.Validators.CustomFields
{
    public sealed class UpdateCustomFieldCommandValidatorTests
    {
        private readonly Mock<ICustomFieldQuery> _mockCustomFieldQuery = new(MockBehavior.Strict);
        private readonly Mock<IUnitQueryRepository> _mockUnitQuery = new(MockBehavior.Strict);
        private readonly Mock<IMenuQuery> _mockMenuQuery = new(MockBehavior.Strict);

        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"UpdateCustomFieldDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private UpdateCustomFieldCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider(), _mockCustomFieldQuery.Object, _mockUnitQuery.Object, _mockMenuQuery.Object);

        private static UpdateCustomFieldCommand ValidCommand() =>
            new UpdateCustomFieldCommand
            {
                Id = 1,
                LabelName = "UpdatedLabel",
                DataTypeId = 1,
                LabelTypeId = 1,
                Menu = new List<CustomFieldMenuUpdateDto> { new CustomFieldMenuUpdateDto { MenuId = 1 } },
                Unit = new List<CustomFieldUnitUpdateDto> { new CustomFieldUnitUpdateDto { UnitId = 1 } }
            };

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockCustomFieldQuery
                .Setup(r => r.AlreadyExistsAsync("UpdatedLabel", id))
                .ReturnsAsync(false);
            _mockCustomFieldQuery
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);
            _mockUnitQuery
                .Setup(r => r.FKColumnExistValidation(1))
                .ReturnsAsync(true);
            _mockMenuQuery
                .Setup(r => r.FKColumnExistValidation(1))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = ValidCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            SetupAllAsyncMocks(0);
            var command = ValidCommand();
            command.Id = 0;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyLabelName_FailsValidation(string? labelName)
        {
            _mockCustomFieldQuery.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockCustomFieldQuery.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            _mockUnitQuery.Setup(r => r.FKColumnExistValidation(1)).ReturnsAsync(true);
            _mockMenuQuery.Setup(r => r.FKColumnExistValidation(1)).ReturnsAsync(true);
            var command = ValidCommand();
            command.LabelName = labelName;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.LabelName);
        }

        [Fact]
        public async Task Validate_ZeroDataTypeId_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = ValidCommand();
            command.DataTypeId = 0;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.DataTypeId);
        }

        [Fact]
        public async Task Validate_CustomFieldNotFound_FailsValidation()
        {
            _mockCustomFieldQuery
                .Setup(r => r.AlreadyExistsAsync("UpdatedLabel", 99))
                .ReturnsAsync(false);
            _mockCustomFieldQuery
                .Setup(r => r.NotFoundAsync(99))
                .ReturnsAsync(false);
            _mockUnitQuery
                .Setup(r => r.FKColumnExistValidation(1))
                .ReturnsAsync(true);
            _mockMenuQuery
                .Setup(r => r.FKColumnExistValidation(1))
                .ReturnsAsync(true);

            var command = ValidCommand();
            command.Id = 99;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }
    }
}
