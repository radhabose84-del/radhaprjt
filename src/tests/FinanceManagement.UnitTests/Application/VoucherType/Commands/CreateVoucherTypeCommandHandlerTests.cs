using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Commands.CreateVoucherType;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.VoucherType.Commands
{
    public sealed class CreateVoucherTypeCommandHandlerTests
    {
        private readonly Mock<IVoucherTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFyLookup = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateVoucherTypeCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockIp.Object, _mockFyLookup.Object, _mockMediator.Object, _mockMapper.Object);

        // The finyear lookup returns one current FY (wide range covers today) → handler seeds its id.
        private void SetupCurrentFinancialYear(int id = 3) =>
            _mockFyLookup.Setup(f => f.GetAllFinancialYearAsync()).ReturnsAsync(new List<FinancialYearLookupDto>
            {
                new() { FinancialYearId = id, FinancialYearName = "2026-27", IsActive = true,
                        StartDate = new DateTime(2000, 1, 1), EndDate = new DateTime(2100, 1, 1) }
            });

        private void SetupHappyPath(int newId = 1)
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            SetupCurrentFinancialYear();
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.VoucherTypeMaster>(It.IsAny<CreateVoucherTypeCommand>()))
                .Returns(VoucherTypeBuilders.ValidEntity());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(
                    It.IsAny<FinanceManagement.Domain.Entities.VoucherTypeMaster>(),
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<int?>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(VoucherTypeBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("created successfully");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var result = await CreateSut().Handle(VoucherTypeBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_SeedsCurrentFinancialYearFromLookup()
        {
            IEnumerable<int>? capturedIds = null;
            int? capturedFy = null;
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            SetupCurrentFinancialYear(id: 3);
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.VoucherTypeMaster>(It.IsAny<CreateVoucherTypeCommand>()))
                .Returns(VoucherTypeBuilders.ValidEntity());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(
                    It.IsAny<FinanceManagement.Domain.Entities.VoucherTypeMaster>(),
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<int?>()))
                .Callback<FinanceManagement.Domain.Entities.VoucherTypeMaster, IEnumerable<int>, int?>(
                    (_, ids, fy) => { capturedIds = ids; capturedFy = fy; })
                .ReturnsAsync(1);

            var command = VoucherTypeBuilders.ValidCreateCommand(accountTypeIds: new List<int> { 1, 5, 6 });
            await CreateSut().Handle(command, CancellationToken.None);

            capturedIds.Should().BeEquivalentTo(new[] { 1, 5, 6 });
            capturedFy.Should().Be(3);   // current FY resolved from the finyear lookup
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(VoucherTypeBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "VOUCHER_TYPE_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
