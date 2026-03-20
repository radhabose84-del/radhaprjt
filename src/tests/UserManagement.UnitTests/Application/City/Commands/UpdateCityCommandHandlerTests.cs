using AutoMapper;
using MediatR;
using UserManagement.Application.City.Commands.UpdateCity;
using UserManagement.Application.City.Queries.GetCities;
using UserManagement.Application.Common.Interfaces.ICity;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using FluentValidation;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Application.City.Commands
{
    public sealed class UpdateCityCommandHandlerTests
    {
        private readonly Mock<ICityCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICityQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private UpdateCityCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCityDto()
        {
            var command = CityBuilders.ValidUpdateCommand();
            var existingEntity = CityBuilders.ValidEntity();
            var updatedEntity = CityBuilders.ValidEntity();
            var dto = CityBuilders.ValidDto(cityName: "Updated City");

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(existingEntity);

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
                .Returns(updatedEntity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<Cities>()))
                .ReturnsAsync(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(updatedEntity);

            _mockMapper
                .Setup(m => m.Map<CityDto>(updatedEntity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.CityName.Should().Be("Updated City");
        }

        [Fact]
        public async Task Handle_CityNotFound_ThrowsValidationException()
        {
            var command = CityBuilders.ValidUpdateCommand(id: 999);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Cities?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*does not exist*");
        }

        [Fact]
        public async Task Handle_DeletedCity_ThrowsValidationException()
        {
            var command = CityBuilders.ValidUpdateCommand(id: 1);
            var deletedEntity = CityBuilders.ValidEntity();
            deletedEntity.IsDeleted = IsDelete.Deleted;

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(deletedEntity);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*does not exist or is deleted*");
        }

        [Fact]
        public async Task Handle_InactivateLinkedCity_ThrowsValidationException()
        {
            var command = CityBuilders.ValidUpdateCommand(isActive: 0);
            var existingEntity = CityBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(existingEntity);

            _mockQueryRepo
                .Setup(r => r.IsCityLinkedAsync(command.Id))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*linked with other records*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = CityBuilders.ValidUpdateCommand();
            var existingEntity = CityBuilders.ValidEntity();
            var updatedEntity = CityBuilders.ValidEntity();
            var dto = CityBuilders.ValidDto();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(existingEntity);

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
                .Returns(updatedEntity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(command.Id, It.IsAny<Cities>()))
                .ReturnsAsync(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(updatedEntity);

            _mockMapper
                .Setup(m => m.Map<CityDto>(updatedEntity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.Module == "City"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
