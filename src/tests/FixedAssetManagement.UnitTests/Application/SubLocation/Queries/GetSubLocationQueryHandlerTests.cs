using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Application.SubLocation.Queries.GetSubLocations;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.SubLocation.Queries
{
    public sealed class GetSubLocationQueryHandlerTests
    {
        private readonly Mock<ISubLocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetSubLocationHandlerQuery CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<FAM.Domain.Entities.SubLocation> { SubLocationBuilders.ValidEntity() };
            var dtos = new List<SubLocationDto> { SubLocationBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllSubLocationAsync(1, 15, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<SubLocationDto>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockDeptLookup
                .Setup(d => d.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DepartmentLookupDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetSubLocationQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllSubLocationAsync(1, 15, null))
                .ReturnsAsync((new List<FAM.Domain.Entities.SubLocation>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<SubLocationDto>>(It.IsAny<object>()))
                .Returns(new List<SubLocationDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetSubLocationQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
