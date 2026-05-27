using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QCManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using QCManagement.Application.MiscMaster.Commands.DeleteMiscMaster;
using QCManagement.Application.MiscMaster.Commands.UpdateMiscMaster;
using QCManagement.Application.MiscMaster.Dto;
using QCManagement.Application.MiscMaster.Queries.GetAllMiscMaster;
using QCManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using QCManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using QCManagement.Presentation.Controllers;

namespace QCManagement.UnitTests.Controllers;

public sealed class MiscMasterControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private MiscMasterController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllMiscMasterQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<MiscMasterDto>>
            {
                IsSuccess = true,
                Data = new List<MiscMasterDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllMiscMasterAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllMiscMasterQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<MiscMasterDto>>
            {
                IsSuccess = true,
                Data = new List<MiscMasterDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllMiscMasterAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllMiscMasterQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetMiscMasterByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MiscMasterDto());

        var result = await CreateSut().GetMiscMasterByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetMiscMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MiscMasterLookupDto>() as IReadOnlyList<MiscMasterLookupDto>);

        var result = await CreateSut().GetMiscMasterAutoCompleteAsync("test", "QP_GROUP");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateMiscMaster(new CreateMiscMasterCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateMiscMaster(new UpdateMiscMasterCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteMiscMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteMiscMaster(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteMiscMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteMiscMaster(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteMiscMasterCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
