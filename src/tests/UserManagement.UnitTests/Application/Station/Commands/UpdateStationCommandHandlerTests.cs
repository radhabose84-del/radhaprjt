using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IStation;
using UserManagement.Application.Station.Command.UpdateStation;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Application.Station.Commands
{
    public sealed class UpdateStationCommandHandlerTests
    {
        private readonly Mock<IStationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IStationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateStationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockQueryRepo.Object, _mockMediator.Object);

        private static UpdateStationCommand ValidCommand(int id = 1) =>
            new()
            {
                Id = id,
                StationName = "Updated Station",
                Description = "Updated description",
                IsActive = Status.Active
            };

        private static UserManagement.Domain.Entities.Station ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                Code = "STA-0001",
                StationName = "Original Station"
            };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsOne()
        {
            var command = ValidCommand();
            _mockQueryRepo.Setup(r => r.GetStationByIdAsync(command.Id)).ReturnsAsync(ValidEntity());
            _mockCommandRepo.Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Station>())).ReturnsAsync(true);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = ValidCommand();
            _mockQueryRepo.Setup(r => r.GetStationByIdAsync(command.Id)).ReturnsAsync(ValidEntity());
            _mockCommandRepo.Setup(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Station>())).ReturnsAsync(true);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(command.Id, It.IsAny<UserManagement.Domain.Entities.Station>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            var command = ValidCommand(99);
            _mockQueryRepo.Setup(r => r.GetStationByIdAsync(99)).ReturnsAsync((UserManagement.Domain.Entities.Station?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*not found*");
        }
    }
}
