using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesAgreement.Commands.CreateSalesAgreement;
using SalesManagement.Application.SalesAgreement.Commands.DeleteSalesAgreement;
using SalesManagement.Application.SalesAgreement.Dto;
using SalesManagement.Application.SalesAgreement.Queries.GetAllSalesAgreement;
using SalesManagement.Application.SalesAgreement.Queries.GetSalesAgreementAutoComplete;
using SalesManagement.Application.SalesAgreement.Queries.GetSalesAgreementById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class SalesAgreementControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private SalesAgreementController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllSalesAgreementQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<SalesAgreementHeaderDto>>
            {
                IsSuccess = true, Data = new List<SalesAgreementHeaderDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10
            });

        var result = await CreateSut().GetAllSalesAgreementAsync(1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesAgreementByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesAgreementHeaderDto());

        var result = await CreateSut().GetSalesAgreementByIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSalesAgreementAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesAgreementLookupDto>() as IReadOnlyList<SalesAgreementLookupDto>);

        var result = await CreateSut().GetSalesAgreementAutoCompleteAsync("test");
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateSalesAgreementCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateSalesAgreement(new CreateSalesAgreementCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesAgreementCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteSalesAgreement(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteSalesAgreementCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteSalesAgreement(1);

        _mockMediator.Verify(m => m.Send(It.IsAny<DeleteSalesAgreementCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
