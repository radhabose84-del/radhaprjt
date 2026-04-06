using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Entity.Commands.UpdateEntity;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.Entity;

namespace UserManagement.UnitTests.Validators.Entity
{
    public sealed class UpdateEntityCommandValidatorTests
    {
        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"UpdateEntityValidatorDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private static UpdateEntityCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider());

        private static UpdateEntityCommand ValidCommand() =>
            new UpdateEntityCommand
            {
                Id = 1,
                EntityName = "Updated Entity",
                EntityDescription = "Updated description",
                Address = "456 Updated Street",
                Phone = "9876543211",
                Email = "updated@entity.com",
                IsActive = 1
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyEntityName_FailsValidation(string? name)
        {
            var command = ValidCommand();
            command.EntityName = name;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.EntityName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyAddress_FailsValidation(string? address)
        {
            var command = ValidCommand();
            command.Address = address;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Address);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyPhone_FailsValidation(string? phone)
        {
            var command = ValidCommand();
            command.Phone = phone;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Phone);
        }

        [Fact]
        public async Task Validate_EntityNameExceedsMaxLength_FailsValidation()
        {
            var command = ValidCommand();
            command.EntityName = new string('A', 101);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.EntityName);
        }

        [Fact]
        public async Task Validate_InvalidPhoneFormat_FailsValidation()
        {
            var command = ValidCommand();
            command.Phone = "12345";
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Phone);
        }
    }
}
