using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IDepreciationGroup;
using FAM.Application.DepreciationGroup.Commands.DeleteDepreciationGroup;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using FAM.Domain.Common;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.DepreciationGroups.Commands
{
    public sealed class DeleteDepreciationGroupCommandHandlerTests
    {
        private readonly Mock<IDepreciationGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDepreciationGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private DeleteDepreciationGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            var dto = DepreciationGroupBuilders.ValidDto(id);
            var entity = DepreciationGroupBuilders.ValidEntity(id);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.DepreciationGroups>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.DepreciationGroups>()))
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<DepreciationGroupDTO>(It.IsAny<object>()))
                .Returns(DepreciationGroupBuilders.ValidDto(id));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath(1);
            var command = DepreciationGroupBuilders.ValidDeleteCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsDeleteOnce()
        {
            SetupHappyPath(1);
            var command = DepreciationGroupBuilders.ValidDeleteCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.DepreciationGroups>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = DepreciationGroupBuilders.ValidDeleteCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsEntityNotFoundException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((DepreciationGroupDTO)null!);

            var command = DepreciationGroupBuilders.ValidDeleteCommand();
            var sut = CreateSut();

            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DeleteReturnsZero_ThrowsExceptionRules()
        {
            var dto = DepreciationGroupBuilders.ValidDto();
            var entity = DepreciationGroupBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.DepreciationGroups>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.DepreciationGroups>()))
                .ReturnsAsync(0);

            var command = DepreciationGroupBuilders.ValidDeleteCommand();
            var sut = CreateSut();

            await Assert.ThrowsAsync<ExceptionRules>(() =>
                sut.Handle(command, CancellationToken.None));
        }
    }
}
