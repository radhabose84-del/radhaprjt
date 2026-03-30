using Contracts.Common;
using FluentValidation;
using FluentValidation.Results;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Application.MiscMaster.Command.CreateMiscMaster;
using InventoryManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using InventoryManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using InventoryManagement.Application.MiscMaster.Queries.GetMiscMaster;
using InventoryManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using InventoryManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using InventoryManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.UnitTests.Controllers
{
    public sealed class MiscMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<CreateMiscMasterCommand>> _mockCreateValidator = new(MockBehavior.Loose);
        private readonly Mock<IValidator<UpdateMiscMasterCommand>> _mockUpdateValidator = new(MockBehavior.Loose);
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
                .ReturnsAsync(new GetMiscMasterDto { Id = 1 });

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMiscMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetMiscMasterAutoCompleteDto>());

            var result = await CreateSut().GetMiscMaster("test", "TYPE001", null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetMiscMasterDto { Id = 1 });

            var result = await CreateSut().CreateAsync(new CreateMiscMasterCommand
            {
                Code = "MSC001",
                Description = "Test",
                MiscTypeId = 1
            });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateMiscMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(new UpdateMiscMasterCommand
            {
                Id = 1,
                Description = "Updated",
                IsActive = 1
            });

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
