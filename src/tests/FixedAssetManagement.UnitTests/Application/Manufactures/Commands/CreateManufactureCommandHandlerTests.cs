using AutoMapper;
using FAM.Application.Common.Interfaces.IManufacture;
using FAM.Application.Manufacture.Commands.CreateManufacture;
using FAM.Application.Manufacture.Queries.GetManufacture;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.Manufactures.Commands
{
    public sealed class CreateManufactureCommandHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IManufactureCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateManufactureCommandHandler CreateSut() =>
            new(_mockMapper.Object, _mockCommandRepo.Object, _mockMediator.Object);

        private void SetupHappyPath()
        {
            var entity = ManufacturesBuilders.ValidEntity();
            var dto = ManufacturesBuilders.ValidDto();

            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.Manufactures>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.Manufactures>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<ManufactureDTO>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath();
            var command = ManufacturesBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            var command = ManufacturesBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.Manufactures>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var command = ManufacturesBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateCode_ThrowsValidationException()
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(true);

            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(ManufacturesBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_DuplicateName_ThrowsValidationException()
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(true);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(ManufacturesBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
