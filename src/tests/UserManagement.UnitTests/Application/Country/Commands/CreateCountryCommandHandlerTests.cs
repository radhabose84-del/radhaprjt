using AutoMapper;
using MediatR;
using UserManagement.Application.Country.Commands.CreateCountry;
using UserManagement.Application.Country.Queries.GetCountries;
using UserManagement.Application.Common.Interfaces.ICountry;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.Country.Commands
{
    public sealed class CreateCountryCommandHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<ICountryCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private CreateCountryCommandHandler CreateSut() =>
            new(_mockMapper.Object, _mockCommandRepo.Object, _mockMediator.Object);

        private void SetupHappyPath(CreateCountryCommand command, Countries createdEntity, CountryDto dto)
        {
            _mockMapper
                .Setup(m => m.Map<Countries>(command))
                .Returns(createdEntity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<Countries>()))
                .ReturnsAsync(createdEntity);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockMapper
                .Setup(m => m.Map<CountryDto>(createdEntity))
                .Returns(dto);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCountryDto()
        {
            var command = CountryBuilders.ValidCreateCommand();
            var entity = CountryBuilders.ValidEntity();
            var dto = CountryBuilders.ValidDto();
            SetupHappyPath(command, entity, dto);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.CountryCode.Should().Be("IND");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = CountryBuilders.ValidCreateCommand();
            var entity = CountryBuilders.ValidEntity();
            var dto = CountryBuilders.ValidDto();
            SetupHappyPath(command, entity, dto);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<Countries>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = CountryBuilders.ValidCreateCommand();
            var entity = CountryBuilders.ValidEntity();
            var dto = CountryBuilders.ValidDto();
            SetupHappyPath(command, entity, dto);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "Country"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsNull_ThrowsInvalidOperationException()
        {
            var command = CountryBuilders.ValidCreateCommand();

            _mockMapper
                .Setup(m => m.Map<Countries>(command))
                .Returns(new Countries());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<Countries>()))
                .ReturnsAsync((Countries?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Country not created*");
        }

        [Fact]
        public async Task Handle_CreateReturnsIdZero_ThrowsInvalidOperationException()
        {
            var command = CountryBuilders.ValidCreateCommand();
            var entity = new Countries { Id = 0 };

            _mockMapper
                .Setup(m => m.Map<Countries>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<Countries>()))
                .ReturnsAsync(entity);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Country not created*");
        }
    }
}
