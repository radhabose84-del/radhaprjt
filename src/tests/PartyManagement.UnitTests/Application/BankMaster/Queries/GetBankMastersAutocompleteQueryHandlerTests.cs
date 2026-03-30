using PartyManagement.Application.BankMaster;
using PartyManagement.Application.BankMaster.Queries.GetBankMastersAutocomplete;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using PartyManagement.UnitTests.TestData;
using Xunit;

namespace PartyManagement.UnitTests.Application.BankMaster.Queries
{
    public sealed class GetBankMastersAutocompleteQueryHandlerTests
    {
        private readonly Mock<IBankMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetBankMastersAutocompleteHandler CreateSut() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ReturnsAutocompleteList()
        {
            var entities = (IReadOnlyList<PartyManagement.Domain.Entities.BankMaster>)new List<PartyManagement.Domain.Entities.BankMaster>
            {
                BankMasterBuilders.ValidEntity(1)
            };

            _mockQueryRepo
                .Setup(r => r.GetAutocompleteAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entities);

            var result = await CreateSut().Handle(new GetBankMastersAutocompleteQuery("ICICI"), CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyList_ReturnsEmpty()
        {
            var entities = (IReadOnlyList<PartyManagement.Domain.Entities.BankMaster>)new List<PartyManagement.Domain.Entities.BankMaster>();

            _mockQueryRepo
                .Setup(r => r.GetAutocompleteAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entities);

            var result = await CreateSut().Handle(new GetBankMastersAutocompleteQuery("XYZ"), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_FormatsBankLabel()
        {
            var entity = BankMasterBuilders.ValidEntity(1);
            entity.BankName = "ICICI Bank";
            entity.BankCode = "BNK001";

            var entities = (IReadOnlyList<PartyManagement.Domain.Entities.BankMaster>)new List<PartyManagement.Domain.Entities.BankMaster> { entity };

            _mockQueryRepo
                .Setup(r => r.GetAutocompleteAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entities);

            var result = await CreateSut().Handle(new GetBankMastersAutocompleteQuery(null), CancellationToken.None);

            result[0].Label.Should().Be("ICICI Bank (BNK001)");
            result[0].Code.Should().Be("BNK001");
        }
    }
}
