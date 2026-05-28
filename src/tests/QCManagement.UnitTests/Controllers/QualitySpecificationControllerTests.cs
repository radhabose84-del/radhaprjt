using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QCManagement.Application.QualitySpecification.Commands.CreateQualitySpecification;
using QCManagement.Application.QualitySpecification.Commands.DeleteQualitySpecification;
using QCManagement.Application.QualitySpecification.Commands.UpdateQualitySpecification;
using QCManagement.Application.QualitySpecification.Dto;
using QCManagement.Application.QualitySpecification.Queries.GetAllQualitySpecification;
using QCManagement.Application.QualitySpecification.Queries.GetQualitySpecificationAutoComplete;
using QCManagement.Application.QualitySpecification.Queries.GetQualitySpecificationById;
using QCManagement.Presentation.Controllers;

namespace QCManagement.UnitTests.Controllers;

public sealed class QualitySpecificationControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private QualitySpecificationController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllQualitySpecificationQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<QualitySpecificationListDto>>
            {
                IsSuccess = true,
                Data = new List<QualitySpecificationListDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllQualitySpecificationAsync(1, 10);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllQualitySpecificationQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<QualitySpecificationListDto>>
            {
                IsSuccess = true,
                Data = new List<QualitySpecificationListDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllQualitySpecificationAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllQualitySpecificationQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetQualitySpecificationByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QualitySpecificationDto());

        var result = await CreateSut().GetQualitySpecificationByIdAsync(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetQualitySpecificationAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<QualitySpecificationLookupDto>() as IReadOnlyList<QualitySpecificationLookupDto>);

        var result = await CreateSut().GetQualitySpecificationAutoCompleteAsync("test");
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateQualitySpecificationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateQualitySpecification(new CreateQualitySpecificationCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateQualitySpecificationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateQualitySpecification(new UpdateQualitySpecificationCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteQualitySpecificationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteQualitySpecification(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteQualitySpecificationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteQualitySpecification(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteQualitySpecificationCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
