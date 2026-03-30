using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Application.MiscMaster.Command.CreateMiscMaster;
using BudgetManagement.Infrastructure.Data;
using BudgetManagement.Presentation.Validation.Common;
using BudgetManagement.Presentation.Validation.MiscMaster;
using BudgetManagement.UnitTests.TestData;
using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;

namespace BudgetManagement.UnitTests.Validators.MiscMaster
{
    public sealed class CreateMiscMasterCommandValidatorTests : IDisposable
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly ApplicationDbContext _dbContext;
        private readonly MaxLengthProvider _maxLengthProvider;

        public CreateMiscMasterCommandValidatorTests()
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

        private CreateMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _maxLengthProvider);

        private void SetupAlreadyExists(string code, int miscTypeId, bool exists = false, int? excludeId = null)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(code, miscTypeId, excludeId))
                .ReturnsAsync(exists);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupAlreadyExists(command.Code!, command.MiscTypeId, exists: false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsNotFoundRule(string? code)
        {
            var command = MiscMasterBuilders.ValidCreateCommand(code: code);

            // AlreadyExists rule uses .When guard: skip setting up if code is null/empty
            if (!string.IsNullOrEmpty(code))
                SetupAlreadyExists(code, command.MiscTypeId, exists: false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsNotFoundRule(string? description)
        {
            var command = MiscMasterBuilders.ValidCreateCommand(description: description);
            SetupAlreadyExists(command.Code!, command.MiscTypeId, exists: false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsAlreadyExistsRule()
        {
            var command = MiscMasterBuilders.ValidCreateCommand(code: "EXIST001");
            SetupAlreadyExists("EXIST001", command.MiscTypeId, exists: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_CodeExceedsMaxLength_FailsMaxLengthRule()
        {
            var longCode = new string('A', 60);
            var command = MiscMasterBuilders.ValidCreateCommand(code: longCode);
            SetupAlreadyExists(longCode, command.MiscTypeId, exists: false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public async Task Validate_DescriptionExceedsMaxLength_FailsMaxLengthRule()
        {
            var longDesc = new string('X', 260);
            var command = MiscMasterBuilders.ValidCreateCommand(description: longDesc);
            SetupAlreadyExists(command.Code!, command.MiscTypeId, exists: false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_ZeroMiscTypeId_FailsValidation()
        {
            var command = MiscMasterBuilders.ValidCreateCommand(miscTypeId: 0);
            SetupAlreadyExists(command.Code!, 0, exists: false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeId);
        }
    }
}
