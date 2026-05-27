using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QCManagement.Application.QualityTemplate.Commands.CreateQualityTemplate;
using QCManagement.Application.QualityTemplate.Commands.DeleteQualityTemplate;
using QCManagement.Application.QualityTemplate.Commands.UpdateQualityTemplate;
using QCManagement.Application.QualityTemplate.Dto;
using QCManagement.Application.QualityTemplate.Queries.GetAllQualityTemplate;
using QCManagement.Application.QualityTemplate.Queries.GetQualityTemplateAutoComplete;
using QCManagement.Application.QualityTemplate.Queries.GetQualityTemplateById;
using QCManagement.Presentation.Controllers;

namespace QCManagement.UnitTests.Controllers;

public sealed class QualityTemplateControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private QualityTemplateController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllQualityTemplateQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<QualityTemplateListDto>>
            {
                IsSuccess = true,
                Data = new List<QualityTemplateListDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllQualityTemplateAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllQualityTemplateQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<QualityTemplateListDto>>
            {
                IsSuccess = true,
                Data = new List<QualityTemplateListDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllQualityTemplateAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllQualityTemplateQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetQualityTemplateByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QualityTemplateDto());

        var result = await CreateSut().GetQualityTemplateByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetQualityTemplateAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<QualityTemplateLookupDto>() as IReadOnlyList<QualityTemplateLookupDto>);

        var result = await CreateSut().GetQualityTemplateAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateQualityTemplateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateQualityTemplate(new CreateQualityTemplateCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateQualityTemplateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateQualityTemplate(new UpdateQualityTemplateCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteQualityTemplateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteQualityTemplate(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteQualityTemplateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteQualityTemplate(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteQualityTemplateCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
