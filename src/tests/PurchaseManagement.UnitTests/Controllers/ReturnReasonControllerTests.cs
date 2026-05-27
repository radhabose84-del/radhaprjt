using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.CreateReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.DeleteReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.UpdateReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetAllReturnReasons;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetReturnReasonAutoComplete;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetReturnReasonById;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetReturnReasonsByReturnType;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers;

public sealed class ReturnReasonControllerTests
{
    private readonly Mock<ISender> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IValidator<CreateReturnReasonCommand>> _mockCreateVal = new(MockBehavior.Loose);
    private readonly Mock<IValidator<UpdateReturnReasonCommand>> _mockUpdateVal = new(MockBehavior.Loose);
    private readonly Mock<IValidator<DeleteReturnReasonCommand>> _mockDeleteVal = new(MockBehavior.Loose);

    private ReturnReasonController CreateSut() =>
        new(_mockMediator.Object, _mockCreateVal.Object, _mockUpdateVal.Object, _mockDeleteVal.Object);

    private static ValidationResult Valid() => new();

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator.Setup(m => m.Send(It.IsAny<GetAllReturnReasonsQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new PagedResult<ReturnReasonDto> { Items = new List<ReturnReasonDto>(), Total = 0 });
        var result = await CreateSut().GetAll(1, 20, null);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator.Setup(m => m.Send(It.IsAny<GetReturnReasonByIdQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(ReturnReasonBuilders.ValidDto(1));
        var result = await CreateSut().GetById(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator.Setup(m => m.Send(It.IsAny<GetReturnReasonAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<ReturnReasonLookupDto>());
        var result = await CreateSut().AutoComplete("M");
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByReturnType_ReturnsOkResult()
    {
        _mockMediator.Setup(m => m.Send(It.IsAny<GetReturnReasonsByReturnTypeQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<ReturnReasonLookupDto>());
        var result = await CreateSut().GetByReturnType(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_Valid_ReturnsOkResult()
    {
        _mockCreateVal.Setup(v => v.ValidateAsync(It.IsAny<CreateReturnReasonCommand>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(Valid());
        _mockMediator.Setup(m => m.Send(It.IsAny<CreateReturnReasonCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(ReturnReasonBuilders.ValidDto(1));

        var result = await CreateSut().Create(ReturnReasonBuilders.ValidCreateCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_Valid_ReturnsOkResult()
    {
        _mockUpdateVal.Setup(v => v.ValidateAsync(It.IsAny<UpdateReturnReasonCommand>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(Valid());
        _mockMediator.Setup(m => m.Send(It.IsAny<UpdateReturnReasonCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(ReturnReasonBuilders.ValidDto(1));

        var result = await CreateSut().Update(ReturnReasonBuilders.ValidUpdateCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_Valid_ReturnsOkResult()
    {
        _mockDeleteVal.Setup(v => v.ValidateAsync(It.IsAny<DeleteReturnReasonCommand>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(Valid());
        _mockMediator.Setup(m => m.Send(It.IsAny<DeleteReturnReasonCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);

        var result = await CreateSut().Delete(1);
        result.Should().BeOfType<OkObjectResult>();
    }
}
