using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.QualityMaster.Commands.CreateQualityMaster;
using ProductionManagement.Application.QualityMaster.Commands.UpdateQualityMaster;
using ProductionManagement.Application.QualityMaster.Commands.DeleteQualityMaster;
using ProductionManagement.Application.QualityMaster.Queries.GetAllQualityMaster;
using ProductionManagement.Application.QualityMaster.Queries.GetQualityMasterById;
using ProductionManagement.Application.QualityMaster.Queries.GetQualityMasterAutoComplete;
using ProductionManagement.Application.QualityMaster.Dto;
using ProductionManagement.Presentation.Controllers;

namespace ProductionManagement.UnitTests.Controllers
{
    public sealed class QualityMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private QualityMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllQualityMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<QualityMasterDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            var result = await CreateSut().GetAllQualityMasterAsync(1, 10, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllQualityMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<QualityMasterDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            await CreateSut().GetAllQualityMasterAsync(1, 10, null);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAllQualityMasterQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetQualityMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QualityMasterDto { Id = 1 });

            var result = await CreateSut().GetQualityMasterByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetQualityMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<QualityMasterLookupDto>() as IReadOnlyList<QualityMasterLookupDto>);

            var result = await CreateSut().GetQualityMasterAutoCompleteAsync(null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateQualityMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateQualityMaster(new CreateQualityMasterCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateQualityMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateQualityMaster(new UpdateQualityMasterCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteQualityMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteQualityMaster(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
