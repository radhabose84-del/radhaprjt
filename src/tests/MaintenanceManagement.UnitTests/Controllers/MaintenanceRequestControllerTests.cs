using Contracts.Common;
using FluentValidation;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.CreateMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestCommand;
using MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestStatusCommand;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetVendors;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExternalRequestById;
using Contracts.Dtos.Lookups.Party;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceDipatchMode;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceExternalRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequestById;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequestType;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceServiceLocation;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceServiceType;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequestForServicePoById;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequestsForServicePo;
using Contracts.Dtos.Lookups.Maintenance;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MaintenanceManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class MaintenanceRequestControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<MaintenanceRequestController>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IValidator<CreateMaintenanceRequestCommand>> _mockCreateValidator = new(MockBehavior.Loose);
        private readonly Mock<IValidator<UpdateMaintenanceRequestCommand>> _mockUpdateValidator = new(MockBehavior.Loose);
        private readonly Mock<IMaintenanceCategoryQueryRepository> _mockCategoryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMaintenanceRequestQueryRepository> _mockRequestRepo = new(MockBehavior.Loose);

        private MaintenanceRequestController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object, _mockCreateValidator.Object,
                _mockUpdateValidator.Object, _mockCategoryRepo.Object, _mockRequestRepo.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMaintenanceRequestQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMaintenanceRequestDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            var result = await CreateSut().GetAllMaintenanceRequestAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllExternal_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMaintenanceExternalRequestQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMaintenanceExternalRequestDto>> { IsSuccess = true, Data = new() { new() } });

            var result = await CreateSut().GetAllMaintenanceExternalRequestAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMaintenanceRequestByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<GetMaintenanceRequestDto> { IsSuccess = true });

            var result = await CreateSut().GetByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetExternalRequestsByIds_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetExternalRequestsByIdsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetExternalRequestByIdDto>> { IsSuccess = true, Data = new() { new() } });

            var result = await CreateSut().GetExternalRequestsByIds("1,2,3");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockUpdateValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateMaintenanceRequestCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateMaintenanceRequestCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool> { IsSuccess = true });

            var result = await CreateSut().Update(new UpdateMaintenanceRequestCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMaintenanceStatusDesc_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMaintenanceRequestTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscMasterDto>> { IsSuccess = true, Data = new() { new() } });

            var result = await CreateSut().GetMaintenanceStatusDescAsync();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMaintenanceServiceDesc_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMaintenanceServiceTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscMasterDto>> { IsSuccess = true, Data = new() { new() } });

            var result = await CreateSut().GetMaintenanceServiceDescAsync();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMaintenanceServiceLocationDesc_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMaintenanceServiceLocationQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscMasterDto>> { IsSuccess = true, Data = new() { new() } });

            var result = await CreateSut().GetMaintenanceServiceLocationDescAsync();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMaintenanceDispatchModeDesc_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMaintenanceDispatchModeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetMiscMasterDto>> { IsSuccess = true, Data = new() { new() } });

            var result = await CreateSut().GetMaintenanceDispatchModeDescAsync();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetVendors_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetVendorsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<SupplierLookupDto>>
                {
                    IsSuccess = true,
                    Data = new() { new SupplierLookupDto { Id = 1, VendorCode = "V001", VendorName = "Acme" } }
                });

            var result = await CreateSut().GetVendors("Acme");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetVendors_CallsMediatorSend_Once()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetVendorsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<SupplierLookupDto>> { IsSuccess = true, Data = new() });

            await CreateSut().GetVendors(null);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetVendorsQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ── ESR linkage endpoints (Service PO consumer) ────────────────────────

        [Fact]
        public async Task GetForServicePo_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMaintenanceRequestsForServicePoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<MaintenanceRequestLookupDto>>
                {
                    IsSuccess = true,
                    Data = new() { new MaintenanceRequestLookupDto { Id = 1, RequestNo = "MR-1" } },
                    TotalCount = 1
                });

            var result = await CreateSut().GetForServicePoAsync("MR-1");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetForServicePo_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMaintenanceRequestsForServicePoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<MaintenanceRequestLookupDto>>
                {
                    IsSuccess = true, Data = new(), TotalCount = 0
                });

            await CreateSut().GetForServicePoAsync(null);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetMaintenanceRequestsForServicePoQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetForServicePoById_ReturnsOkResult_WhenFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMaintenanceRequestForServicePoByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<MaintenanceRequestLookupDto?>
                {
                    IsSuccess = true,
                    Data = new MaintenanceRequestLookupDto { Id = 501, RequestNo = "MR-501" }
                });

            var result = await CreateSut().GetForServicePoByIdAsync(501);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetForServicePoById_ReturnsOkResult_WhenNotFound()
        {
            // Controller still returns 200 with isSuccess=false; matches existing pattern in this codebase
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMaintenanceRequestForServicePoByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<MaintenanceRequestLookupDto?>
                {
                    IsSuccess = false, Message = "MaintenanceRequest not found.", Data = null
                });

            var result = await CreateSut().GetForServicePoByIdAsync(999999);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
