using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.CustomerVisit.Commands.CreateCustomerVisit;
using SalesManagement.Application.CustomerVisit.Commands.DeleteCustomerVisit;
using SalesManagement.Application.CustomerVisit.Commands.DeleteCustomerVisitImage;
using SalesManagement.Application.CustomerVisit.Commands.UpdateCustomerVisit;
using SalesManagement.Application.CustomerVisit.Commands.UploadCustomerVisitImage;
using SalesManagement.Application.CustomerVisit.Dto;
using SalesManagement.Application.CustomerVisit.Queries.GetAllCustomerVisit;
using SalesManagement.Application.CustomerVisit.Queries.GetCustomerVisitAutoComplete;
using SalesManagement.Application.CustomerVisit.Queries.GetCustomerVisitById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class CustomerVisitControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private CustomerVisitController CreateSut() => new(_mockMediator.Object);

    // ── GetAll ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllCustomerVisitQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<CustomerVisitDto>>
            {
                IsSuccess = true,
                Data = new List<CustomerVisitDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllCustomerVisitAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllCustomerVisitQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<CustomerVisitDto>>
            {
                IsSuccess = true,
                Data = new List<CustomerVisitDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllCustomerVisitAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllCustomerVisitQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetById ──────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetCustomerVisitByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerVisitDto());

        var result = await CreateSut().GetCustomerVisitByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── AutoComplete ─────────────────────────────────────────────

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetCustomerVisitAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CustomerVisitLookupDto>() as IReadOnlyList<CustomerVisitLookupDto>);

        var result = await CreateSut().GetCustomerVisitAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Create ───────────────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateCustomerVisitCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateCustomerVisit(new CreateCustomerVisitCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Update ───────────────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateCustomerVisitCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateCustomerVisit(new UpdateCustomerVisitCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Delete ───────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteCustomerVisitCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteCustomerVisit(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteCustomerVisitCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteCustomerVisit(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteCustomerVisitCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── UploadImage ──────────────────────────────────────────────

    [Fact]
    public async Task UploadImage_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UploadCustomerVisitImageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerVisitImageDto { ImageName = "test.jpg" });

        var result = await CreateSut().UploadCustomerVisitImage(new UploadCustomerVisitImageCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── DeleteImage ──────────────────────────────────────────────

    [Fact]
    public async Task DeleteImage_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteCustomerVisitImageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteCustomerVisitImage("test.jpg");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteImage_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteCustomerVisitImageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteCustomerVisitImage("test.jpg");

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteCustomerVisitImageCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
