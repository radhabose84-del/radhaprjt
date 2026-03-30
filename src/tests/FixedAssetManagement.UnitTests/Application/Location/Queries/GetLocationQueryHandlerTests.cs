using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using FAM.Application.Common.Interfaces.ILocation;
using FAM.Application.Location.Queries.GetLocations;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.Location.Queries
{
    public sealed class GetLocationQueryHandlerTests
    {
        private readonly Mock<ILocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetLocationHandlerQuery CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockDeptLookup.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtos = new List<LocationDto> { LocationBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllLocationListAsync(1, 15, null))
                .ReturnsAsync((dtos, 1));

            _mockDeptLookup
                .Setup(d => d.GetByIdsAsync(It.IsAny<int[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.DepartmentLookupDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetLocationQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllLocationListAsync(1, 15, null))
                .ReturnsAsync((new List<LocationDto>(), 0));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetLocationQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
