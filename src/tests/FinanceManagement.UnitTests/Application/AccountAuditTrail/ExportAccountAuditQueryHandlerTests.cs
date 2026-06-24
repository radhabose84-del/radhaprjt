using Contracts.Interfaces;
using FinanceManagement.Application.AccountAuditTrail.Dto;
using FinanceManagement.Application.AccountAuditTrail.Queries.ExportAccountAudit;
using FinanceManagement.Application.Common.Interfaces.IAccountAuditTrail;

namespace FinanceManagement.UnitTests.Application.AccountAuditTrail
{
    public sealed class ExportAccountAuditQueryHandlerTests
    {
        private readonly Mock<IAccountAuditTrailQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private ExportAccountAuditQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockIp.Object, _mockMediator.Object);

        private static List<AccountAuditTrailDto> SampleRows() => new()
        {
            new() { Id = 1, CompanyId = 1, EntityName = "GlAccountMaster", EntityId = 42, Action = "Update",
                    PropertyName = "Description", OldValue = "A", NewValue = "B",
                    CreatedBy = 372, CreatedByRole = "Finance Controller",
                    CreatedDate = new DateTimeOffset(2026, 6, 18, 9, 0, 0, TimeSpan.FromHours(5.5)) },
            new() { Id = 2, CompanyId = 1, EntityName = "GlAccountMaster", EntityId = 42, Action = "Update",
                    PropertyName = "AccountName", OldValue = "X", NewValue = "Y",
                    CreatedBy = 372, CreatedByRole = "Finance Controller",
                    CreatedDate = new DateTimeOffset(2026, 6, 19, 9, 0, 0, TimeSpan.FromHours(5.5)) },
        };

        private ExportAccountAuditQuery Query() =>
            new(new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero), "GlAccountMaster");

        private void SetupRepo(List<AccountAuditTrailDto> rows)
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockRepo.Setup(r => r.ExportAsync(1, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(),
                "GlAccountMaster", It.IsAny<CancellationToken>())).ReturnsAsync(rows);
        }

        [Fact]
        public async Task Handle_RecordCount_EqualsRowCount()
        {
            SetupRepo(SampleRows());
            var dto = await CreateSut().Handle(Query(), CancellationToken.None);

            dto.RecordCount.Should().Be(2);
            dto.Rows.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_Checksum_IsNonEmpty()
        {
            SetupRepo(SampleRows());
            var dto = await CreateSut().Handle(Query(), CancellationToken.None);

            dto.Checksum.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Handle_SameRows_ProduceSameChecksum_Deterministic()
        {
            SetupRepo(SampleRows());
            var first = await CreateSut().Handle(Query(), CancellationToken.None);

            // Fresh SUT + fresh equivalent rows → identical checksum.
            var repo2 = new Mock<IAccountAuditTrailQueryRepository>(MockBehavior.Strict);
            var ip2 = new Mock<IIPAddressService>(MockBehavior.Loose);
            ip2.Setup(s => s.GetCompanyId()).Returns(1);
            repo2.Setup(r => r.ExportAsync(1, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(),
                "GlAccountMaster", It.IsAny<CancellationToken>())).ReturnsAsync(SampleRows());
            var second = await new ExportAccountAuditQueryHandler(repo2.Object, ip2.Object, _mockMediator.Object)
                .Handle(Query(), CancellationToken.None);

            second.Checksum.Should().Be(first.Checksum);
        }

        [Fact]
        public async Task Handle_AlteredRow_ChangesChecksum_TamperEvident()
        {
            SetupRepo(SampleRows());
            var original = await CreateSut().Handle(Query(), CancellationToken.None);

            var tampered = SampleRows();
            tampered[0].NewValue = "TAMPERED";
            var repo2 = new Mock<IAccountAuditTrailQueryRepository>(MockBehavior.Strict);
            var ip2 = new Mock<IIPAddressService>(MockBehavior.Loose);
            ip2.Setup(s => s.GetCompanyId()).Returns(1);
            repo2.Setup(r => r.ExportAsync(1, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(),
                "GlAccountMaster", It.IsAny<CancellationToken>())).ReturnsAsync(tampered);
            var changed = await new ExportAccountAuditQueryHandler(repo2.Object, ip2.Object, _mockMediator.Object)
                .Handle(Query(), CancellationToken.None);

            changed.Checksum.Should().NotBe(original.Checksum);
        }

        [Fact]
        public async Task Handle_EmptyRange_ReturnsZeroCount_AndChecksum()
        {
            SetupRepo(new List<AccountAuditTrailDto>());
            var dto = await CreateSut().Handle(Query(), CancellationToken.None);

            dto.RecordCount.Should().Be(0);
            dto.Rows.Should().BeEmpty();
            dto.Checksum.Should().NotBeNullOrWhiteSpace();   // hash of empty content is still defined
        }
    }
}
