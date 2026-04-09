using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesEnquiry.Commands.CreateSalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Commands.DeleteSalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Commands.UpdateSalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Dto;
using SalesManagement.Application.SalesEnquiry.Queries.GetAllSalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Queries.GetSalesEnquiryAutoComplete;
using SalesManagement.Application.SalesEnquiry.Queries.GetSalesEnquiryById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class SalesEnquiryControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private SalesEnquiryController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllSalesEnquiryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesEnquiryHeaderDto>>
            {
                IsSuccess = true,
                Data = new List<SalesEnquiryHeaderDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllSalesEnquiryAsync(1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllSalesEnquiryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesEnquiryHeaderDto>>
            {
                IsSuccess = true,
                Data = new List<SalesEnquiryHeaderDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllSalesEnquiryAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllSalesEnquiryQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesEnquiryByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesEnquiryHeaderDto { Id = 1 });

        var result = await CreateSut().GetSalesEnquiryByIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesEnquiryAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesEnquiryLookupDto>() as IReadOnlyList<SalesEnquiryLookupDto>);

        var result = await CreateSut().GetSalesEnquiryAutoCompleteAsync("test");
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateSalesEnquiryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateSalesEnquiryCommand
        {
            SalesEnquiryDetails = new CreateSalesEnquiryDto { PartyId = 1 }
        };
        var result = await CreateSut().CreateSalesEnquiry(command);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateSalesEnquiryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateSalesEnquiryCommand { Id = 1, PartyId = 1 };
        var result = await CreateSut().UpdateSalesEnquiry(command);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesEnquiryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteSalesEnquiry(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesEnquiryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteSalesEnquiry(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteSalesEnquiryCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
