using AutoMapper;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Application.QualityParameter.Dto;
using QCManagement.Application.QualityParameter.Queries.GetQualityParameterAutoComplete;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.QualityParameter.Queries
{
    public class GetQualityParameterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IQualityParameterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetQualityParameterAutoCompleteQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<List<QualityParameterLookupDto>>(It.IsAny<object>()))
                .Returns<object>(o => o is IEnumerable<QualityParameterLookupDto> e ? e.ToList() : new List<QualityParameterLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetQualityParameterAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_WithMatchingTerm_ReturnsLookupList()
        {
            var list = QualityParameterBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("QP", It.IsAny<CancellationToken>())).ReturnsAsync(list);

            var result = await CreateSut().Handle(new GetQualityParameterAutoCompleteQuery("QP"), CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectData()
        {
            var list = QualityParameterBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("QP", It.IsAny<CancellationToken>())).ReturnsAsync(list);

            var result = await CreateSut().Handle(new GetQualityParameterAutoCompleteQuery("QP"), CancellationToken.None);

            result[0].ParameterCode.Should().Be("QP-000001");
            result[1].ParameterCode.Should().Be("QP-000002");
        }

        [Fact]
        public async Task Handle_NoMatch_ReturnsEmpty()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("ZZZ", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<QualityParameterLookupDto>());

            var result = await CreateSut().Handle(new GetQualityParameterAutoCompleteQuery("ZZZ"), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsAutocompleteAsync_Once()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("QP", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<QualityParameterLookupDto>());

            await CreateSut().Handle(new GetQualityParameterAutoCompleteQuery("QP"), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync("QP", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
