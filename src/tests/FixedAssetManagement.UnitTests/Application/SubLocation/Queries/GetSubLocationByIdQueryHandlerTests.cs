using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Application.SubLocation.Queries.GetSubLocationById;
using FAM.Application.SubLocation.Queries.GetSubLocations;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.SubLocation.Queries
{
    public sealed class GetSubLocationByIdQueryHandlerTests
    {
        private readonly Mock<ISubLocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetSubLocationByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var entity = SubLocationBuilders.ValidEntity(1);
            var dto = SubLocationBuilders.ValidDto(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<SubLocationDto>(It.IsAny<object>()))
                .Returns(dto);

            _mockDeptLookup
                .Setup(d => d.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DepartmentLookupDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetSubLocationByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }
    }
}
