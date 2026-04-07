using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using GateEntryManagement.Presentation.Controllers;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.CreateVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.UpdateVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.DeleteVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Queries.GetAllVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Queries.GetVehicleMovementRecordById;
using GateEntryManagement.Application.VehicleMovementRecord.Queries.GetVehicleMovementRecordAutoComplete;
using GateEntryManagement.Application.VehicleMovementRecord.Queries.GetPendingVehicle;
using GateEntryManagement.Application.VehicleMovementRecord.Dto;

namespace GateEntryManagement.UnitTests.Controllers
{
    public sealed class VehicleMovementRecordControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private VehicleMovementRecordController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllVehicleMovementRecordQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<VehicleMovementRecordDto>>
                {
                    IsSuccess = true,
                    Data = new List<VehicleMovementRecordDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllVehicleMovementRecordAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllVehicleMovementRecordQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<VehicleMovementRecordDto>>
                {
                    IsSuccess = true,
                    Data = new List<VehicleMovementRecordDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllVehicleMovementRecordAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllVehicleMovementRecordQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetVehicleMovementRecordByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new VehicleMovementRecordDto());

            var result = await CreateSut().GetVehicleMovementRecordByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetVehicleMovementRecordAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<VehicleMovementRecordAutoCompleteDto>() as IReadOnlyList<VehicleMovementRecordAutoCompleteDto>);

            var result = await CreateSut().GetVehicleMovementRecordAutoCompleteAsync("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingVehicles_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPendingVehicleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<PendingVehicleDto>>
                {
                    IsSuccess = true,
                    Data = new List<PendingVehicleDto>(),
                    TotalCount = 0
                });

            var result = await CreateSut().GetPendingVehiclesAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingVehicles_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPendingVehicleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<PendingVehicleDto>>
                {
                    IsSuccess = true,
                    Data = new List<PendingVehicleDto>(),
                    TotalCount = 0
                });

            await CreateSut().GetPendingVehiclesAsync();

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetPendingVehicleQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateVehicleMovementRecordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Message = "Vehicle Movement Record created successfully.",
                    Data = 1
                });

            var result = await CreateSut().CreateVehicleMovementRecord(new CreateVehicleMovementRecordCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateVehicleMovementRecordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Message = "Vehicle Movement Record updated successfully.",
                    Data = 1
                });

            var result = await CreateSut().UpdateVehicleMovementRecord(new UpdateVehicleMovementRecordCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteVehicleMovementRecordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteVehicleMovementRecord(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteVehicleMovementRecordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteVehicleMovementRecord(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteVehicleMovementRecordCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
