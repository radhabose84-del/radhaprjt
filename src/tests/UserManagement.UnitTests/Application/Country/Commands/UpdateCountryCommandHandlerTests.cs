using AutoMapper;
using MediatR;
using UserManagement.Application.Country.Commands.UpdateCountry;
using UserManagement.Application.Country.Queries.GetCountries;
using UserManagement.Application.Common.Interfaces.ICountry;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using FluentValidation;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Application.Country.Commands
{
    public sealed class UpdateCountryCommandHandlerTests
    {
        private readonly Mock<ICountryCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICountryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private UpdateCountryCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCountryDto()
        {
            var command = CountryBuilders.ValidUpdateCommand();
            var existingEntity = CountryBuilders.ValidEntity();
            var updatedEntity = CountryBuilders.ValidEntity();
            updatedEntity.CountryName = "Updated India";
            var dto = CountryBuilders.ValidDto(countryName: "Updated India");

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(existingEntity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(existingEntity.Id, It.IsAny<Countries>()))
                .ReturnsAsync(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(existingEntity.Id))
                .ReturnsAsync(updatedEntity);

            _mockMapper
                .Setup(m => m.Map<CountryDto>(updatedEntity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.CountryName.Should().Be("Updated India");
        }

        [Fact]
        public async Task Handle_CountryNotFound_ThrowsKeyNotFoundException()
        {
            var command = CountryBuilders.ValidUpdateCommand(id: 999);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Countries?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_DeletedCountry_ThrowsKeyNotFoundException()
        {
            var command = CountryBuilders.ValidUpdateCommand(id: 1);
            var deletedEntity = CountryBuilders.ValidEntity();
            deletedEntity.IsDeleted = IsDelete.Deleted;

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(deletedEntity);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*not found or is deleted*");
        }

        [Fact]
        public async Task Handle_InactivateLinkedCountry_ThrowsValidationException()
        {
            var command = CountryBuilders.ValidUpdateCommand(isActive: 0);
            var existingEntity = CountryBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(existingEntity);

            _mockQueryRepo
                .Setup(r => r.IsLinkedWithStatesAsync(command.Id))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*linked*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = CountryBuilders.ValidUpdateCommand();
            var existingEntity = CountryBuilders.ValidEntity();
            var updatedEntity = CountryBuilders.ValidEntity();
            updatedEntity.CountryName = "Updated India";
            var dto = CountryBuilders.ValidDto(countryName: "Updated India");

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(existingEntity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(existingEntity.Id, It.IsAny<Countries>()))
                .ReturnsAsync(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(existingEntity.Id))
                .ReturnsAsync(updatedEntity);

            _mockMapper
                .Setup(m => m.Map<CountryDto>(updatedEntity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.Module == "Country"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
