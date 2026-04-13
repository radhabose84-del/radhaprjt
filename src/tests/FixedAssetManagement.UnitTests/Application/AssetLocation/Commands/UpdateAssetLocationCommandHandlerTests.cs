using AutoMapper;
using FAM.Application.AssetMaster.AssetLocation.Commands.UpdateAssetLocation;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetLocation;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetLocation.Commands
{
    public sealed class UpdateAssetLocationCommandHandlerTests
    {
        private readonly Mock<IAssetLocationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetLocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateAssetLocationCommandHandler CreateSut() =>
            new(
                _mockCommandRepo.Object,
                _mockMapper.Object,
                _mockQueryRepo.Object,
                _mockMediator.Object);

        private static UpdateAssetLocationCommand BuildCommand(int assetId = 1) =>
            new UpdateAssetLocationCommand
            {
                AssetId = assetId,
                UnitId = 1,
                DepartmentId = 2,
                LocationId = 3,
                SubLocationId = 4,
                CustodianId = 5,
                UserID = 10
            };

        private static FAM.Domain.Entities.AssetMaster.AssetLocation BuildEntity(int id = 1) =>
            new FAM.Domain.Entities.AssetMaster.AssetLocation
            {
                Id = id,
                AssetId = id,
                UnitId = 1,
                DepartmentId = 2,
                LocationId = 3,
                SubLocationId = 4,
                CustodianId = 5,
                UserID = 10
            };

        private void SetupHappyPath(int assetId = 1, int updateResult = 1)
        {
            var entity = BuildEntity(assetId);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(assetId))
                .ReturnsAsync(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(assetId, It.IsAny<FAM.Domain.Entities.AssetMaster.AssetLocation>()))
                .ReturnsAsync(updateResult);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsAssetId()
        {
            SetupHappyPath(1, 1);
            var command = BuildCommand(1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1, 1);
            var command = BuildCommand(1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(1, It.IsAny<FAM.Domain.Entities.AssetMaster.AssetLocation>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1, 1);
            var command = BuildCommand(1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_AssetLocationNotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((FAM.Domain.Entities.AssetMaster.AssetLocation)null!);

            var command = BuildCommand(99);
            var sut = CreateSut();

            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));

            ex.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task Handle_UpdateReturnsZero_ThrowsException()
        {
            SetupHappyPath(1, 0);
            var command = BuildCommand(1);

            var sut = CreateSut();

            await Assert.ThrowsAsync<Exception>(() =>
                sut.Handle(command, CancellationToken.None));
        }
    }
}
