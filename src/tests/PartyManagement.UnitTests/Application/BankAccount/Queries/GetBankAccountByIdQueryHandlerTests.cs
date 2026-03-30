using AutoMapper;
using MediatR;
using PartyManagement.Application.BankAccount;
using PartyManagement.Application.BankAccount.Query.GetBankAccountById;
using PartyManagement.Application.Common.Interfaces.IBankAccount;
using Xunit;

namespace PartyManagement.UnitTests.Application.BankAccount.Queries
{
    public sealed class GetBankAccountByIdQueryHandlerTests
    {
        private readonly Mock<IBankAccountQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetBankAccountByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static BankAccountDto ValidDto(int id = 1) =>
            new BankAccountDto { Id = id, BankId = 1, AccountNumber = "1234567890", AccountHolderName = "Holder" };

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var entity = new PartyManagement.Domain.Entities.BankAccount { Id = 1, BankId = 1 };
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidDto(1));

            _mockMapper
                .Setup(m => m.Map<BankAccountDto>(It.IsAny<object>()))
                .Returns(ValidDto(1));

            var result = await CreateSut().Handle(new GetBankAccountByIdQuery(1), CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BankAccountDto?)null);

            var result = await CreateSut().Handle(new GetBankAccountByIdQuery(99), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidDto(1));

            _mockMapper
                .Setup(m => m.Map<BankAccountDto>(It.IsAny<object>()))
                .Returns(ValidDto(1));

            await CreateSut().Handle(new GetBankAccountByIdQuery(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<PartyManagement.Domain.Events.AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
