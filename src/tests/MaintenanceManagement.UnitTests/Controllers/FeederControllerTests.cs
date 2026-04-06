using Contracts.Common;
using FluentValidation;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.CreateFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.DeleteFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.UpdateFeeder;
using MaintenanceManagement.Application.Power.Feeder.Queries.GetFeeder;
using MaintenanceManagement.Application.Power.Feeder.Queries.GetFeederAutoComplete;
using MaintenanceManagement.Application.Power.Feeder.Queries.GetFeederById;
using MaintenanceManagement.Presentation.Controllers.Power;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class FeederControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<FeederController>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IFeederQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IValidator<CreateFeederCommand>> _mockCreateValidator = new(MockBehavior.Loose);
        private readonly Mock<IValidator<UpdateFeederCommand>> _mockUpdateValidator = new(MockBehavior.Loose);
        private readonly Mock<IValidator<DeleteFeederCommand>> _mockDeleteValidator = new(MockBehavior.Loose);

        private FeederController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object, _mockQueryRepo.Object,
                _mockCreateValidator.Object, _mockUpdateValidator.Object, _mockDeleteValidator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetFeederQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetFeederDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            var result = await CreateSut().GetAllFeederAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetFeederQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetFeederDto>> { IsSuccess = true, Data = new() });

            await CreateSut().GetAllFeederAsync(1, 10);
            _mockMediator.Verify(m => m.Send(It.IsAny<GetFeederQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetFeederByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFeederByIdDto());

            var result = await CreateSut().GetById(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetFeederAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetFeederAutoCompleteDto>());

            var result = await CreateSut().GetFeeder(null);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsNotNull()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateFeederCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Create(new CreateFeederCommand());
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateFeederCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(new UpdateFeederCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteFeederCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
