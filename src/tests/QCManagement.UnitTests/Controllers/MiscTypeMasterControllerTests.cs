using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QCManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using QCManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster;
using QCManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster;
using QCManagement.Application.MiscTypeMaster.Dto;
using QCManagement.Application.MiscTypeMaster.Queries.GetAllMiscTypeMaster;
using QCManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using QCManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using QCManagement.Presentation.Controllers;

namespace QCManagement.UnitTests.Controllers;

public sealed class MiscTypeMasterControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private MiscTypeMasterController CreateSut() => new(_mockMediator.Object);

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllMiscTypeMasterQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<MiscTypeMasterDto>>
            {
                IsSuccess = true,
                Data = new List<MiscTypeMasterDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetAllMiscTypeMasterAsync(1, 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAllMiscTypeMasterQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<MiscTypeMasterDto>>
            {
                IsSuccess = true,
                Data = new List<MiscTypeMasterDto>(),
                TotalCount = 0
            });

        await CreateSut().GetAllMiscTypeMasterAsync(1, 10);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAllMiscTypeMasterQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MiscTypeMasterDto());

        var result = await CreateSut().GetMiscTypeMasterByIdAsync(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetMiscTypeMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MiscTypeMasterLookupDto>() as IReadOnlyList<MiscTypeMasterLookupDto>);

        var result = await CreateSut().GetMiscTypeMasterAutoCompleteAsync("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().CreateMiscTypeMaster(new CreateMiscTypeMasterCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

        var result = await CreateSut().UpdateMiscTypeMaster(new UpdateMiscTypeMasterCommand());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().DeleteMiscTypeMaster(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().DeleteMiscTypeMaster(1);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<DeleteMiscTypeMasterCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
