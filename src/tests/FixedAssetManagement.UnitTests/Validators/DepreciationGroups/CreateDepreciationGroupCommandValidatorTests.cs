using FAM.Application.DepreciationGroup.Commands.CreateDepreciationGroup;
using FAM.Infrastructure.Data;
using FAM.Presentation.Validation.Common;
using FAM.Presentation.Validation.DepreciationGroup;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;
using Microsoft.EntityFrameworkCore;

namespace FixedAssetManagement.UnitTests.Validators.DepreciationGroups
{
    public sealed class CreateDepreciationGroupCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private ApplicationDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var mockIp = new Mock<Contracts.Interfaces.IIPAddressService>(MockBehavior.Loose);
            mockIp.Setup(s => s.GetUserId()).Returns(1);
            mockIp.Setup(s => s.GetUserName()).Returns("test");
            mockIp.Setup(s => s.GetCompanyId()).Returns((int?)1);
            mockIp.Setup(s => s.GetUnitId()).Returns((int?)1);
            mockIp.Setup(s => s.GetGroupCode()).Returns("GRP");
            mockIp.Setup(s => s.GetOldUnitId()).Returns("UNIT");

            var mockTz = new Mock<FAM.Application.Common.Interfaces.ITimeZoneService>(MockBehavior.Loose);
            mockTz.Setup(s => s.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);

            return new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
        }

        private CreateDepreciationGroupCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, CreateInMemoryDbContext());

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = DepreciationGroupBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = DepreciationGroupBuilders.ValidCreateCommand(code: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var command = DepreciationGroupBuilders.ValidCreateCommand(name: name!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_SpecialCharCode_FailsValidation()
        {
            var command = DepreciationGroupBuilders.ValidCreateCommand(code: "DG@001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
