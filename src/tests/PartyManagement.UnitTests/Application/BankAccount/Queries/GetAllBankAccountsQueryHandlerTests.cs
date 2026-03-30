using AutoMapper;
using MediatR;
using PartyManagement.Application.BankAccount;
using PartyManagement.Application.BankAccount.Query.GetAllBankAccounts;
using PartyManagement.Application.BankAccount.Query.GetBankAccountsPaged;
using PartyManagement.Application.Common.Interfaces.IBankAccount;
using Xunit;

namespace PartyManagement.UnitTests.Application.BankAccount.Queries
{
    public sealed class GetAllBankAccountsQueryHandlerTests
    {
        private readonly Mock<IBankAccountQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllBankAccountsQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static BankAccountDto ValidDto(int id = 1) =>
            new BankAccountDto
            {
                Id = id,
                BankId = 1,
                AccountNumber = "1234567890",
                AccountHolderName = "Test Holder",
                IsActive = 1
            };

        [Fact]
        public async Task Handle_ReturnsItems()
        {
            var dtoList = new List<BankAccountDto> { ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<BankAccountDto>)dtoList, 1));

            _mockMapper
                .Setup(m => m.Map<BankAccountDto>(It.IsAny<object>()))
                .Returns(ValidDto());

            var (items, total) = await CreateSut().Handle(
                new GetAllBankAccountsQuery(1, 10),
                CancellationToken.None);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsTotalZero()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<BankAccountDto>)new List<BankAccountDto>(), 0));

            var (items, total) = await CreateSut().Handle(
                new GetAllBankAccountsQuery(1, 10),
                CancellationToken.None);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }
    }
}
