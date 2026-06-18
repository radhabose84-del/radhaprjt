using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using BudgetManagement.Infrastructure.Data;
using BudgetManagement.Presentation.Validation.Common;
using BudgetManagement.Presentation.Validation.MiscMaster;
using BudgetManagement.UnitTests.TestData;
using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;

namespace BudgetManagement.UnitTests.Validators.MiscMaster
{
    public sealed class UpdateMiscMasterCommandValidatorTests : IDisposable
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly ApplicationDbContext _dbContext;
        private readonly MaxLengthProvider _maxLengthProvider;

        public UpdateMiscMasterCommandValidatorTests()
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

        private UpdateMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _maxLengthProvider);

        private void SetupAllAsyncMocks(UpdateMiscMasterCommand command)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(command.Code!, command.MiscTypeId, command.Id))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(command.Id))
                .ReturnsAsync(false); // entity EXISTS → NotFoundAsync (count==0) is false; validator uses !NotFoundAsync
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand();
            SetupAllAsyncMocks(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(code: code);

            if (!string.IsNullOrEmpty(code))
            {
                _mockQueryRepo
                    .Setup(r => r.AlreadyExistsAsync(code, command.MiscTypeId, command.Id))
                    .ReturnsAsync(false);
            }

            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(command.Id))
                .ReturnsAsync(false); // entity EXISTS → NotFoundAsync (count==0) is false; validator uses !NotFoundAsync

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(description: description);
            SetupAllAsyncMocks(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsAlreadyExistsRule()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(code: "EXIST001");

            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("EXIST001", command.MiscTypeId, command.Id))
                .ReturnsAsync(true);

            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(command.Id))
                .ReturnsAsync(false); // entity EXISTS → NotFoundAsync (count==0) is false; validator uses !NotFoundAsync

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_EntityNotFound_FailsNotFoundRule()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(id: 999);

            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(command.Code!, command.MiscTypeId, 999))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(999))
                .ReturnsAsync(true); // entity MISSING → NotFoundAsync (count==0) is true → !NotFoundAsync fails the rule

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_CodeExceedsMaxLength_FailsMaxLengthRule()
        {
            var longCode = new string('A', 60);
            var command = MiscMasterBuilders.ValidUpdateCommand(code: longCode);

            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(longCode, command.MiscTypeId, command.Id))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(command.Id))
                .ReturnsAsync(false); // entity EXISTS → NotFoundAsync (count==0) is false; validator uses !NotFoundAsync

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public async Task Validate_DescriptionExceedsMaxLength_FailsMaxLengthRule()
        {
            var longDesc = new string('X', 260);
            var command = MiscMasterBuilders.ValidUpdateCommand(description: longDesc);
            SetupAllAsyncMocks(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }
    }
}
