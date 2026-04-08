using AutoMapper;
using FAM.Application.Common.Interfaces.IDepreciationDetail;
using FAM.Application.DepreciationDetail.Queries.GetDepreciationAbstract;
using FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.DepreciationDetail.Queries
{
    public sealed class GetDepreciationAbstractQueryHandlerTests
    {
        private readonly Mock<IDepreciationDetailQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDepreciationAbstractQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static readonly DateTimeOffset StartDate = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset EndDate = new(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);

        [Fact]
        public async Task Handle_ReturnsAbstractList()
        {
            var entities = new List<DepreciationAbstractDto> { new() };
            var dtos = new List<DepreciationAbstractDto> { new() };

            _mockRepo
                .Setup(r => r.GetDepreciationAbstractAsync(1, 1, 1, StartDate, EndDate, 1, 1))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<DepreciationAbstractDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetDepreciationAbstractQuery
                {
                    companyId = 1, unitId = 1, finYearId = 1,
                    startDate = StartDate, endDate = EndDate,
                    depreciationPeriod = 1, depreciationType = 1
                }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetDepreciationAbstractAsync(1, 1, 1, StartDate, EndDate, 1, 1))
                .ReturnsAsync(new List<DepreciationAbstractDto>());
            _mockMapper
                .Setup(m => m.Map<List<DepreciationAbstractDto>>(It.IsAny<object>()))
                .Returns(new List<DepreciationAbstractDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetDepreciationAbstractQuery
                {
                    companyId = 1, unitId = 1, finYearId = 1,
                    startDate = StartDate, endDate = EndDate,
                    depreciationPeriod = 1, depreciationType = 1
                }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
