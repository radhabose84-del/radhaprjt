using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.AssetMaster.AssetPurchase.Queries;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetSourceAutoComplete;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetPurchase.Queries
{
    public sealed class GetAssetUnitAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Strict);

        private GetAssetUnitAutoCompleteQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockMediator.Object, _mockUnitLookup.Object);

        [Fact]
        public async Task Handle_ValidRequest_ReturnsUnitList()
        {
            var units = new List<UnitLookupDto>
            {
                new() { OldUnitId = "UNIT1", UnitName = "Test Unit" }
            };

            _mockUnitLookup
                .Setup(r => r.GetUserUnitByUserNameAsync("admin"))
                .ReturnsAsync(units);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetUnitAutoCompleteQuery { Username = "admin" }, CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].UnitName.Should().Be("Test Unit");
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockUnitLookup
                .Setup(r => r.GetUserUnitByUserNameAsync(""))
                .ReturnsAsync(new List<UnitLookupDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetUnitAutoCompleteQuery { Username = null }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
