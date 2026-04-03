using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.PackType.Commands.CreatePackType;
using ProductionManagement.Application.PackType.Commands.UpdatePackType;
using ProductionManagement.Application.PackType.Commands.DeletePackType;
using ProductionManagement.Application.PackType.Queries.GetAllPackType;
using ProductionManagement.Application.PackType.Queries.GetPackTypeById;
using ProductionManagement.Application.PackType.Queries.GetPackTypeAutoComplete;
using ProductionManagement.Application.PackType.Dto;
using ProductionManagement.Presentation.Controllers;
using AppLookupDto = Contracts.Dtos.Lookups.Production.PackTypeLookupDto;

namespace ProductionManagement.UnitTests.Controllers
{
    public sealed class PackTypeControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private PackTypeController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllPackTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<PackTypeDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            var result = await CreateSut().GetAllPackTypeAsync(1, 10, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllPackTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<PackTypeDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            await CreateSut().GetAllPackTypeAsync(1, 10, null);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAllPackTypeQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPackTypeByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PackTypeDto { Id = 1 });

            var result = await CreateSut().GetPackTypeByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPackTypeAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IReadOnlyList<AppLookupDto>>(new List<AppLookupDto>()));

            var result = await CreateSut().GetPackTypeAutoCompleteAsync(null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreatePackTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreatePackType(new CreatePackTypeCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdatePackTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdatePackType(new UpdatePackTypeCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeletePackTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeletePackType(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
