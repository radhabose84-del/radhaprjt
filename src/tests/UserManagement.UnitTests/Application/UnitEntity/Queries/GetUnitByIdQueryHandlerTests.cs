using AutoMapper;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Party;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Units.Queries.GetUnitById;
using UserManagement.Application.Units.Queries.GetUnits;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.UnitEntity.Queries
{
    public sealed class GetUnitByIdQueryHandlerTests
    {
        private readonly Mock<IUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IBankAccountLookup> _mockBankLookup = new(MockBehavior.Strict);
        private readonly Mock<ILogger<GetUnitByIdQueryHandler>> _mockLogger = new(MockBehavior.Loose);

        private GetUnitByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockBankLookup.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var repoResult = UnitEntityBuilders.ValidGetUnitsByIdDto();
            var mappedDto = UnitEntityBuilders.ValidGetUnitsByIdDto();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(repoResult);

            _mockMapper
                .Setup(m => m.Map<GetUnitsByIdDto>(repoResult))
                .Returns(mappedDto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUnitByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.UnitName.Should().Be("Test Unit");
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((GetUnitsByIdDto?)null);

            // Not found is a normal read outcome → returns null (controller responds 200/data:null).
            var result = await CreateSut().Handle(
                new GetUnitByIdQuery { Id = 999 },
                CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var repoResult = UnitEntityBuilders.ValidGetUnitsByIdDto();
            var mappedDto = UnitEntityBuilders.ValidGetUnitsByIdDto();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(repoResult);

            _mockMapper
                .Setup(m => m.Map<GetUnitsByIdDto>(repoResult))
                .Returns(mappedDto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetUnitByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetUnitByIdQuery" &&
                        e.Module == "Unit"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithBankAccountId_PopulatesBankDisplayFields()
        {
            var dto = new GetUnitsByIdDto { Id = 1, UnitName = "Test Unit", BankAccountId = 5 };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<GetUnitsByIdDto>(dto)).Returns(dto);
            _mockBankLookup
                .Setup(l => l.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BankAccountLookupDto { Id = 5, AccountNumber = "9999", BankName = "HDFC" });
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetUnitByIdQuery { Id = 1 }, CancellationToken.None);

            result.BankAccountId.Should().Be(5);
            result.BankAccountNumber.Should().Be("9999");
            result.BankName.Should().Be("HDFC");
            result.BankAccountDetails.Should().NotBeNull();
            result.BankAccountDetails!.Id.Should().Be(5);
            result.BankAccountDetails.AccountNumber.Should().Be("9999");
        }

        [Fact]
        public async Task Handle_WithoutBankAccountId_DoesNotCallLookup()
        {
            var dto = new GetUnitsByIdDto { Id = 2, UnitName = "No Bank Unit", BankAccountId = null };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<GetUnitsByIdDto>(dto)).Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetUnitByIdQuery { Id = 2 }, CancellationToken.None);

            _mockBankLookup.Verify(
                l => l.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
