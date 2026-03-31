using AutoMapper;
using Contracts.Interfaces.Lookups.FixedAssetManagement;
using MediatR;
using UserManagement.Application.City.Commands.DeleteCity;
using UserManagement.Application.City.Queries.GetCities;
using UserManagement.Application.Common.Interfaces.ICity;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using FluentValidation;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Application.City.Commands
{
    public sealed class DeleteCityCommandHandlerTests
    {
        private readonly Mock<ICityCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICityQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ICityValidationLookup> _mockCityValidationLookup = new(MockBehavior.Strict);

        private DeleteCityCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockCityValidationLookup.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = CityBuilders.ValidDeleteCommand();
            var entity = CityBuilders.ValidEntity();
            var deleteMappedEntity = new Cities { Id = 0, IsDeleted = IsDelete.Deleted };
            var dto = CityBuilders.ValidDto();

            _mockCityValidationLookup
                .Setup(r => r.IsCityUsedAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<Cities>(command))
                .Returns(deleteMappedEntity);

            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(command.Id))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<Cities>()))
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<CityDto>(deleteMappedEntity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_CityUsedInFixedAsset_ThrowsValidationException()
        {
            var command = CityBuilders.ValidDeleteCommand();

            _mockCityValidationLookup
                .Setup(r => r.IsCityUsedAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*in use in FixedAsset*");
        }

        [Fact]
        public async Task Handle_CityNotFound_ThrowsValidationException()
        {
            var command = CityBuilders.ValidDeleteCommand(id: 999);

            _mockCityValidationLookup
                .Setup(r => r.IsCityUsedAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Cities?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*does not exist*");
        }

        [Fact]
        public async Task Handle_CityHasDependencies_ThrowsValidationException()
        {
            var command = CityBuilders.ValidDeleteCommand();
            var entity = CityBuilders.ValidEntity();

            _mockCityValidationLookup
                .Setup(r => r.IsCityUsedAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<Cities>(command))
                .Returns(new Cities());

            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(command.Id))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*in use by other records*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = CityBuilders.ValidDeleteCommand();
            var entity = CityBuilders.ValidEntity();
            var deleteMappedEntity = new Cities { Id = 0, IsDeleted = IsDelete.Deleted };
            var dto = CityBuilders.ValidDto();

            _mockCityValidationLookup
                .Setup(r => r.IsCityUsedAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<Cities>(command))
                .Returns(deleteMappedEntity);

            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(command.Id))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<Cities>()))
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<CityDto>(deleteMappedEntity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.Module == "City"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
