using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Commands.UpdateComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Presentation.Validation.Complaint;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.Complaint;

public sealed class UpdateComplaintCommandValidatorTests
{
    private readonly Mock<IComplaintQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

    private UpdateComplaintCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

    private void SetupAllAsyncMocks(int id = 1, int customerId = 1)
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.CustomerExistsAsync(customerId)).ReturnsAsync(true);
        _mockQueryRepo
            .Setup(r => r.InvoiceBelongsToCustomerAsync(It.IsAny<int>(), customerId))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        SetupAllAsyncMocks();
        var command = new UpdateComplaintCommand
        {
            Id = 1,
            CustomerId = 1,
            ComplaintDate = new DateOnly(2026, 1, 1),
            IsActive = 1,
            Details = new List<CreateComplaintDetailDto>
            {
                new CreateComplaintDetailDto
                {
                    InvoiceHeaderId = 1,
                    ItemId = 1,
                    NumberOfPacks = 10,
                    NetWeight = 100m,
                    InvoiceAmount = 5000m,
                    NatureOfComplaintIds = new List<int> { 1 }
                }
            }
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_NotFoundId_FailsValidation()
    {
        _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(true);
        _mockQueryRepo
            .Setup(r => r.InvoiceBelongsToCustomerAsync(It.IsAny<int>(), 1))
            .ReturnsAsync(true);

        var command = new UpdateComplaintCommand
        {
            Id = 99,
            CustomerId = 1,
            IsActive = 1,
            Details = new List<CreateComplaintDetailDto>
            {
                new CreateComplaintDetailDto
                {
                    InvoiceHeaderId = 1,
                    ItemId = 1,
                    NumberOfPacks = 10,
                    NetWeight = 100m,
                    NatureOfComplaintIds = new List<int> { 1 }
                }
            }
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(99)]
    public async Task Validate_InvalidIsActive_FailsValidation(int isActive)
    {
        SetupAllAsyncMocks();
        var command = new UpdateComplaintCommand
        {
            Id = 1,
            CustomerId = 1,
            IsActive = isActive,
            Details = new List<CreateComplaintDetailDto>
            {
                new CreateComplaintDetailDto
                {
                    InvoiceHeaderId = 1,
                    ItemId = 1,
                    NumberOfPacks = 10,
                    NetWeight = 100m,
                    NatureOfComplaintIds = new List<int> { 1 }
                }
            }
        };

        var result = await CreateValidator().TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.IsActive);
    }
}
