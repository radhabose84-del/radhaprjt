using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;
using PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetVendorEvaluationDashboard;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.VendorEvaluationHeader.Queries
{
    public sealed class GetVendorEvaluationDashboardQueryHandlerTests
    {
        private readonly Mock<IVendorEvaluationDashboardQueryRepository> _mockDashboardRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetVendorEvaluationDashboardQueryHandler CreateSut() =>
            new(_mockDashboardRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static GetVendorEvaluationDashboardQuery ValidQuery() =>
            new()
            {
                VendorId = 1,
                EvaluationMonth = 5,
                EvaluationYear = 2026
            };

        private static VendorEvaluationDashboardDto ValidDashboardDto() =>
            new()
            {
                VendorId = 1,
                VendorName = "Test Vendor",
                EvaluationMonth = 5,
                EvaluationYear = 2026,
                Criteria = new List<DashboardCriteriaDto>
                {
                    new()
                    {
                        CriteriaId = 1,
                        CriteriaCode = "VEC001",
                        CriteriaName = "Delivery Performance",
                        CalculationType = "DELIVERY",
                        IsAutoCalculated = true,
                        AutoCalculatedScore = 85.5m,
                        WeightagePercent = 30m,
                        MinimumScore = 0m
                    },
                    new()
                    {
                        CriteriaId = 2,
                        CriteriaCode = "VEC002",
                        CriteriaName = "Manual Assessment",
                        CalculationType = null,
                        IsAutoCalculated = false,
                        AutoCalculatedScore = null,
                        WeightagePercent = 20m,
                        MinimumScore = 0m
                    }
                },
                TotalWeightedScore = 25.65m,
                ResolvedGradeId = 1,
                ResolvedGradeCode = "A",
                ResolvedGradeName = "Excellent"
            };

        [Fact]
        public async Task Handle_ValidQuery_ReturnsDashboardDto()
        {
            var query = ValidQuery();
            var expected = ValidDashboardDto();

            _mockDashboardRepo
                .Setup(r => r.VendorEvaluationCalcAsync(query.VendorId, query.EvaluationMonth, query.EvaluationYear))
                .ReturnsAsync(expected);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result!.VendorId.Should().Be(1);
            result.VendorName.Should().Be("Test Vendor");
            result.Criteria.Should().HaveCount(2);
            result.TotalWeightedScore.Should().Be(25.65m);
            result.ResolvedGradeCode.Should().Be("A");
        }

        [Fact]
        public async Task Handle_ValidQuery_PublishesAuditEvent()
        {
            var query = ValidQuery();

            _mockDashboardRepo
                .Setup(r => r.VendorEvaluationCalcAsync(query.VendorId, query.EvaluationMonth, query.EvaluationYear))
                .ReturnsAsync(ValidDashboardDto());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(query, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetDashboard" &&
                        e.ActionCode == "GetVendorEvaluationDashboardQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_VendorNotFound_ReturnsNull()
        {
            var query = ValidQuery();

            _mockDashboardRepo
                .Setup(r => r.VendorEvaluationCalcAsync(query.VendorId, query.EvaluationMonth, query.EvaluationYear))
                .ReturnsAsync((VendorEvaluationDashboardDto?)null);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_VendorNotFound_DoesNotPublishAuditEvent()
        {
            var query = ValidQuery();

            _mockDashboardRepo
                .Setup(r => r.VendorEvaluationCalcAsync(query.VendorId, query.EvaluationMonth, query.EvaluationYear))
                .ReturnsAsync((VendorEvaluationDashboardDto?)null);

            await CreateSut().Handle(query, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ValidQuery_CallsRepositoryOnce()
        {
            var query = ValidQuery();

            _mockDashboardRepo
                .Setup(r => r.VendorEvaluationCalcAsync(query.VendorId, query.EvaluationMonth, query.EvaluationYear))
                .ReturnsAsync(ValidDashboardDto());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(query, CancellationToken.None);

            _mockDashboardRepo.Verify(
                r => r.VendorEvaluationCalcAsync(query.VendorId, query.EvaluationMonth, query.EvaluationYear),
                Times.Once);
        }
    }
}
