using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IDepreciationGroup;
using FAM.Application.DepreciationGroup.Commands.CreateDepreciationGroup;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using FAM.Domain.Common;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.DepreciationGroups.Commands
{
    public sealed class CreateDepreciationGroupCommandHandlerTests
    {
        private readonly Mock<IDepreciationGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateDepreciationGroupCommandHandler CreateSut() =>
            new(_mockMapper.Object, _mockCommandRepo.Object, _mockMediator.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = DepreciationGroupBuilders.ValidEntity(newId);
            var dto = DepreciationGroupBuilders.ValidDto(newId);

            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.CheckForDuplicatesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((FAM.Domain.Entities.DepreciationGroups)null!);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.DepreciationGroups>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.DepreciationGroups>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<DepreciationGroupDTO>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath(1);
            var command = DepreciationGroupBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath(1);
            var command = DepreciationGroupBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.DepreciationGroups>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_ChecksForDuplicateCode()
        {
            SetupHappyPath(1);
            var command = DepreciationGroupBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.ExistsByCodeAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_ChecksForDuplicateCombination()
        {
            SetupHappyPath(1);
            var command = DepreciationGroupBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CheckForDuplicatesAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = DepreciationGroupBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateCode_ThrowsEntityAlreadyExistsException()
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var command = DepreciationGroupBuilders.ValidCreateCommand();
            var sut = CreateSut();

            await Assert.ThrowsAsync<EntityAlreadyExistsException>(() =>
                sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DuplicateCombination_ThrowsEntityAlreadyExistsException()
        {
            var existingEntity = DepreciationGroupBuilders.ValidEntity();
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _mockCommandRepo
                .Setup(r => r.CheckForDuplicatesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(existingEntity);

            var command = DepreciationGroupBuilders.ValidCreateCommand();
            var sut = CreateSut();

            await Assert.ThrowsAsync<EntityAlreadyExistsException>(() =>
                sut.Handle(command, CancellationToken.None));
        }
    }
}
