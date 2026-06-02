using Contracts.Common;
using Microsoft.AspNetCore.Mvc;
using QCManagement.Application.QcInspection.Commands.CreateQcInspection;
using QCManagement.Application.QcInspection.Commands.DeleteQcInspection;
using QCManagement.Application.QcInspection.Commands.SaveDisposition;
using QCManagement.Application.QcInspection.Commands.SaveParameterCollection;
using QCManagement.Application.QcInspection.Dto;
using QCManagement.Application.QcInspection.Queries.GetAllQcInspection;
using QCManagement.Application.QcInspection.Queries.GetGrnQcStatus;
using QCManagement.Application.QcInspection.Queries.GetQcInspectionById;
using QCManagement.Presentation.Controllers;

namespace QCManagement.UnitTests.Controllers
{
    public sealed class QcInspectionControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private QcInspectionController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllQcInspectionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<QcInspectionListDto>> { IsSuccess = true, Data = new(), TotalCount = 0 });

            (await CreateSut().GetAllQcInspectionAsync(1, 10)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorOnce()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllQcInspectionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<QcInspectionListDto>> { IsSuccess = true, Data = new() });

            await CreateSut().GetAllQcInspectionAsync(1, 10);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAllQcInspectionQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetQcInspectionByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QcInspectionDto());
            (await CreateSut().GetQcInspectionByIdAsync(1)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GrnStatus_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetGrnQcStatusQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GrnQcStatusDto());
            (await CreateSut().GetGrnQcStatusAsync(100)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateQcInspectionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<QcInspectionDto> { IsSuccess = true, Data = new QcInspectionDto { Id = 1 } });
            (await CreateSut().CreateQcInspection(new CreateQcInspectionCommand())).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SaveParameters_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<SaveParameterCollectionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });
            (await CreateSut().SaveParameters(new SaveParameterCollectionCommand())).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SaveDisposition_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<SaveDispositionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });
            (await CreateSut().SaveDisposition(new SaveDispositionCommand())).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOk()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteQcInspectionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            (await CreateSut().DeleteQcInspection(1)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorOnce()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteQcInspectionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            await CreateSut().DeleteQcInspection(1);
            _mockMediator.Verify(m => m.Send(It.IsAny<DeleteQcInspectionCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
