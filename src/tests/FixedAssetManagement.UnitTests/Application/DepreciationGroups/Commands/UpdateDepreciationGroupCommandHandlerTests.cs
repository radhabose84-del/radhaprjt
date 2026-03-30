using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IDepreciationGroup;
using FAM.Application.DepreciationGroup.Commands.UpdateDepreciationGroup;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using FAM.Domain.Common;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.DepreciationGroups.Commands
{
    public sealed class UpdateDepreciationGroupCommandHandlerTests
    {
        private readonly Mock<IDepreciationGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDepreciationGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateDepreciationGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockQueryRepo.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var dto = DepreciationGroupBuilders.ValidDto(id);
            var entity = DepreciationGroupBuilders.ValidEntity(id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(dto);

            _mockCommandRepo
                .Setup(r => r.CheckForDuplicatesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((FAM.Domain.Entities.DepreciationGroups)null!);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.DepreciationGroups>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<FAM.Domain.Entities.DepreciationGroups>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath(1);
            var command = DepreciationGroupBuilders.ValidUpdateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1);
            var command = DepreciationGroupBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<FAM.Domain.Entities.DepreciationGroups>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = DepreciationGroupBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsEntityNotFoundException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((DepreciationGroupDTO)null!);

            var command = DepreciationGroupBuilders.ValidUpdateCommand();
            var sut = CreateSut();

            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DuplicateCombination_ThrowsEntityAlreadyExistsException()
        {
            var dto = DepreciationGroupBuilders.ValidDto();
            var existingEntity = DepreciationGroupBuilders.ValidEntity(99);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(dto);

            _mockCommandRepo
                .Setup(r => r.CheckForDuplicatesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(existingEntity);

            var command = DepreciationGroupBuilders.ValidUpdateCommand();
            var sut = CreateSut();

            await Assert.ThrowsAsync<EntityAlreadyExistsException>(() =>
                sut.Handle(command, CancellationToken.None));
        }
    }
}
