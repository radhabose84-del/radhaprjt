using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using BudgetManagement.Infrastructure.Data;
using BudgetManagement.Presentation.Validation.Common;
using BudgetManagement.Presentation.Validation.MiscTypeMaster;
using BudgetManagement.UnitTests.TestData;
using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;

namespace BudgetManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class CreateMiscTypeMasterCommandValidatorTests : IDisposable
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly ApplicationDbContext _dbContext;
        private readonly MaxLengthProvider _maxLengthProvider;

        public CreateMiscTypeMasterCommandValidatorTests()
        {
            var mockIpService = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTimeZone = new Mock<ITimeZoneService>(MockBehavior.Loose);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options, mockIpService.Object, mockTimeZone.Object);
            _maxLengthProvider = new MaxLengthProvider(_dbContext);
        }

        public void Dispose() => _dbContext.Dispose();

        private CreateMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _maxLengthProvider);

        private void SetupAlreadyExists(string code, bool exists = false)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(code, null))
                .ReturnsAsync(exists);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();
            SetupAlreadyExists(command.MiscTypeCode!, exists: false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyMiscTypeCode_FailsValidation(string? code)
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(miscTypeCode: code);

            if (!string.IsNullOrEmpty(code))
                SetupAlreadyExists(code, exists: false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(description: description);
            SetupAlreadyExists(command.MiscTypeCode!, exists: false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_DuplicateMiscTypeCode_FailsAlreadyExistsRule()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(miscTypeCode: "EXIST001");
            SetupAlreadyExists("EXIST001", exists: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode)
                .WithErrorMessage("MiscTypeCode already exists.");
        }

        [Fact]
        public async Task Validate_MiscTypeCodeExceedsMaxLength_FailsMaxLengthRule()
        {
            var longCode = new string('A', 60);
            var command = MiscTypeMasterBuilders.ValidCreateCommand(miscTypeCode: longCode);
            SetupAlreadyExists(longCode, exists: false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Fact]
        public async Task Validate_DescriptionExceedsMaxLength_FailsMaxLengthRule()
        {
            var longDesc = new string('X', 260);
            var command = MiscTypeMasterBuilders.ValidCreateCommand(description: longDesc);
            SetupAlreadyExists(command.MiscTypeCode!, exists: false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_UniqueCode_PassesAlreadyExistsRule()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(miscTypeCode: "UNIQUE99");
            SetupAlreadyExists("UNIQUE99", exists: false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.MiscTypeCode);
        }
    }
}
