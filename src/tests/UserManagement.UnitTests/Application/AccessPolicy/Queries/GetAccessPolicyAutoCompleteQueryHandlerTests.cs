using AutoMapper;
using MediatR;
using UserManagement.Application.AccessPolicy.Dto;
using UserManagement.Application.AccessPolicy.Queries.GetAccessPolicyAutoComplete;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.AccessPolicy.Queries
{
    public sealed class GetAccessPolicyAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IAccessPolicyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper>                      _mockMapper    = new(MockBehavior.Loose);
        private readonly Mock<IMediator>                    _mockMediator  = new(MockBehavior.Loose);

        private GetAccessPolicyAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithTerm_ReturnsMatchingList()
        {
            var list = new List<AccessPolicyDto> { AccessPolicyBuilders.ValidDto() }.AsReadOnly();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("Test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var result = await CreateSut().Handle(
                new GetAccessPolicyAutoCompleteQuery("Test"), CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].PolicyCode.Should().Be("AP001");
        }

        [Fact]
        public async Task Handle_NullTerm_UsesEmptyString()
        {
            var list = new List<AccessPolicyDto>().AsReadOnly();
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var result = await CreateSut().Handle(
                new GetAccessPolicyAutoCompleteQuery(null), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsAutocompleteOnce()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("pol", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AccessPolicyDto>().AsReadOnly());

            await CreateSut().Handle(
                new GetAccessPolicyAutoCompleteQuery("pol"), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.AutocompleteAsync("pol", It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
