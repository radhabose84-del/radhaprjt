using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using PurchaseManagement.Application.Port.Commands;
using PurchaseManagement.Application.Port.Dto;
using PurchaseManagement.Application.Port.Queries.GetAllPorts;
using PurchaseManagement.Application.Port.Queries.GetById;
using PurchaseManagement.Application.Port.Queries.GetPortAutocomplete;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class PortMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IValidator<CreatePortMasterCommand>> _mockCreateValidator = new(MockBehavior.Loose);
        private readonly Mock<IValidator<UpdatePortMasterCommand>> _mockUpdateValidator = new(MockBehavior.Loose);
        private readonly Mock<IValidator<DeletePortMasterCommand>> _mockDeleteValidator = new(MockBehavior.Loose);

        private PortMasterController CreateSut() =>
            new(_mockMediator.Object, _mockCreateValidator.Object, _mockUpdateValidator.Object, _mockDeleteValidator.Object);

        private void SetupValidValidation()
        {
            _mockCreateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CreatePortMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _mockUpdateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<UpdatePortMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _mockDeleteValidator
                .Setup(v => v.ValidateAsync(It.IsAny<DeletePortMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllPortsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<PortMasterDto> { Items = new List<PortMasterDto>(), Total = 0 });

            var result = await CreateSut().GetAll(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllPortsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<PortMasterDto> { Items = new List<PortMasterDto>(), Total = 0 });

            await CreateSut().GetAll(1, 10);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAllPortsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPortByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PortMasterBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPortAutocompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PortLookupDto> { PortMasterBuilders.ValidLookupDto() });

            var result = await CreateSut().AutoComplete("PORT");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ValidCommand_ReturnsOkResult()
        {
            SetupValidValidation();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePortMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PortMasterBuilders.ValidDto());

            var result = await CreateSut().CreateAsync(PortMasterBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_InvalidCommand_ReturnsBadRequest()
        {
            var errors = new List<ValidationFailure> { new ValidationFailure("PortCode", "Port Code is required.") };
            _mockCreateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CreatePortMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(errors));

            var result = await CreateSut().CreateAsync(PortMasterBuilders.ValidCreateCommand());

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_ValidCommand_ReturnsOkResult()
        {
            SetupValidValidation();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdatePortMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PortMasterBuilders.ValidDto());

            var result = await CreateSut().Update(PortMasterBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeletePortMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeletePortMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Delete(1);

            _mockMediator.Verify(m => m.Send(It.IsAny<DeletePortMasterCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
