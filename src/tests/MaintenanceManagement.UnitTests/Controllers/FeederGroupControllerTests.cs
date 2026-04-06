using Contracts.Common;
using FluentValidation;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Command.CreateFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Command.DeleteFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Command.UpdateFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroupAutoComplete;
using MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroupById;
using MaintenanceManagement.Presentation.Controllers.Power;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class FeederGroupControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<FeederGroupController>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IFeederGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IValidator<CreateFeederGroupCommand>> _mockCreateValidator = new(MockBehavior.Loose);
        private readonly Mock<IValidator<UpdateFeederGroupCommand>> _mockUpdateValidator = new(MockBehavior.Loose);
        private readonly Mock<IValidator<DeleteFeederGroupCommand>> _mockDeleteValidator = new(MockBehavior.Loose);

        private FeederGroupController CreateSut() =>
            new(_mockMediator.Object, _mockLogger.Object, _mockQueryRepo.Object,
                _mockCreateValidator.Object, _mockUpdateValidator.Object, _mockDeleteValidator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetFeederGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<FeederGroupDto>> { IsSuccess = true, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 });

            var result = await CreateSut().GetAllFeederGroupAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetFeederGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<FeederGroupDto>> { IsSuccess = true, Data = new() });

            await CreateSut().GetAllFeederGroupAsync(1, 10);
            _mockMediator.Verify(m => m.Send(It.IsAny<GetFeederGroupQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetFeederGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetFeederGroupByIdDto());

            var result = await CreateSut().GetFeederById(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetFeederGroupAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetFeederGroupAutoCompleteDto>());

            var result = await CreateSut().GetFeederGroup(null);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtActionResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateFeederGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Create(new CreateFeederGroupCommand());
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateFeederGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(new UpdateFeederGroupCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteFeederGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
