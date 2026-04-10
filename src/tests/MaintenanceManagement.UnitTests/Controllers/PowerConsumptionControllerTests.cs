using Contracts.Common;
using MaintenanceManagement.Application.Power.PowerConsumption.Command.CreatePowerConsumption;
using MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetClosingReaderValueById;
using MaintenanceManagement.Application.Power.PowerConsumption.Queries;
using MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetFeederSubFeederById;
using MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetPowerConsumption;
using MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetPowerConsumptionById;
using MaintenanceManagement.Presentation.Controllers.Power;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class PowerConsumptionControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private PowerConsumptionController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPowerConsumptionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetPowerConsumptionDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            var result = await CreateSut().GetAllPowerConsumptionAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPowerConsumptionByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetPowerConsumptionDto());

            var result = await CreateSut().GetByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetFeederSubFeeder_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetFeederSubFeederByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetFeederSubFeederDto>());

            var result = await CreateSut().GetFeederSubFeederByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetOpeningReaderValue_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetClosingReaderValueByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetClosingReaderValueDto());

            var result = await CreateSut().GetOpeningReaderValueIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreatePowerConsumptionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreatePowerConsumptionCommand());
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
