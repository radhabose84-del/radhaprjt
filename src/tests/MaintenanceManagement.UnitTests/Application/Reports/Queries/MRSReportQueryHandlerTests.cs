using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MaintenanceManagement.Application.Reports.MRS;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.Reports.Queries
{
    public sealed class MRSReportQueryHandlerTests
    {
        private readonly Mock<IReportRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private MRSReportQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static MRSReportQuery ValidQuery() => new()
        {
            FromDate = DateTimeOffset.UtcNow.AddDays(-7),
            ToDate = DateTimeOffset.UtcNow,
            OldUnitCode = "U01"
        };

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var dtos = new List<MRSReportDto> { new() };
            _mockRepo.Setup(r => r.GetMRSReports(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>()))
                .ReturnsAsync(dtos);
            _mockMapper.Setup(m => m.Map<List<MRSReportDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockRepo.Setup(r => r.GetMRSReports(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>()))
                .ReturnsAsync(new List<MRSReportDto>());
            _mockMapper.Setup(m => m.Map<List<MRSReportDto>>(It.IsAny<object>())).Returns(new List<MRSReportDto>());

            var result = await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_NullFromDate_ThrowsArgumentNullException()
        {
            var query = new MRSReportQuery { FromDate = null, ToDate = DateTimeOffset.UtcNow };

            Func<Task> act = async () => await CreateSut().Handle(query, CancellationToken.None);
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo.Setup(r => r.GetMRSReports(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>()))
                .ReturnsAsync(new List<MRSReportDto>());
            _mockMapper.Setup(m => m.Map<List<MRSReportDto>>(It.IsAny<object>())).Returns(new List<MRSReportDto>());

            await CreateSut().Handle(ValidQuery(), CancellationToken.None);

            _mockRepo.Verify(r => r.GetMRSReports(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>()), Times.Once);
        }
    }
}
