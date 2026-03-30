using AutoMapper;
using Contracts.Common;
using FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsurance;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetInsurance;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetInsurance.Queries
{
    public sealed class GetAssetInsuranceQueryHandlerTests
    {
        private readonly Mock<IAssetInsuranceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetInsuranceQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<FAM.Domain.Entities.AssetMaster.AssetInsurance>
            {
                AssetInsuranceBuilders.ValidEntity()
            };
            var dtoList = new List<GetAssetInsuranceDto> { AssetInsuranceBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllAssetInsuranceAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<GetAssetInsuranceDto>>(It.IsAny<object>()))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetInsuranceQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<FAM.Domain.Entities.AssetMaster.AssetInsurance>();
            var dtoList = new List<GetAssetInsuranceDto>();

            _mockQueryRepo
                .Setup(r => r.GetAllAssetInsuranceAsync(2, 5, "test"))
                .ReturnsAsync((entities, 20));

            _mockMapper
                .Setup(m => m.Map<List<GetAssetInsuranceDto>>(It.IsAny<object>()))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetInsuranceQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(20);
        }
    }
}
