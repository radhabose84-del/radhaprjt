using AutoMapper;
using Contracts.Interfaces.Lookups.FixedAssetManagement;
using MediatR;
using UserManagement.Application.Country.Commands.DeleteCountry;
using UserManagement.Application.Country.Queries.GetCountries;
using UserManagement.Application.Common.Interfaces.ICountry;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using FluentValidation;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Application.Country.Commands
{
    public sealed class DeleteCountryCommandHandlerTests
    {
        private readonly Mock<ICountryCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICountryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ICountryValidationLookup> _mockCountryValidationLookup = new(MockBehavior.Strict);

        private DeleteCountryCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockCountryValidationLookup.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCountryDto()
        {
            var command = CountryBuilders.ValidDeleteCommand();
            var entity = CountryBuilders.ValidEntity();
            var deleteMappedEntity = new Countries { Id = 0, IsDeleted = IsDelete.Deleted };
            var dto = CountryBuilders.ValidDto();

            _mockCountryValidationLookup
                .Setup(r => r.IsCountryUsedAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetStateByCountryIdAsync(command.Id))
                .ReturnsAsync(new List<Countries>());

            _mockMapper
                .Setup(m => m.Map<Countries>(command))
                .Returns(deleteMappedEntity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<Countries>()))
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<CountryDto>(deleteMappedEntity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_CountryUsedInFixedAsset_ThrowsValidationException()
        {
            var command = CountryBuilders.ValidDeleteCommand();

            _mockCountryValidationLookup
                .Setup(r => r.IsCountryUsedAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*in use in FixedAsset*");
        }

        [Fact]
        public async Task Handle_CountryNotFound_ThrowsValidationException()
        {
            var command = CountryBuilders.ValidDeleteCommand(id: 999);

            _mockCountryValidationLookup
                .Setup(r => r.IsCountryUsedAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Countries?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*does not exist*");
        }

        [Fact]
        public async Task Handle_CountryHasStates_ThrowsValidationException()
        {
            var command = CountryBuilders.ValidDeleteCommand();
            var entity = CountryBuilders.ValidEntity();

            _mockCountryValidationLookup
                .Setup(r => r.IsCountryUsedAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetStateByCountryIdAsync(command.Id))
                .ReturnsAsync(new List<Countries> { new Countries { Id = 1 } });

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*State already exists*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = CountryBuilders.ValidDeleteCommand();
            var entity = CountryBuilders.ValidEntity();
            var deleteMappedEntity = new Countries { Id = 0, IsDeleted = IsDelete.Deleted };
            var dto = CountryBuilders.ValidDto();

            _mockCountryValidationLookup
                .Setup(r => r.IsCountryUsedAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetStateByCountryIdAsync(command.Id))
                .ReturnsAsync(new List<Countries>());

            _mockMapper
                .Setup(m => m.Map<Countries>(command))
                .Returns(deleteMappedEntity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(command.Id, It.IsAny<Countries>()))
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<CountryDto>(deleteMappedEntity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.Module == "Country"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
