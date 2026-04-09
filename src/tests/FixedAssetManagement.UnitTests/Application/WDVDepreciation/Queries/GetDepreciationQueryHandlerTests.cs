using AutoMapper;
using FAM.Application.Common.Interfaces.IWdvDepreciation;
using FAM.Application.WDVDepreciation.Queries.CalculateDepreciation;
using FAM.Application.WDVDepreciation.Queries.GetDepreciation;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.WDVDepreciation.Queries
{
    public sealed class GetDepreciationQueryHandlerTests
    {
        private readonly Mock<IWdvDepreciationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDepreciationQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithData_ReturnsList()
        {
            _mockQueryRepo
                .Setup(r => r.ExistDataAsync(1))
                .ReturnsAsync(true);

            var wdvList = new List<CalculationDepreciationDto>
            {
                new CalculationDepreciationDto { FinYearId = 1, GroupName = "Group A" }
            };

            _mockQueryRepo
                .Setup(r => r.GetWDVDepreciationAsync(1))
                .ReturnsAsync(wdvList);

            _mockMapper
                .Setup(m => m.Map<List<CalculationDepreciationDto>>(It.IsAny<object>()))
                .Returns(wdvList);

            var result = await CreateSut().Handle(
                new GetDepreciationQuery { FinYearId = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NoDataExists_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.ExistDataAsync(1))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(new GetDepreciationQuery { FinYearId = 1 }, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.ExistDataAsync(1)).ReturnsAsync(true);

            _mockQueryRepo
                .Setup(r => r.GetWDVDepreciationAsync(1))
                .ReturnsAsync(new List<CalculationDepreciationDto>());

            _mockMapper
                .Setup(m => m.Map<List<CalculationDepreciationDto>>(It.IsAny<object>()))
                .Returns(new List<CalculationDepreciationDto>());

            await CreateSut().Handle(
                new GetDepreciationQuery { FinYearId = 1 },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
