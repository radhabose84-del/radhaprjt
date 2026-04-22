using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.RawMaterialType.Commands.CreateRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Commands.UpdateRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Commands.DeleteRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Queries.GetAllRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Queries.GetRawMaterialTypeById;
using ProductionManagement.Application.RawMaterialType.Queries.GetRawMaterialTypeAutoComplete;
using ProductionManagement.Application.RawMaterialType.Dto;
using ProductionManagement.Presentation.Controllers;

namespace ProductionManagement.UnitTests.Controllers
{
    public sealed class RawMaterialTypeControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private RawMaterialTypeController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllRawMaterialTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<RawMaterialTypeDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            var result = await CreateSut().GetAllRawMaterialTypeAsync(1, 10, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllRawMaterialTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<RawMaterialTypeDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            await CreateSut().GetAllRawMaterialTypeAsync(1, 10, null);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAllRawMaterialTypeQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetRawMaterialTypeByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RawMaterialTypeDto { Id = 1 });

            var result = await CreateSut().GetRawMaterialTypeByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetRawMaterialTypeAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IReadOnlyList<RawMaterialTypeLookupDto>>(new List<RawMaterialTypeLookupDto>()));

            var result = await CreateSut().GetRawMaterialTypeAutoCompleteAsync(null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateRawMaterialTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateRawMaterialType(new CreateRawMaterialTypeCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateRawMaterialTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateRawMaterialType(new UpdateRawMaterialTypeCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteRawMaterialTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteRawMaterialType(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteRawMaterialTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteRawMaterialType(1);

            _mockMediator.Verify(m => m.Send(It.IsAny<DeleteRawMaterialTypeCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
