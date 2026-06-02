using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.CancelPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.CreatePurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.DeletePurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.UpdatePurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetAllPurchaseReturns;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetPurchaseReturnAutoComplete;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetPurchaseReturnById;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetReturnableQtyByGrn;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers;

public sealed class PurchaseReturnControllerTests
{
    private readonly Mock<ISender> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IValidator<CreatePurchaseReturnCommand>> _mockCreateVal = new(MockBehavior.Loose);
    private readonly Mock<IValidator<UpdatePurchaseReturnCommand>> _mockUpdateVal = new(MockBehavior.Loose);
    private readonly Mock<IValidator<DeletePurchaseReturnCommand>> _mockDeleteVal = new(MockBehavior.Loose);

    private PurchaseReturnController CreateSut() =>
        new(_mockMediator.Object, _mockCreateVal.Object, _mockUpdateVal.Object, _mockDeleteVal.Object);

    private static ValidationResult Valid() => new();

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator.Setup(m => m.Send(It.IsAny<GetAllPurchaseReturnsQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new PagedResult<PurchaseReturnListItemDto> { Items = new List<PurchaseReturnListItemDto>(), Total = 0 });
        var result = await CreateSut().GetAll(1, 20, null);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator.Setup(m => m.Send(It.IsAny<GetPurchaseReturnByIdQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(PurchaseReturnBuilders.ValidHeaderDto(1));
        var result = await CreateSut().GetById(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator.Setup(m => m.Send(It.IsAny<GetPurchaseReturnAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<PurchaseReturnListItemDto>());
        var result = await CreateSut().AutoComplete("RTV");
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetReturnableQty_ReturnsOkResult()
    {
        _mockMediator.Setup(m => m.Send(It.IsAny<GetReturnableQtyByGrnQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<ReturnableQtyDto>());
        var result = await CreateSut().GetReturnableQty(200);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_Valid_ReturnsOkResult()
    {
        _mockCreateVal.Setup(v => v.ValidateAsync(It.IsAny<CreatePurchaseReturnCommand>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(Valid());
        _mockMediator.Setup(m => m.Send(It.IsAny<CreatePurchaseReturnCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(PurchaseReturnBuilders.ValidHeaderDto(1));

        var result = await CreateSut().Create(PurchaseReturnBuilders.ValidCreateCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_RouteIdMismatch_ReturnsBadRequest()
    {
        var result = await CreateSut().Update(999, PurchaseReturnBuilders.ValidUpdateCommand(1));
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_Valid_ReturnsOkResult()
    {
        _mockUpdateVal.Setup(v => v.ValidateAsync(It.IsAny<UpdatePurchaseReturnCommand>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(Valid());
        _mockMediator.Setup(m => m.Send(It.IsAny<UpdatePurchaseReturnCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(PurchaseReturnBuilders.ValidHeaderDto(1));

        var result = await CreateSut().Update(1, PurchaseReturnBuilders.ValidUpdateCommand(1));
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Cancel_ReturnsOkResult()
    {
        _mockMediator.Setup(m => m.Send(It.IsAny<CancelPurchaseReturnCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);
        var result = await CreateSut().Cancel(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_Valid_ReturnsOkResult()
    {
        _mockDeleteVal.Setup(v => v.ValidateAsync(It.IsAny<DeletePurchaseReturnCommand>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(Valid());
        _mockMediator.Setup(m => m.Send(It.IsAny<DeletePurchaseReturnCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);

        var result = await CreateSut().Delete(1);
        result.Should().BeOfType<OkObjectResult>();
    }
}
