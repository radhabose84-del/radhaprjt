using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QCManagement.Application.QualityParameter.Commands.CreateQualityParameter;
using QCManagement.Application.QualityParameter.Commands.DeleteQualityParameter;
using QCManagement.Application.QualityParameter.Commands.UpdateQualityParameter;
using QCManagement.Application.QualityParameter.Dto;
using QCManagement.Application.QualityParameter.Queries.GetAllQualityParameter;
using QCManagement.Application.QualityParameter.Queries.GetQualityParameterAutoComplete;
using QCManagement.Application.QualityParameter.Queries.GetQualityParameterById;
using QCManagement.Presentation.Controllers;

namespace QCManagement.UnitTests.Controllers;

public sealed class QualityParameterControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private QualityParameterController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllQualityParameterQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<QualityParameterDto>>
            {
                IsSuccess = true,
                Data = new List<QualityParameterDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllQualityParameterAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllQualityParameterQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<QualityParameterDto>>
            {
                IsSuccess = true,
                Data = new List<QualityParameterDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllQualityParameterAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllQualityParameterQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetQualityParameterByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QualityParameterDto());

        var result = await CreateSut().GetQualityParameterByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetQualityParameterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<QualityParameterLookupDto>() as IReadOnlyList<QualityParameterLookupDto>);

        var result = await CreateSut().GetQualityParameterAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateQualityParameterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateQualityParameter(new CreateQualityParameterCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateQualityParameterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateQualityParameter(new UpdateQualityParameterCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteQualityParameterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteQualityParameter(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteQualityParameterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteQualityParameter(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteQualityParameterCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
