using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers.JournalMaster;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.CreateJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.UpdateJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.DeleteJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Queries.GetAllJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Queries.GetJournalThresholdRuleById;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Queries.GetJournalThresholdRuleAutoComplete;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Queries.GetAllJournalFlag;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class JournalThresholdRuleControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private JournalThresholdRuleController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllJournalThresholdRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<JournalThresholdRuleDto>> { IsSuccess = true, Data = new() });
            (await CreateSut().GetAllAsync(1, 10)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetJournalThresholdRuleAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<JournalThresholdRuleLookupDto>)JournalThresholdRuleBuilders.ValidLookupList());
            (await CreateSut().GetAutoCompleteAsync("Amount")).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetJournalThresholdRuleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(JournalThresholdRuleBuilders.ValidDto());
            (await CreateSut().GetByIdAsync(1)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetFlags_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllJournalFlagQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<JournalFlagDto>> { IsSuccess = true, Data = new() });
            (await CreateSut().GetFlagsAsync(1, 10)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateJournalThresholdRuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });
            (await CreateSut().Create(JournalThresholdRuleBuilders.ValidCreateCommand())).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateJournalThresholdRuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });
            (await CreateSut().Update(JournalThresholdRuleBuilders.ValidUpdateCommand())).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteJournalThresholdRuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            (await CreateSut().Delete(1)).Should().BeOfType<OkObjectResult>();
        }
    }
}
