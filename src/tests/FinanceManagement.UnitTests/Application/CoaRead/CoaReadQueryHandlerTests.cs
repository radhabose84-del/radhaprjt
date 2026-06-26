using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaRead;
using FinanceManagement.Application.CoaRead.Dto;
using FinanceManagement.Application.CoaRead.Queries.GetAccountByCode;
using FinanceManagement.Application.CoaRead.Queries.SearchAccountsForRead;
using FinanceManagement.Application.CoaRead.Queries.ValidateForPosting;

namespace FinanceManagement.UnitTests.Application.CoaRead
{
    public sealed class CoaReadQueryHandlerTests
    {
        private readonly Mock<ICoaReadQueryRepository> _repo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Loose);

        public CoaReadQueryHandlerTests() => _ip.Setup(s => s.GetCompanyId()).Returns(1);

        // ── get-by-code (AC1) ───────────────────────────────────────────────────
        [Fact]
        public async Task GetByCode_Found_ReturnsAccount()
        {
            _repo.Setup(r => r.GetByCodeAsync(1, "1001", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CoaAccountReadDto { AccountCode = "1001", IsActive = true });

            var sut = new GetAccountByCodeQueryHandler(_repo.Object, _ip.Object, _mediator.Object);
            var result = await sut.Handle(new GetAccountByCodeQuery("1001"), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data!.AccountCode.Should().Be("1001");
        }

        [Fact]
        public async Task GetByCode_NotFound_ReturnsMiss()
        {
            _repo.Setup(r => r.GetByCodeAsync(1, "ZZZ", It.IsAny<CancellationToken>())).ReturnsAsync((CoaAccountReadDto?)null);

            var sut = new GetAccountByCodeQueryHandler(_repo.Object, _ip.Object, _mediator.Object);
            var result = await sut.Handle(new GetAccountByCodeQuery("ZZZ"), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeNull();
        }

        // ── search by type/group (AC5) ──────────────────────────────────────────
        [Fact]
        public async Task Search_ReturnsRowsWithStatus()
        {
            _repo.Setup(r => r.SearchByTypeGroupAsync(1, 3, null, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CoaAccountReadDto> { new() { AccountCode = "1001", IsActive = true }, new() { AccountCode = "1002", IsActive = false } });

            var sut = new SearchAccountsForReadQueryHandler(_repo.Object, _ip.Object, _mediator.Object);
            var result = await sut.Handle(new SearchAccountsForReadQuery { AccountTypeId = 3 }, CancellationToken.None);

            result.Data.Should().HaveCount(2);
            result.Data!.Select(a => a.IsActive).Should().Contain(new[] { true, false });
        }

        // ── validate-for-posting (AC2) ──────────────────────────────────────────
        private ValidateForPostingQueryHandler ValidateSut() => new(_repo.Object, _ip.Object, _mediator.Object);

        [Fact]
        public async Task Validate_ActiveCurrencyMatchNoCcNeeded_IsValid()
        {
            _repo.Setup(r => r.GetPostingInfoByCodeAsync(1, "1001", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccountPostingInfo { Id = 9, IsActive = true, CurrencyTypeId = 2, IsCostCentreMandatory = false });

            var r = await ValidateSut().Handle(new ValidateForPostingQuery { AccountCode = "1001", CurrencyId = 2 }, CancellationToken.None);

            r.Data!.IsValid.Should().BeTrue();
            r.Data!.Reasons.Should().BeEmpty();
        }

        [Fact]
        public async Task Validate_Inactive_FailsWithReason()
        {
            _repo.Setup(r => r.GetPostingInfoByCodeAsync(1, "1001", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccountPostingInfo { Id = 9, IsActive = false, CurrencyTypeId = 2 });

            var r = await ValidateSut().Handle(new ValidateForPostingQuery { AccountCode = "1001", CurrencyId = 2 }, CancellationToken.None);

            r.Data!.IsValid.Should().BeFalse();
            r.Data!.Reasons.Should().Contain(x => x.Contains("inactive"));
        }

        [Fact]
        public async Task Validate_CurrencyMismatch_FailsWithReason()
        {
            _repo.Setup(r => r.GetPostingInfoByCodeAsync(1, "1001", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccountPostingInfo { Id = 9, IsActive = true, CurrencyTypeId = 2, CurrencyTypeCode = "INR" });

            var r = await ValidateSut().Handle(new ValidateForPostingQuery { AccountCode = "1001", CurrencyId = 7 }, CancellationToken.None);

            r.Data!.IsValid.Should().BeFalse();
            r.Data!.Reasons.Should().Contain(x => x.Contains("Currency mismatch"));
        }

        [Fact]
        public async Task Validate_CostCentreMandatoryMissing_FailsWithReason()
        {
            _repo.Setup(r => r.GetPostingInfoByCodeAsync(1, "1001", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccountPostingInfo { Id = 9, IsActive = true, CurrencyTypeId = 2, IsCostCentreMandatory = true });

            var r = await ValidateSut().Handle(new ValidateForPostingQuery { AccountCode = "1001", CurrencyId = 2 }, CancellationToken.None);

            r.Data!.IsValid.Should().BeFalse();
            r.Data!.Reasons.Should().Contain(x => x.Contains("Cost centre"));
        }

        [Fact]
        public async Task Validate_NotFound_FailsWithReason()
        {
            _repo.Setup(r => r.GetPostingInfoByCodeAsync(1, "ZZZ", It.IsAny<CancellationToken>())).ReturnsAsync((AccountPostingInfo?)null);

            var r = await ValidateSut().Handle(new ValidateForPostingQuery { AccountCode = "ZZZ" }, CancellationToken.None);

            r.Data!.IsValid.Should().BeFalse();
            r.Data!.Reasons.Should().Contain(x => x.Contains("not found"));
        }
    }
}
