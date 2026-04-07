using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Commands.CreateComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Presentation.Validation.Complaint;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.Complaint;

public sealed class CreateComplaintCommandValidatorTests
{
    private readonly Mock<IComplaintQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

    private CreateComplaintCommandValidator CreateValidator() =>
        new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

    private void SetupAllAsyncMocks(int customerId = 1)
    {
        _mockQueryRepo
            .Setup(r => r.CustomerExistsAsync(customerId))
            .ReturnsAsync(true);

        _mockQueryRepo
            .Setup(r => r.InvoiceBelongsToCustomerAsync(It.IsAny<int>(), customerId))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        SetupAllAsyncMocks();
        var command = new CreateComplaintCommand
        {
            CustomerId = 1,
            ComplaintDate = new DateOnly(2026, 1, 1),
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
    public async Task Validate_MissingCustomerId_FailsValidation()
    {
        // CustomerId=0 won't trigger FK check (.When(x => x.CustomerId > 0))
        // but still need to setup for InvoiceBelongsToCustomer if Details present
        _mockQueryRepo.Setup(r => r.InvoiceBelongsToCustomerAsync(It.IsAny<int>(), 0)).ReturnsAsync(true);

        var command = new CreateComplaintCommand
        {
            CustomerId = 0,
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

        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public async Task Validate_NullDetails_FailsValidation()
    {
        SetupAllAsyncMocks();
        var command = new CreateComplaintCommand
        {
            CustomerId = 1,
            Details = null
        };

        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task Validate_EmptyDetails_FailsValidation()
    {
        SetupAllAsyncMocks();
        var command = new CreateComplaintCommand
        {
            CustomerId = 1,
            Details = new List<CreateComplaintDetailDto>()
        };

        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task Validate_InvalidCustomerId_FailsValidation()
    {
        _mockQueryRepo
            .Setup(r => r.CustomerExistsAsync(999))
            .ReturnsAsync(false);

        _mockQueryRepo
            .Setup(r => r.InvoiceBelongsToCustomerAsync(It.IsAny<int>(), 999))
            .ReturnsAsync(true);

        var command = new CreateComplaintCommand
        {
            CustomerId = 999,
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

        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }
}
