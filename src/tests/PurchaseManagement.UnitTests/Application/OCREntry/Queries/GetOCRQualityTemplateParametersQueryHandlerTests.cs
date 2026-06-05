using Contracts.Dtos.Lookups.QC;
using Contracts.Interfaces.Lookups.QC;
using MediatR;
using PurchaseManagement.Application.OCREntry.Queries.GetOCRQualityTemplateParameters;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.OCREntry.Queries
{
    public sealed class GetOCRQualityTemplateParametersQueryHandlerTests
    {
        private readonly Mock<IQualityTemplateLookup> _mockLookup = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private GetOCRQualityTemplateParametersQueryHandler CreateSut() =>
            new(_mockLookup.Object, _mockMediator.Object);

        private void SetupMediator() =>
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        [Fact]
        public async Task Handle_ReturnsTemplateParameters()
        {
            var parameters = new List<QualityTemplateParameterLookupDto>
            {
                new() { QualityParameterId = 30, ParameterCode = "STP", ParameterName = "Staple", SequenceNo = 1 },
                new() { QualityParameterId = 31, ParameterCode = "MIC", ParameterName = "Micronaire", SequenceNo = 2 }
            };
            _mockLookup.Setup(l => l.GetParametersByTemplateIdAsync(20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(parameters);
            SetupMediator();

            var result = await CreateSut().Handle(new GetOCRQualityTemplateParametersQuery(20), CancellationToken.None);

            result.Should().HaveCount(2);
            result[0].ParameterName.Should().Be("Staple");
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockLookup.Setup(l => l.GetParametersByTemplateIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<QualityTemplateParameterLookupDto>());
            SetupMediator();

            await CreateSut().Handle(new GetOCRQualityTemplateParametersQuery(20), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
