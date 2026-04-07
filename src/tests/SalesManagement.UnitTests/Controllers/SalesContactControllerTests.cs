using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesContact.Commands.CreateSalesContact;
using SalesManagement.Application.SalesContact.Commands.DeleteSalesContact;
using SalesManagement.Application.SalesContact.Commands.UpdateSalesContact;
using SalesManagement.Application.SalesContact.Dto;
using SalesManagement.Application.SalesContact.Queries.GetAllSalesContact;
using SalesManagement.Application.SalesContact.Queries.GetSalesContactAutoComplete;
using SalesManagement.Application.SalesContact.Queries.GetSalesContactById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class SalesContactControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private SalesContactController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllSalesContactQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesContactDto>>
            {
                IsSuccess = true,
                Data = new List<SalesContactDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllSalesContactAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllSalesContactQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesContactDto>>
            {
                IsSuccess = true,
                Data = new List<SalesContactDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllSalesContactAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllSalesContactQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesContactByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesContactDto());

        var result = await CreateSut().GetSalesContactByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesContactAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesContactLookupDto>() as IReadOnlyList<SalesContactLookupDto>);

        var result = await CreateSut().GetSalesContactAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateSalesContactCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateSalesContact(new CreateSalesContactCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateSalesContactCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateSalesContact(new UpdateSalesContactCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesContactCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteSalesContact(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesContactCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteSalesContact(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteSalesContactCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
