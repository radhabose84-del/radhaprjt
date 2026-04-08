using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Application.PriceMaster.Command.CreatePriceMaster;
using PurchaseManagement.Application.PriceMaster.Commands.Create;
using PurchaseManagement.Application.PriceMaster.Dtos;
using PurchaseManagement.Domain.Entities.PriceMaster;
using PurchaseManagement.UnitTests.TestData;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;

namespace PurchaseManagement.UnitTests.Application.PriceMaster.Commands
{
    public sealed class CreatePriceMasterCommandHandlerTests
    {
        private readonly Mock<IPriceMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreatePriceMasterCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<IAppDataMiscMasterLookup> _mockAppDataMisc = new(MockBehavior.Loose);

        private CreatePriceMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockIpService.Object,
                _mockMiscRepo.Object, _mockOutbox.Object, _mockMediator.Object,
                _mockLogger.Object, _mockItemLookup.Object, _mockPartyLookup.Object,
                _mockAppDataMisc.Object);

        private PurchaseManagement.Domain.Entities.MiscMaster BuildMiscMaster(int id = 1) =>
            new PurchaseManagement.Domain.Entities.MiscMaster { Id = id };

        private void SetupHappyPath(int newId = 1)
        {
            var header = PriceMasterBuilders.ValidHeader(newId);

            _mockCommandRepo
                .Setup(r => r.HasOverlappingHeaderAsync(
                    It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateOnly>(), It.IsAny<DateOnly?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<PriceMasterHeader>(It.IsAny<object>()))
                .Returns(header);

            _mockMapper
                .Setup(m => m.Map<PriceMasterDetail>(It.IsAny<object>()))
                .Returns(PriceMasterBuilders.ValidDetail(newId));

            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(BuildMiscMaster(1));

            _mockCommandRepo
                .Setup(r => r.AddAsync(It.IsAny<PriceMasterHeader>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockCommandRepo
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _mockCommandRepo
                .Setup(r => r.LoadAggregateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(header);
        }

        private static CreatePriceMasterCommand BuildCommand() =>
            new CreatePriceMasterCommand
            {
                Data = new PriceMasterCreateDto
                {
                    ItemId = 1,
                    VendorId = 1,
                    ValidFrom = new DateOnly(2025, 1, 1),
                    ValidTo = null,
                    UomId = 1,
                    Details = new List<PriceMasterDetailUpsertDto>
                    {
                        new PriceMasterDetailUpsertDto
                        {
                            ScaleQtyFrom = 1,
                            UnitPrice = 100m,
                            CurrencyId = 1,
                            IsActive = 1
                        }
                    }
                }
            };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(5);
            var result = await CreateSut().Handle(BuildCommand(), CancellationToken.None);
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsAddAsync_Once()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(BuildCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.AddAsync(It.IsAny<PriceMasterHeader>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsSaveChanges_Once()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(BuildCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_OverlappingHeader_ThrowsInvalidOperationException()
        {
            _mockCommandRepo
                .Setup(r => r.HasOverlappingHeaderAsync(
                    It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateOnly>(), It.IsAny<DateOnly?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(BuildCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Handle_OverlappingHeader_DoesNotCallAddAsync()
        {
            _mockCommandRepo
                .Setup(r => r.HasOverlappingHeaderAsync(
                    It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateOnly>(), It.IsAny<DateOnly?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();
            try { await sut.Handle(BuildCommand(), CancellationToken.None); } catch { /* expected */ }

            _mockCommandRepo.Verify(r => r.AddAsync(It.IsAny<PriceMasterHeader>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
