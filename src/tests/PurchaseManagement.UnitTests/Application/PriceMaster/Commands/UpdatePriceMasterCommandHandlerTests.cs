using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Application.PriceMaster.Commands.Update;
using PurchaseManagement.Application.PriceMaster.Dtos;
using PurchaseManagement.Domain.Entities.PriceMaster;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PriceMaster.Commands
{
    public sealed class UpdatePriceMasterCommandHandlerTests
    {
        private readonly Mock<IPriceMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Strict);

        private UpdatePriceMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMiscRepo.Object);

        private PurchaseManagement.Domain.Entities.MiscMaster BuildMiscMaster(int id) =>
            new PurchaseManagement.Domain.Entities.MiscMaster { Id = id };

        private void SetupHappyPath(int headerId = 1, int pendingId = 10, int approvedId = 11)
        {
            var header = PriceMasterBuilders.ValidHeader(headerId);
            header.StatusId = pendingId; // PENDING status

            _mockCommandRepo
                .Setup(r => r.LoadAggregateAsync(headerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(header);

            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), "Pending"))
                .ReturnsAsync(BuildMiscMaster(pendingId));

            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), "Approved"))
                .ReturnsAsync(BuildMiscMaster(approvedId));

            _mockCommandRepo
                .Setup(r => r.HasOverlappingHeaderExceptAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateOnly>(), It.IsAny<DateOnly?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
        }

        private static UpdatePriceMasterCommand BuildCommand(int id = 1, int isActive = 1) =>
            new UpdatePriceMasterCommand
            {
                Data = new PriceMasterUpdateDto
                {
                    Id = id,
                    ItemId = 1,
                    VendorId = 1,
                    ValidFrom = new DateOnly(2025, 1, 1),
                    ValidTo = null,
                    UomId = 1,
                    IsActive = isActive,
                    Details = new List<PriceMasterDetailUpsertDto>()
                }
            };

        [Fact]
        public async Task Handle_PendingStatus_ReturnsTrue()
        {
            SetupHappyPath(1, pendingId: 10, approvedId: 11);
            var result = await CreateSut().Handle(BuildCommand(1), CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_PendingStatus_CallsSaveChanges()
        {
            SetupHappyPath(1, pendingId: 10, approvedId: 11);
            await CreateSut().Handle(BuildCommand(1), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsKeyNotFoundException()
        {
            _mockCommandRepo
                .Setup(r => r.LoadAggregateAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PriceMasterHeader?)null);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(BuildCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task Handle_ApprovedStatus_ActiveUpdate_ThrowsInvalidOperation()
        {
            var header = PriceMasterBuilders.ValidHeader(1);
            header.StatusId = 11; // approved

            _mockCommandRepo
                .Setup(r => r.LoadAggregateAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(header);

            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), "Pending"))
                .ReturnsAsync(BuildMiscMaster(10));

            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), "Approved"))
                .ReturnsAsync(BuildMiscMaster(11));

            var sut = CreateSut();
            // IsActive = 1 on approved status should throw
            Func<Task> act = async () => await sut.Handle(BuildCommand(1, isActive: 1), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
