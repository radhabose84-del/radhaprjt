using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IMRS;
using InventoryManagement.Application.MRS.Command.UpdateMrsEntry;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.MRS;
using static InventoryManagement.Application.MRS.Command.UpdateMrsEntry.UpdateMrsEntryDto;

namespace InventoryManagement.UnitTests.Validators.MRS
{
    public sealed class UpdateMrsEntryCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });
        private readonly Mock<IMrsEntryQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateMrsEntryCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockQueryRepo.Object);

        private static UpdateMrsEntryCommand ValidCommand() => new()
        {
            updateMrsEntry = new UpdateMrsEntryDto
            {
                Id = 1,
                RequestCategoryId = 1,
                DepartmentId = 1,
                SubDepartmentId = 1,
                UpdateMrsDetails = new List<UpdateMrsDetailDto>
                {
                    new UpdateMrsDetailDto { MrsHeaderId = 1, ItemId = 1, UomId = 1, RequestQuantity = 5m }
                }
            }
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroRequestCategoryId_FailsValidation()
        {
            var command = ValidCommand();
            command.updateMrsEntry.RequestCategoryId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroDepartmentId_FailsValidation()
        {
            var command = ValidCommand();
            command.updateMrsEntry.DepartmentId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroSubDepartmentId_FailsValidation()
        {
            var command = ValidCommand();
            command.updateMrsEntry.SubDepartmentId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DetailWithZeroItemId_FailsValidation()
        {
            var command = ValidCommand();
            command.updateMrsEntry.UpdateMrsDetails[0].ItemId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
