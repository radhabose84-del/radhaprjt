using AutoMapper;
using FAM.Application.Common.Interfaces.IManufacture;
using FAM.Application.Manufacture.Commands.DeleteManufacture;
using FAM.Application.Manufacture.Queries.GetManufacture;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.Manufactures.Commands
{
    public sealed class DeleteManufactureCommandHandlerTests
    {
        private readonly Mock<IManufactureCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IManufactureQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteManufactureCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            var dto = ManufacturesBuilders.ValidDto(id);
            var entity = ManufacturesBuilders.ValidEntity(id);
            var resultDto = ManufacturesBuilders.ValidDto(id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(dto);

            _mockQueryRepo
                .Setup(r => r.IsManufactureLinkedAsync(id))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.Manufactures>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(id, It.IsAny<FAM.Domain.Entities.Manufactures>()))
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<ManufactureDTO>(It.IsAny<object>()))
                .Returns(resultDto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath();
            var command = ManufacturesBuilders.ValidDeleteCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsDeleteOnce()
        {
            SetupHappyPath();
            var command = ManufacturesBuilders.ValidDeleteCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(1, It.IsAny<FAM.Domain.Entities.Manufactures>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var command = ManufacturesBuilders.ValidDeleteCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((ManufactureDTO?)null);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new DeleteManufactureCommand { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_LinkedRecords_ThrowsValidationException()
        {
            var dto = ManufacturesBuilders.ValidDto(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);

            _mockQueryRepo
                .Setup(r => r.IsManufactureLinkedAsync(1))
                .ReturnsAsync(true);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                ManufacturesBuilders.ValidDeleteCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
