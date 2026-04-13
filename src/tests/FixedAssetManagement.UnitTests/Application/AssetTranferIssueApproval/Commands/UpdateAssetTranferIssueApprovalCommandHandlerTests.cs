using AutoMapper;
using Contracts.Interfaces;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Commands.UpdateAssetTranferIssueApproval;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetTransferIssueApproval;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTranferIssueApproval.Commands
{
    public sealed class UpdateAssetTranferIssueApprovalCommandHandlerTests
    {
        private readonly Mock<IAssetTransferIssueApprovalCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTimeZoneService = new(MockBehavior.Loose);

        public UpdateAssetTranferIssueApprovalCommandHandlerTests()
        {
            _mockIpService.Setup(i => i.GetUserId()).Returns(1);
            _mockIpService.Setup(i => i.GetUserName()).Returns("test-user");
            _mockIpService.Setup(i => i.GetSystemIPAddress()).Returns("127.0.0.1");
            _mockTimeZoneService.Setup(t => t.GetSystemTimeZone()).Returns("UTC");
            _mockTimeZoneService.Setup(t => t.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);
        }

        private UpdateAssetTranferIssueApprovalCommandHandler CreateSut() =>
            new(
                _mockCommandRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object,
                _mockIpService.Object,
                _mockTimeZoneService.Object);

        private static UpdateAssetTranferIssueApprovalCommand ValidCommand() =>
            new UpdateAssetTranferIssueApprovalCommand(new List<int> { 1, 2, 3 }, "Approved");

        private void SetupHappyPath(int bulkRows = 3)
        {
            _mockCommandRepo
                .Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<AssetTransferIssueHdr>
                {
                    new AssetTransferIssueHdr { Id = 1 },
                    new AssetTransferIssueHdr { Id = 2 },
                    new AssetTransferIssueHdr { Id = 3 }
                });

            _mockCommandRepo
                .Setup(r => r.ExecuteBulkUpdateAsync(
                    It.IsAny<List<int>>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(bulkRows);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdatedRows()
        {
            SetupHappyPath(3);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.Should().Be(3);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsBulkUpdateOnce()
        {
            SetupHappyPath(3);
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.ExecuteBulkUpdateAsync(
                    It.IsAny<List<int>>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(3);
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NoTransfersFound_ThrowsValidationException()
        {
            _mockCommandRepo
                .Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<AssetTransferIssueHdr>());

            var sut = CreateSut();
            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(ValidCommand(), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_BulkUpdateReturnsZero_ThrowsException()
        {
            SetupHappyPath(0);

            var sut = CreateSut();
            await Assert.ThrowsAsync<Exception>(() =>
                sut.Handle(ValidCommand(), CancellationToken.None));
        }
    }
}
