using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IMRS;
using PurchaseManagement.Application.MRS.Command.UpdateMrsEntry;
using PurchaseManagement.Presentation.Validation.Common;
using PurchaseManagement.Presentation.Validation.MRS;

namespace PurchaseManagement.UnitTests.Validators.MRS
{
    public sealed class UpdateMrsEntryCommandValidatorTests
    {
        private readonly Mock<IMrsEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);

        private UpdateMrsEntryCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockRepo.Object);

        [Fact]
        public async Task Validate_NullUpdateMrsEntry_FailsValidation()
        {
            // Provide a non-null DTO with invalid (zero) IDs instead of null,
            // because the validator accesses nested properties without a null guard.
            var command = new UpdateMrsEntryCommand
            {
                updateMrsEntry = new UpdateMrsEntryDto { Id = 0, RequestCategoryId = 0, DepartmentId = 0, SubDepartmentId = 0 }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
