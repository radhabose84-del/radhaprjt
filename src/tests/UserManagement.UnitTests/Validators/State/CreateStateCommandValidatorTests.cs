using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.State.Commands.CreateState;
using UserManagement.Domain.Entities;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.State;
using UserManagement.Infrastructure.Data;
using UserManagement.UnitTests.TestData;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.UnitTests.Validators.State
{
    public sealed class CreateStateCommandValidatorTests
    {
        private CreateStateCommandValidator CreateValidator()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetGroupCode()).Returns("ADMIN");
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");

            var tzMock = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tzMock.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTime.UtcNow);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var db = new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
            var maxLenProvider = new MaxLengthProvider(db);

            return new CreateStateCommandValidator(maxLenProvider);
        }

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            var command = StateBuilders.ValidCreateCommand();
            var validator = CreateValidator();

            var result = await validator.TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyStateName_Fails(string? name)
        {
            var command = StateBuilders.ValidCreateCommand(stateName: name);
            var validator = CreateValidator();

            var result = await validator.TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.StateName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyStateCode_Fails(string? code)
        {
            var command = StateBuilders.ValidCreateCommand(stateCode: code);
            var validator = CreateValidator();

            var result = await validator.TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.StateCode);
        }

        [Fact]
        public async Task Validate_StateCodeExceedsMaxLength_Fails()
        {
            var command = StateBuilders.ValidCreateCommand(stateCode: "ABCDEFGHIJ");
            var validator = CreateValidator();

            var result = await validator.TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.StateCode);
        }

        [Fact]
        public async Task Validate_StateNameExceedsMaxLength_Fails()
        {
            var command = StateBuilders.ValidCreateCommand(
                stateName: new string('A', 100));
            var validator = CreateValidator();

            var result = await validator.TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.StateName);
        }
    }
}
