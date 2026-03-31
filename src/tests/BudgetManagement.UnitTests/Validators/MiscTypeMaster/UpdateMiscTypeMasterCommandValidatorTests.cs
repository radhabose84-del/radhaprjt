using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using BudgetManagement.Infrastructure.Data;
using BudgetManagement.Presentation.Validation.Common;
using BudgetManagement.Presentation.Validation.MiscTypeMaster;
using BudgetManagement.UnitTests.TestData;
using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;

namespace BudgetManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class UpdateMiscTypeMasterCommandValidatorTests : IDisposable
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly ApplicationDbContext _dbContext;
        private readonly MaxLengthProvider _maxLengthProvider;

        public UpdateMiscTypeMasterCommandValidatorTests()
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

        private UpdateMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _maxLengthProvider);

        private void SetupAllAsyncMocks(UpdateMiscTypeMasterCommand command, bool alreadyExists = false, bool notFound = true)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(command.MiscTypeCode!, command.Id))
                .ReturnsAsync(alreadyExists);

            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(command.Id))
                .ReturnsAsync(notFound);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();
            SetupAllAsyncMocks(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyMiscTypeCode_FailsValidation(string? code)
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(miscTypeCode: code);

            if (!string.IsNullOrEmpty(code))
            {
                _mockQueryRepo
                    .Setup(r => r.AlreadyExistsAsync(code, command.Id))
                    .ReturnsAsync(false);
            }

            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(command.Id))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(description: description);
            SetupAllAsyncMocks(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_DuplicateMiscTypeCode_FailsAlreadyExistsRule()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(miscTypeCode: "EXIST001");
            SetupAllAsyncMocks(command, alreadyExists: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode)
                .WithErrorMessage("MiscTypeCode already exists.");
        }

        [Fact]
        public async Task Validate_EntityNotFound_FailsNotFoundRule()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(id: 999);
            SetupAllAsyncMocks(command, alreadyExists: false, notFound: false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_MiscTypeCodeExceedsMaxLength_FailsMaxLengthRule()
        {
            var longCode = new string('A', 60);
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(miscTypeCode: longCode);
            SetupAllAsyncMocks(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Fact]
        public async Task Validate_DescriptionExceedsMaxLength_FailsMaxLengthRule()
        {
            var longDesc = new string('X', 260);
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(description: longDesc);
            SetupAllAsyncMocks(command);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }
    }
}
