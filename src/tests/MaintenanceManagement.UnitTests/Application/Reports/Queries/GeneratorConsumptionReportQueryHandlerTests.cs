using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MaintenanceManagement.Application.Reports.GeneratorConsumption;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.Reports.Queries
{
    public sealed class GeneratorConsumptionReportQueryHandlerTests
    {
        private readonly Mock<IReportRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GeneratorConsumptionReportQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static GeneratorConsumptionReportQuery ValidQuery() => new()
        {
            FromDate = DateTimeOffset.UtcNow.AddDays(-7),
            ToDate = DateTimeOffset.UtcNow
        };

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var dtos = new List<GeneratorReportDto> { new() };
            _mockRepo.Setup(r => r.GetGeneratorReports(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(dtos);
            _mockMapper.Setup(m => m.Map<List<GeneratorReportDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo.Setup(r => r.GetGeneratorReports(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(new List<GeneratorReportDto>());
            _mockMapper.Setup(m => m.Map<List<GeneratorReportDto>>(It.IsAny<object>())).Returns(new List<GeneratorReportDto>());

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_NullFromDate_ThrowsArgumentNullException()
        {
            var query = new GeneratorConsumptionReportQuery { FromDate = null, ToDate = DateTimeOffset.UtcNow };

            Func<Task> act = async () => await CreateSut().Handle(query, CancellationToken.None);
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            var dtos = new List<GeneratorReportDto>();
            _mockRepo.Setup(r => r.GetGeneratorReports(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(dtos);
            _mockMapper.Setup(m => m.Map<List<GeneratorReportDto>>(It.IsAny<object>())).Returns(dtos);

            await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            _mockRepo.Verify(r => r.GetGeneratorReports(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()), Times.Once);
        }
    }
}
