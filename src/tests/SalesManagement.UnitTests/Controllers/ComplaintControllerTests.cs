using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.Complaint.Commands.CreateComplaint;
using SalesManagement.Application.Complaint.Commands.DeleteAttachment;
using SalesManagement.Application.Complaint.Commands.DeleteComplaint;
using SalesManagement.Application.Complaint.Commands.UpdateComplaint;
using SalesManagement.Application.Complaint.Commands.UploadAttachment;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Application.Complaint.Queries.GetAllComplaint;
using SalesManagement.Application.Complaint.Queries.GetComplaintAutoComplete;
using SalesManagement.Application.Complaint.Queries.GetComplaintById;
using SalesManagement.Application.Complaint.Queries.GetCustomerInvoices;
using SalesManagement.Application.Complaint.Queries.GetInvoiceLineDetails;
using SalesManagement.Application.Complaint.Queries.GetPendingComplaint;
using SalesManagement.Application.Complaint.Queries.SearchInvoices;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class ComplaintControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private ComplaintController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllComplaintQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<ComplaintHeaderDto>>
            {
                IsSuccess = true,
                Data = new List<ComplaintHeaderDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllComplaintAsync(1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllComplaintQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<ComplaintHeaderDto>>
            {
                IsSuccess = true,
                Data = new List<ComplaintHeaderDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllComplaintAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllComplaintQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPending_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetPendingComplaintQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PendingComplaintListDto>(), 0));

        var result = await CreateSut().GetPendingComplaintAsync(1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetComplaintByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ComplaintHeaderDto { Id = 1 });

        var result = await CreateSut().GetComplaintByIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetComplaintAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ComplaintLookupDto>() as IReadOnlyList<ComplaintLookupDto>);

        var result = await CreateSut().GetComplaintAutoCompleteAsync("test");
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetCustomerInvoices_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetCustomerInvoicesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CustomerInvoiceDto>());

        var result = await CreateSut().GetCustomerInvoicesAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetInvoiceLineDetails_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetInvoiceLineDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InvoiceLineDetailDto>());

        var result = await CreateSut().GetInvoiceLineDetailsAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SearchInvoices_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<SearchInvoicesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<InvoiceSearchDto>>
            {
                IsSuccess = true,
                Data = new List<InvoiceSearchDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().SearchInvoicesAsync(1, 1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateComplaintCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Complaint created successfully.",
                Data = 1
            });

        var result = await CreateSut().CreateComplaint(new CreateComplaintCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateComplaintCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Complaint updated successfully.",
                Data = 1
            });

        var result = await CreateSut().UpdateComplaint(new UpdateComplaintCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteComplaintCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteComplaint(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UploadAttachment_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UploadComplaintAttachmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ComplaintAttachmentDto { Id = 1 });

        var result = await CreateSut().UploadAttachment(new UploadComplaintAttachmentCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteAttachment_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteComplaintAttachmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteAttachment(1);
        result.Should().BeOfType<OkObjectResult>();
    }
}
