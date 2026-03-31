using AutoMapper;
using MediatR;
using UserManagement.Application.City.Commands.CreateCity;
using UserManagement.Application.City.Queries.GetCities;
using UserManagement.Application.Common.Interfaces.ICity;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using FluentValidation;

namespace UserManagement.UnitTests.Application.City.Commands
{
    public sealed class CreateCityCommandHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<ICityCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private CreateCityCommandHandler CreateSut() =>
            new(_mockMapper.Object, _mockCommandRepo.Object, _mockMediator.Object);

        private void SetupHappyPath(CreateCityCommand command, Cities createdEntity, CityDto dto)
        {
            _mockCommandRepo
                .Setup(r => r.StateExistsAsync(command.StateId))
                .ReturnsAsync(true);

            _mockCommandRepo
                .Setup(r => r.GetCityByNameAsync(
                    command.CityName ?? string.Empty,
                    command.CityCode ?? string.Empty,
                    command.StateId))
                .ReturnsAsync(new Cities { Id = 0 });

            _mockMapper
                .Setup(m => m.Map<Cities>(command))
                .Returns(createdEntity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<Cities>()))
                .ReturnsAsync(createdEntity);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockMapper
                .Setup(m => m.Map<CityDto>(createdEntity))
                .Returns(dto);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCityDto()
        {
            var command = CityBuilders.ValidCreateCommand();
            var entity = CityBuilders.ValidEntity();
            var dto = CityBuilders.ValidDto();
            SetupHappyPath(command, entity, dto);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.CityCode.Should().Be("CTY01");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = CityBuilders.ValidCreateCommand();
            var entity = CityBuilders.ValidEntity();
            var dto = CityBuilders.ValidDto();
            SetupHappyPath(command, entity, dto);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<Cities>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = CityBuilders.ValidCreateCommand();
            var entity = CityBuilders.ValidEntity();
            var dto = CityBuilders.ValidDto();
            SetupHappyPath(command, entity, dto);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "City"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidState_ThrowsValidationException()
        {
            var command = CityBuilders.ValidCreateCommand(stateId: 999);

            _mockCommandRepo
                .Setup(r => r.StateExistsAsync(999))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*state does not exist*");
        }

        [Fact]
        public async Task Handle_DuplicateCityName_ThrowsValidationException()
        {
            var command = CityBuilders.ValidCreateCommand();

            _mockCommandRepo
                .Setup(r => r.StateExistsAsync(command.StateId))
                .ReturnsAsync(true);

            _mockCommandRepo
                .Setup(r => r.GetCityByNameAsync(
                    command.CityName ?? string.Empty,
                    command.CityCode ?? string.Empty,
                    command.StateId))
                .ReturnsAsync(new Cities { Id = 5 });

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*already exists*");
        }
    }
}
