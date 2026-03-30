using AutoMapper;
using FAM.Application.AssetMaster.AssetInsurance.Commands.UpdateAssetInsurance;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetInsurance;
using FAM.Domain.Events;
using FluentValidation;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetInsurance.Commands
{
    public sealed class UpdateAssetInsuranceCommandHandlerTests
    {
        private readonly Mock<IAssetInsuranceCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetInsuranceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateAssetInsuranceCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = AssetInsuranceBuilders.ValidEntity(id);

            _mockQueryRepo
                .Setup(r => r.GetByAssetIdAsync(id))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetMaster.AssetInsurance>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.AssetMaster.AssetInsurance>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath(1);
            var command = AssetInsuranceBuilders.ValidUpdateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1);
            var command = AssetInsuranceBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.AssetMaster.AssetInsurance>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = AssetInsuranceBuilders.ValidUpdateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByAssetIdAsync(It.IsAny<int>()))
                .ReturnsAsync((FAM.Domain.Entities.AssetMaster.AssetInsurance?)null);

            var command = AssetInsuranceBuilders.ValidUpdateCommand();
            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ThrowsException()
        {
            var entity = AssetInsuranceBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByAssetIdAsync(It.IsAny<int>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetMaster.AssetInsurance>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<FAM.Domain.Entities.AssetMaster.AssetInsurance>()))
                .ReturnsAsync(false);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = AssetInsuranceBuilders.ValidUpdateCommand();
            var sut = CreateSut();

            await Assert.ThrowsAsync<Exception>(() =>
                sut.Handle(command, CancellationToken.None));
        }
    }
}
