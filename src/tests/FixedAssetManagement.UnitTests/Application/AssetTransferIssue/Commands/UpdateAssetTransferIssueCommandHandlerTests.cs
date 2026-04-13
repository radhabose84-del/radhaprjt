using AutoMapper;
using Contracts.Interfaces;
using FAM.Application.AssetMaster.AssetTransferIssue.Command.UpdateAssetTransferIssue;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Domain.Entities.AssetMaster;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTransferIssue.Commands
{
    public sealed class UpdateAssetTransferIssueCommandHandlerTests
    {
        private readonly Mock<IAssetTransferCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetTransferQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTimeZoneService = new(MockBehavior.Loose);
        private readonly Mock<IValidator<UpdateAssetTransferIssueCommand>> _mockValidator = new(MockBehavior.Loose);

        public UpdateAssetTransferIssueCommandHandlerTests()
        {
            _mockIpService.Setup(i => i.GetUserId()).Returns(1);
            _mockIpService.Setup(i => i.GetUserName()).Returns("test-user");
            _mockIpService.Setup(i => i.GetSystemIPAddress()).Returns("127.0.0.1");
            _mockTimeZoneService.Setup(t => t.GetSystemTimeZone()).Returns("UTC");
            _mockTimeZoneService.Setup(t => t.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);
        }

        private UpdateAssetTransferIssueCommandHandler CreateSut() =>
            new(
                _mockCommandRepo.Object,
                _mockQueryRepo.Object,
                _mockMapper.Object,
                _mockMediator.Object,
                _mockIpService.Object,
                _mockTimeZoneService.Object,
                _mockValidator.Object);

        private static UpdateAssetTransferIssueCommand ValidUpdateCommand() =>
            new UpdateAssetTransferIssueCommand
            {
                AssetTransferHdr = new UpdateAssetTransferHdrDto
                {
                    Id = 1,
                    DocDate = DateTimeOffset.UtcNow,
                    TransferType = 1,
                    FromUnitId = 1,
                    ToUnitId = 2,
                    FromDepartmentId = 1,
                    ToDepartmentId = 2,
                    FromCustodianId = 1,
                    ToCustodianId = 2,
                    Status = "Pending"
                }
            };

        private void SetupHappyPath(bool updateResult = true)
        {
            _mockQueryRepo
                .Setup(r => r.GetAssetTransferByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new AssetTransferJsonDto { AssetTransferId = 1 });

            _mockMapper
                .Setup(m => m.Map<AssetTransferIssueHdr>(It.IsAny<object>()))
                .Returns(new AssetTransferIssueHdr { Id = 1 });

            _mockCommandRepo
                .Setup(r => r.UpdateAssetTransferAsync(It.IsAny<AssetTransferIssueHdr>()))
                .ReturnsAsync(updateResult);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath(true);
            var result = await CreateSut().Handle(ValidUpdateCommand(), CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(true);
            await CreateSut().Handle(ValidUpdateCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.UpdateAssetTransferAsync(It.IsAny<AssetTransferIssueHdr>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetAssetTransferByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((AssetTransferJsonDto?)null);

            var sut = CreateSut();
            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(ValidUpdateCommand(), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UpdateFails_ThrowsException()
        {
            SetupHappyPath(false);

            var sut = CreateSut();
            await Assert.ThrowsAsync<Exception>(() =>
                sut.Handle(ValidUpdateCommand(), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsAuditFieldsFromIpAndTimeZoneServices()
        {
            SetupHappyPath(true);
            await CreateSut().Handle(ValidUpdateCommand(), CancellationToken.None);

            _mockIpService.Verify(i => i.GetUserId(), Times.AtLeastOnce);
            _mockIpService.Verify(i => i.GetUserName(), Times.AtLeastOnce);
            _mockIpService.Verify(i => i.GetSystemIPAddress(), Times.AtLeastOnce);
            _mockTimeZoneService.Verify(t => t.GetCurrentTime(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
