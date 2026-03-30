using AutoMapper;
using PartyManagement.Application.BankMaster;
using PartyManagement.Application.BankMaster.Queries.GetBankMastersPaged;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using PartyManagement.UnitTests.TestData;
using Xunit;

namespace PartyManagement.UnitTests.Application.BankMaster.Queries
{
    public sealed class GetBankMastersPagedQueryHandlerTests
    {
        private readonly Mock<IBankMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetBankMastersPagedQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsItems()
        {
            var entities = new List<PartyManagement.Domain.Entities.BankMaster> { BankMasterBuilders.ValidEntity() };
            var dto = BankMasterBuilders.ValidDto();

            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<PartyManagement.Domain.Entities.BankMaster>)entities, 1));

            _mockMapper
                .Setup(m => m.Map<BankMasterDto>(It.IsAny<PartyManagement.Domain.Entities.BankMaster>()))
                .Returns(dto);

            var (items, total) = await CreateSut().Handle(
                new GetBankMastersPagedQuery(1, 10, null), CancellationToken.None);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<PartyManagement.Domain.Entities.BankMaster>)new List<PartyManagement.Domain.Entities.BankMaster>(), 0));

            var (items, total) = await CreateSut().Handle(
                new GetBankMastersPagedQuery(1, 10, null), CancellationToken.None);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetAllAsyncOnce()
        {
            var entities = new List<PartyManagement.Domain.Entities.BankMaster> { BankMasterBuilders.ValidEntity() };

            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<PartyManagement.Domain.Entities.BankMaster>)entities, 1));

            _mockMapper
                .Setup(m => m.Map<BankMasterDto>(It.IsAny<PartyManagement.Domain.Entities.BankMaster>()))
                .Returns(BankMasterBuilders.ValidDto());

            await CreateSut().Handle(new GetBankMastersPagedQuery(1, 10, null), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
