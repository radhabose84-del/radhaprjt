using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers.JournalMaster;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.CreateRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.UpdateRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.DeleteRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetAllRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetRecurringJournalTemplateById;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetRecurringJournalTemplateAutoComplete;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class RecurringJournalTemplateControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private RecurringJournalTemplateController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllRecurringJournalTemplateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<RecurringJournalTemplateHeaderDto>> { IsSuccess = true, Data = new() });

            (await CreateSut().GetAllAsync(1, 10)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetRecurringJournalTemplateAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<RecurringJournalTemplateLookupDto>)RecurringTemplateBuilders.ValidLookupList());

            (await CreateSut().GetAutoCompleteAsync("Rent")).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetRecurringJournalTemplateByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(RecurringTemplateBuilders.ValidDto());

            (await CreateSut().GetByIdAsync(1)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateRecurringJournalTemplateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            (await CreateSut().Create(RecurringTemplateBuilders.ValidCreateCommand())).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateRecurringJournalTemplateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            (await CreateSut().Update(RecurringTemplateBuilders.ValidUpdateCommand())).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteRecurringJournalTemplateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            (await CreateSut().Delete(1)).Should().BeOfType<OkObjectResult>();
        }
    }
}
