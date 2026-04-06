using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.ICustomField;
using UserManagement.Application.Common.Interfaces.IMenu;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Application.CustomFields.Commands.CreateCustomField;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.CustomFields;

namespace UserManagement.UnitTests.Validators.CustomFields
{
    public sealed class CreateCustomFieldCommandValidatorTests
    {
        private readonly Mock<ICustomFieldQuery> _mockCustomFieldQuery = new(MockBehavior.Strict);
        private readonly Mock<IUnitQueryRepository> _mockUnitQuery = new(MockBehavior.Strict);
        private readonly Mock<IMenuQuery> _mockMenuQuery = new(MockBehavior.Strict);

        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"CustomFieldDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private CreateCustomFieldCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider(), _mockCustomFieldQuery.Object, _mockUnitQuery.Object, _mockMenuQuery.Object);

        private static CreateCustomFieldCommand ValidCommand() =>
            new CreateCustomFieldCommand
            {
                LabelName = "TestLabel",
                DataTypeId = 1,
                LabelTypeId = 1,
                Menu = new List<CustomFieldMenuDto> { new CustomFieldMenuDto { MenuId = 1 } },
                Unit = new List<CustomFieldUnitDto> { new CustomFieldUnitDto { UnitId = 1 } }
            };

        private void SetupAllAsyncMocks()
        {
            _mockCustomFieldQuery
                .Setup(r => r.AlreadyExistsAsync("TestLabel", null))
                .ReturnsAsync(false);
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

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyLabelName_FailsValidation(string? labelName)
        {
            SetupAllAsyncMocks();
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
        public async Task Validate_ZeroLabelTypeId_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = ValidCommand();
            command.LabelTypeId = 0;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.LabelTypeId);
        }

        [Fact]
        public async Task Validate_LabelNameExceedsMaxLength_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = ValidCommand();
            command.LabelName = new string('A', 51);
            _mockCustomFieldQuery
                .Setup(r => r.AlreadyExistsAsync(command.LabelName, null))
                .ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.LabelName);
        }

        [Fact]
        public async Task Validate_DuplicateLabelName_FailsValidation()
        {
            _mockCustomFieldQuery
                .Setup(r => r.AlreadyExistsAsync("TestLabel", null))
                .ReturnsAsync(true);
            _mockUnitQuery
                .Setup(r => r.FKColumnExistValidation(1))
                .ReturnsAsync(true);
            _mockMenuQuery
                .Setup(r => r.FKColumnExistValidation(1))
                .ReturnsAsync(true);

            var command = ValidCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_EmptyMenuList_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = ValidCommand();
            command.Menu = new List<CustomFieldMenuDto>();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Menu);
        }

        [Fact]
        public async Task Validate_EmptyUnitList_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = ValidCommand();
            command.Unit = new List<CustomFieldUnitDto>();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Unit);
        }
    }
}
