using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesLead.Commands.CreateSalesLead;
using SalesManagement.Application.SalesLead.Commands.DeleteSalesLead;
using SalesManagement.Application.SalesLead.Commands.UpdateSalesLead;
using SalesManagement.Application.SalesLead.Dto;
using SalesManagement.Application.SalesLead.Queries.GetAllSalesLead;
using SalesManagement.Application.SalesLead.Queries.GetSalesLeadAutoComplete;
using SalesManagement.Application.SalesLead.Queries.GetSalesLeadById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class SalesLeadControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private SalesLeadController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllSalesLeadQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesLeadDto>>
            {
                IsSuccess = true,
                Data = new List<SalesLeadDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllSalesLeadAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllSalesLeadQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesLeadDto>>
            {
                IsSuccess = true,
                Data = new List<SalesLeadDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllSalesLeadAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllSalesLeadQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesLeadByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesLeadDto());

        var result = await CreateSut().GetSalesLeadByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesLeadAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesLeadLookupDto>() as IReadOnlyList<SalesLeadLookupDto>);

        var result = await CreateSut().GetSalesLeadAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateSalesLeadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateSalesLead(new CreateSalesLeadCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateSalesLeadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateSalesLead(new UpdateSalesLeadCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesLeadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteSalesLead(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesLeadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteSalesLead(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteSalesLeadCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
