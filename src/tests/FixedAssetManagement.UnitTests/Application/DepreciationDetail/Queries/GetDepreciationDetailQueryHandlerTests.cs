using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IDepreciationDetail;
using FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.DepreciationDetail.Queries
{
    public sealed class GetDepreciationDetailQueryHandlerTests
    {
        private readonly Mock<IDepreciationDetailQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDepreciationDetailQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtos = new List<DepreciationDto> { DepreciationDetailBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.CalculateDepreciationAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<string?>(), It.IsAny<int>()))
                .ReturnsAsync((dtos, 1, true, (string?)null));

            _mockMapper
                .Setup(m => m.Map<List<DepreciationDto>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetDepreciationDetailQuery
                {
                    companyId = 1,
                    unitId = 1,
                    finYearId = 1,
                    depreciationType = 1,
                    depreciationPeriod = 1,
                    PageNumber = 1,
                    PageSize = 10
                },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.CalculateDepreciationAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<string?>(), It.IsAny<int>()))
                .ReturnsAsync((new List<DepreciationDto>(), 0, true, (string?)null));

            _mockMapper
                .Setup(m => m.Map<List<DepreciationDto>>(It.IsAny<object>()))
                .Returns(new List<DepreciationDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetDepreciationDetailQuery { PageNumber = 1, PageSize = 10, companyId = 1, unitId = 1, finYearId = 1, depreciationType = 1, depreciationPeriod = 1 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
