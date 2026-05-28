using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.CreateReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.DeleteReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.UpdateReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Queries.GetAllReturnTypes;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Queries.GetReturnTypeAutoComplete;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Queries.GetReturnTypeById;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers;

public sealed class ReturnTypeControllerTests
{
    private readonly Mock<ISender> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IValidator<CreateReturnTypeCommand>> _mockCreateVal = new(MockBehavior.Loose);
    private readonly Mock<IValidator<UpdateReturnTypeCommand>> _mockUpdateVal = new(MockBehavior.Loose);
    private readonly Mock<IValidator<DeleteReturnTypeCommand>> _mockDeleteVal = new(MockBehavior.Loose);

    private ReturnTypeController CreateSut() =>
        new(_mockMediator.Object, _mockCreateVal.Object, _mockUpdateVal.Object, _mockDeleteVal.Object);

    private static ValidationResult Valid() => new();
    private static ValidationResult Invalid(string field, string error) => new(new[] { new ValidationFailure(field, error) });

    [Fact]
    public async Task GetAll_ReturnsOkResult()
    {
        _mockMediator.Setup(m => m.Send(It.IsAny<GetAllReturnTypesQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new PagedResult<ReturnTypeDto> { Items = new List<ReturnTypeDto>(), Total = 0 });

        var result = await CreateSut().GetAll(1, 20, null);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOkResult()
    {
        _mockMediator.Setup(m => m.Send(It.IsAny<GetReturnTypeByIdQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(ReturnTypeBuilders.ValidDto(1));

        var result = await CreateSut().GetById(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AutoComplete_ReturnsOkResult()
    {
        _mockMediator.Setup(m => m.Send(It.IsAny<GetReturnTypeAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<ReturnTypeLookupDto>());

        var result = await CreateSut().AutoComplete("R");
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ValidationFailure_ReturnsBadRequest()
    {
        _mockCreateVal.Setup(v => v.ValidateAsync(It.IsAny<CreateReturnTypeCommand>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(Invalid("Code", "Code is required."));

        var result = await CreateSut().Create(ReturnTypeBuilders.ValidCreateCommand());
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_Valid_ReturnsOkResult()
    {
        _mockCreateVal.Setup(v => v.ValidateAsync(It.IsAny<CreateReturnTypeCommand>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(Valid());
        _mockMediator.Setup(m => m.Send(It.IsAny<CreateReturnTypeCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(ReturnTypeBuilders.ValidDto(1));

        var result = await CreateSut().Create(ReturnTypeBuilders.ValidCreateCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_Valid_ReturnsOkResult()
    {
        _mockUpdateVal.Setup(v => v.ValidateAsync(It.IsAny<UpdateReturnTypeCommand>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(Valid());
        _mockMediator.Setup(m => m.Send(It.IsAny<UpdateReturnTypeCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(ReturnTypeBuilders.ValidDto(1));

        var result = await CreateSut().Update(ReturnTypeBuilders.ValidUpdateCommand());
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_Valid_ReturnsOkResult()
    {
        _mockDeleteVal.Setup(v => v.ValidateAsync(It.IsAny<DeleteReturnTypeCommand>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(Valid());
        _mockMediator.Setup(m => m.Send(It.IsAny<DeleteReturnTypeCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);

        var result = await CreateSut().Delete(1);
        result.Should().BeOfType<OkObjectResult>();
    }
}
