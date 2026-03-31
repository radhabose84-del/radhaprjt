using AutoMapper;
using PartyManagement.Application.BankMaster;
using PartyManagement.Application.BankMaster.Queries.GetBankMasterById;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using PartyManagement.UnitTests.TestData;
using Xunit;

namespace PartyManagement.UnitTests.Application.BankMaster.Queries
{
    public sealed class GetBankMasterByIdQueryHandlerTests
    {
        private readonly Mock<IBankMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetBankMasterByIdHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var entity = BankMasterBuilders.ValidEntity(1);
            var dto = BankMasterBuilders.ValidDto(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<BankMasterDto>(entity))
                .Returns(dto);

            var result = await CreateSut().Handle(new GetBankMasterByIdQuery(1), CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartyManagement.Domain.Entities.BankMaster?)null);

            var result = await CreateSut().Handle(new GetBankMasterByIdQuery(99), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_CallsGetByIdOnce()
        {
            var entity = BankMasterBuilders.ValidEntity(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            await CreateSut().Handle(new GetBankMasterByIdQuery(1), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
