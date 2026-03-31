using AutoMapper;
using Contracts.Interfaces;
using FAM.Application.AssetMaster.AssetTransferIssue.Command.CreateAssetTransferIssue;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTransferIssue.Commands
{
    public sealed class CreateAssetTransferIssueCommandHandlerTests
    {
        private readonly Mock<IAssetTransferCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTimeZoneService = new(MockBehavior.Loose);
        private readonly Mock<IValidator<CreateAssetTransferIssueCommand>> _mockValidator = new(MockBehavior.Loose);

        private CreateAssetTransferIssueCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockIpService.Object, _mockTimeZoneService.Object, _mockValidator.Object);

        private void SetupHappyPath(int returnId = 1)
        {
            _mockIpService.Setup(s => s.GetSystemIPAddress()).Returns("127.0.0.1");
            _mockIpService.Setup(s => s.GetUserId()).Returns(1);
            _mockIpService.Setup(s => s.GetUserName()).Returns("testuser");
            _mockTimeZoneService.Setup(s => s.GetSystemTimeZone()).Returns("UTC");
            _mockTimeZoneService.Setup(s => s.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);

            _mockMapper
                .Setup(m => m.Map<AssetTransferIssueHdr>(It.IsAny<object>()))
                .Returns(new AssetTransferIssueHdr { Id = 1 });

            _mockCommandRepo
                .Setup(r => r.CreateAssetTransferAsync(It.IsAny<AssetTransferIssueHdr>()))
                .ReturnsAsync(returnId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsId()
        {
            SetupHappyPath(3);

            var result = await CreateSut().Handle(AssetTransferIssueBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().Be(3);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(AssetTransferIssueBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAssetTransferAsync(It.IsAny<AssetTransferIssueHdr>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(AssetTransferIssueBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsException()
        {
            SetupHappyPath(0);

            await Assert.ThrowsAsync<Exception>(() =>
                CreateSut().Handle(AssetTransferIssueBuilders.ValidCreateCommand(), CancellationToken.None));
        }
    }
}
