using AutoMapper;
using FAM.Application.Common.Interfaces.ISpecificationMaster;
using FAM.Application.SpecificationMaster.Commands.CreateSpecificationMaster;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using FAM.Domain.Events;
using FluentValidation;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.SpecificationMasters.Commands
{
    public sealed class CreateSpecificationMasterCommandHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ISpecificationMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateSpecificationMasterCommandHandler CreateSut() =>
            new(_mockMapper.Object, _mockCommandRepo.Object, _mockMediator.Object);

        private void SetupHappyPath(FAM.Domain.Entities.SpecificationMasters entity, SpecificationMasterDTO dto)
        {
            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.SpecificationMasters>(It.IsAny<CreateSpecificationMasterCommand>()))
                .Returns(entity);
            _mockMapper
                .Setup(m => m.Map<SpecificationMasterDTO>(It.IsAny<FAM.Domain.Entities.SpecificationMasters>()))
                .Returns(dto);
            _mockCommandRepo
                .Setup(r => r.ExistsByAssetGroupIdAsync(It.IsAny<int?>(), It.IsAny<string?>()))
                .ReturnsAsync(false);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.SpecificationMasters>()))
                .ReturnsAsync(entity);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            var command = SpecificationMasterBuilders.ValidCreateCommand();
            var entity = SpecificationMasterBuilders.ValidEntity();
            var dto = SpecificationMasterBuilders.ValidDto(1);
            SetupHappyPath(entity, dto);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = SpecificationMasterBuilders.ValidCreateCommand();
            var entity = SpecificationMasterBuilders.ValidEntity();
            var dto = SpecificationMasterBuilders.ValidDto(1);
            SetupHappyPath(entity, dto);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.SpecificationMasters>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = SpecificationMasterBuilders.ValidCreateCommand();
            var entity = SpecificationMasterBuilders.ValidEntity();
            var dto = SpecificationMasterBuilders.ValidDto(1);
            SetupHappyPath(entity, dto);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_AlreadyExists_ThrowsValidationException()
        {
            var command = SpecificationMasterBuilders.ValidCreateCommand();
            _mockCommandRepo
                .Setup(r => r.ExistsByAssetGroupIdAsync(It.IsAny<int?>(), It.IsAny<string?>()))
                .ReturnsAsync(true);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_AlreadyExists_DoesNotCallCreate()
        {
            var command = SpecificationMasterBuilders.ValidCreateCommand();
            _mockCommandRepo
                .Setup(r => r.ExistsByAssetGroupIdAsync(It.IsAny<int?>(), It.IsAny<string?>()))
                .ReturnsAsync(true);

            var sut = CreateSut();
            try { await sut.Handle(command, CancellationToken.None); } catch { /* expected */ }

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.SpecificationMasters>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_DtoIdZero_ThrowsException()
        {
            var command = SpecificationMasterBuilders.ValidCreateCommand();
            var entity = SpecificationMasterBuilders.ValidEntity(0);
            entity.Id = 0;
            var dto = SpecificationMasterBuilders.ValidDto(0);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.SpecificationMasters>(It.IsAny<CreateSpecificationMasterCommand>()))
                .Returns(entity);
            _mockMapper
                .Setup(m => m.Map<SpecificationMasterDTO>(It.IsAny<FAM.Domain.Entities.SpecificationMasters>()))
                .Returns(dto);
            _mockCommandRepo
                .Setup(r => r.ExistsByAssetGroupIdAsync(It.IsAny<int?>(), It.IsAny<string?>()))
                .ReturnsAsync(false);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.SpecificationMasters>()))
                .ReturnsAsync(entity);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*not created*");
        }
    }
}
