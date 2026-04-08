using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.UpdateServiceEntrySheet;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetAllSES;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetApprovedPOList;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetPoServiceHeaderByPoId;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetScheduleByPoIdandSeviceidandServiceItemid;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetServicePOLines;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetServiceScheduleByPoAndServiceId;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetSESListToApprove;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.ServiceEntrySheetGetById;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllSES;
using PurchaseManagement.Presentation.Controllers.PurchaseOrder;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class SESControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private SESController CreateSut() => new(_mockMediator.Object);

        // --- GetServicePoHeaderByPoId ---

        [Fact]
        public async Task GetServicePoHeaderByPoId_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPoServiceHeaderByPoIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PoServiceHeaderByIdDto());

            var result = await CreateSut().GetServicePoHeaderByPoId(1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetServicePoHeaderByPoId_WhenNull_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPoServiceHeaderByPoIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PoServiceHeaderByIdDto?)null);

            var result = await CreateSut().GetServicePoHeaderByPoId(999, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // --- GetSchedulesByPoAndServiceId ---

        [Fact]
        public async Task GetSchedulesByPoAndServiceId_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetByPoAndServiceIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ServiceScheduleDto> { new() });

            var result = await CreateSut().GetSchedulesByPoAndServiceId(1, 1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetSchedulesByPoAndServiceId_WhenEmpty_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetByPoAndServiceIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ServiceScheduleDto>());

            var result = await CreateSut().GetSchedulesByPoAndServiceId(1, 1, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetSchedulesByPoAndServiceId_WhenNull_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetByPoAndServiceIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<ServiceScheduleDto>?)null);

            var result = await CreateSut().GetSchedulesByPoAndServiceId(1, 1, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // --- GetApprovedList ---

        [Fact]
        public async Task GetApprovedList_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetApprovedPOListQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PoIdNumberDto>());

            var result = await CreateSut().GetApprovedList(CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetServicePOLinesAsync ---

        [Fact]
        public async Task GetServicePOLinesAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetServicePOLinesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetServicePOLinesDto>());

            var result = await CreateSut().GetServicePOLinesAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- CreateSes ---

        [Fact]
        public async Task CreateSes_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateServiceEntrySheetCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateSes(new CreateServiceSheetDto(), CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateSes_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateServiceEntrySheetCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateSes(new CreateServiceSheetDto(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateServiceEntrySheetCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // --- UpdateServiceEntrySheetAsync ---

        [Fact]
        public async Task UpdateServiceEntrySheetAsync_WithValidDto_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateServiceEntrySheetCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var dto = new CreateServiceSheetDto { Id = 1 };
            var result = await CreateSut().UpdateServiceEntrySheetAsync(dto);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateServiceEntrySheetAsync_WithNullDto_ReturnsBadRequest()
        {
            var result = await CreateSut().UpdateServiceEntrySheetAsync(null!);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateServiceEntrySheetAsync_WithZeroId_ReturnsBadRequest()
        {
            var dto = new CreateServiceSheetDto { Id = 0 };
            var result = await CreateSut().UpdateServiceEntrySheetAsync(dto);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- GetSesCreateSourceAsync ---

        [Fact]
        public async Task GetSesCreateSourceAsync_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetSESCreateSourceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SesFromScheduleRawDto { Id = 1 });

            var result = await CreateSut().GetSesCreateSourceAsync(1, 1, 1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetSesCreateSourceAsync_WhenNull_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetSESCreateSourceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SesFromScheduleRawDto?)null);

            var result = await CreateSut().GetSesCreateSourceAsync(1, 1, 1, CancellationToken.None);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // --- GetSesById ---

        [Fact]
        public async Task GetSesById_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetServiceEntrySheetByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<ServiceEntrySheetDetailDto?>
                {
                    IsSuccess = true,
                    Data = new ServiceEntrySheetDetailDto()
                });

            var result = await CreateSut().GetSesById(1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetSesById_WhenNull_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetServiceEntrySheetByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ApiResponseDTO<ServiceEntrySheetDetailDto?>?)null);

            var result = await CreateSut().GetSesById(999, CancellationToken.None);

            result.Should().BeOfType<NotFoundResult>();
        }

        // --- GetSesForApproval ---

        [Fact]
        public async Task GetSesForApproval_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetSesApprovalListQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SesApprovalListDto>());

            var result = await CreateSut().GetSesForApproval(null, null, null, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetSesWithActivities ---

        [Fact]
        public async Task GetSesWithActivities_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetServiceEntrySheetsWithActivitiesByPoIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ServiceEntrySheetWithActivitiesDto>>
                {
                    IsSuccess = true,
                    Message = "OK",
                    Data = new List<ServiceEntrySheetWithActivitiesDto>()
                });

            var result = await CreateSut().GetSesWithActivities(1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetAllSesList ---

        [Fact]
        public async Task GetAllSesList_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetSESListQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetServiceEntrySheetListDto>>
                {
                    IsSuccess = true,
                    Message = "OK",
                    Data = new List<GetServiceEntrySheetListDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllSesList();

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- GetSesFullDetails ---

        [Fact]
        public async Task GetSesFullDetails_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetServiceEntrySheetByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<ServiceEntrySheetDetailDto?>
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "OK",
                    Data = new ServiceEntrySheetDetailDto()
                });

            var result = await CreateSut().GetSesFullDetails(1, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
