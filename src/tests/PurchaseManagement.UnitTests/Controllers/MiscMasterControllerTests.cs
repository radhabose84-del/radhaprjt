using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.MiscMaster.Command.CreateMiscMaster;
using PurchaseManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using PurchaseManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using PurchaseManagement.Application.MiscMaster.Queries.GetMiscMaster;
using PurchaseManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using PurchaseManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;
using Contracts.Common;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class MiscMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<CreateMiscMasterCommand>> _mockCreateValidator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<UpdateMiscMasterCommand>> _mockUpdateValidator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<DeleteMiscMasterCommand>> _mockDeleteValidator = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);

        private MiscMasterController CreateSut() =>
            new(_mockMediator.Object, _mockCreateValidator.Object, _mockUpdateValidator.Object,
                _mockCommandRepo.Object, _mockDeleteValidator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetMiscMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllMiscMasterAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MiscMasterBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetMiscMasterAutoCompleteDto>());

            var result = await CreateSut().GetMiscMaster("test", "TYPE001");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_WithValidCommand_ReturnsOkResult()
        {
            _mockCreateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CreateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MiscMasterBuilders.ValidDto());

            var result = await CreateSut().CreateAsync(MiscMasterBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_WithValidCommand_ReturnsOkResult()
        {
            _mockUpdateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(MiscMasterBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Delete(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteMiscMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
