using AutoMapper;
using FAM.Application.Common.Interfaces.ISpecificationMaster;
using FAM.Application.SpecificationMaster.Commands.UpdateSpecificationMaster;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using FAM.Domain.Events;
using FluentValidation;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.SpecificationMasters.Commands
{
    public sealed class UpdateSpecificationMasterCommandHandlerTests
    {
        private readonly Mock<ISpecificationMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ISpecificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateSpecificationMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockQueryRepo.Object, _mockMediator.Object);

        private void SetupHappyPath(FAM.Domain.Entities.SpecificationMasters entity, SpecificationMasterDTO dto)
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(entity);
            _mockQueryRepo
                .Setup(r => r.IsSpecificationMasterLinkedAsync(It.IsAny<int>()))
                .ReturnsAsync(false);
            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.SpecificationMasters>(It.IsAny<UpdateSpecificationMasterCommand>()))
                .Returns(entity);
            _mockMapper
                .Setup(m => m.Map<SpecificationMasterDTO>(It.IsAny<FAM.Domain.Entities.SpecificationMasters>()))
                .Returns(dto);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<FAM.Domain.Entities.SpecificationMasters>()))
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            var command = SpecificationMasterBuilders.ValidUpdateCommand();
            var entity = SpecificationMasterBuilders.ValidEntity();
            var dto = SpecificationMasterBuilders.ValidDto();
            SetupHappyPath(entity, dto);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = SpecificationMasterBuilders.ValidUpdateCommand();
            var entity = SpecificationMasterBuilders.ValidEntity();
            var dto = SpecificationMasterBuilders.ValidDto();
            SetupHappyPath(entity, dto);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<FAM.Domain.Entities.SpecificationMasters>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = SpecificationMasterBuilders.ValidUpdateCommand();
            var entity = SpecificationMasterBuilders.ValidEntity();
            var dto = SpecificationMasterBuilders.ValidDto();
            SetupHappyPath(entity, dto);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            var command = SpecificationMasterBuilders.ValidUpdateCommand(id: 99);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((FAM.Domain.Entities.SpecificationMasters)null!);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*does not exist*");
        }

        [Fact]
        public async Task Handle_LinkedRecord_ThrowsValidationException()
        {
            var command = SpecificationMasterBuilders.ValidUpdateCommand();
            var entity = SpecificationMasterBuilders.ValidEntity();
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(entity);
            _mockQueryRepo
                .Setup(r => r.IsSpecificationMasterLinkedAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*linked*");
        }

        [Fact]
        public async Task Handle_UpdateResultZero_ThrowsException()
        {
            var command = SpecificationMasterBuilders.ValidUpdateCommand();
            var entity = SpecificationMasterBuilders.ValidEntity();
            var dto = SpecificationMasterBuilders.ValidDto();
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(entity);
            _mockQueryRepo
                .Setup(r => r.IsSpecificationMasterLinkedAsync(It.IsAny<int>()))
                .ReturnsAsync(false);
            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.SpecificationMasters>(It.IsAny<UpdateSpecificationMasterCommand>()))
                .Returns(entity);
            _mockMapper
                .Setup(m => m.Map<SpecificationMasterDTO>(It.IsAny<FAM.Domain.Entities.SpecificationMasters>()))
                .Returns(dto);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<FAM.Domain.Entities.SpecificationMasters>()))
                .ReturnsAsync(0);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*not updated*");
        }
    }
}
