using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers;
using FinanceManagement.Application.DocumentSequence.Commands.CreateDocumentSequence;
using FinanceManagement.Application.DocumentSequence.Commands.UpdateDocumentSequence;
using FinanceManagement.Application.DocumentSequence.Commands.DeleteDocumentSequence;
using FinanceManagement.Application.DocumentSequence.Queries.GetAllDocumentSequence;
using FinanceManagement.Application.DocumentSequence.Queries.GetDocumentSequenceById;
using FinanceManagement.Application.DocumentSequence.Queries.GetDocumentSequenceAutoComplete;
using FinanceManagement.Application.DocumentSequence.Dto;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class DocumentSequenceControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DocumentSequenceController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllDocumentSequenceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<DocumentSequenceDto>>
                {
                    IsSuccess = true,
                    Data = new List<DocumentSequenceDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllDocumentSequenceAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllDocumentSequenceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<DocumentSequenceDto>>
                {
                    IsSuccess = true,
                    Data = new List<DocumentSequenceDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllDocumentSequenceAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllDocumentSequenceQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDocumentSequenceByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DocumentSequenceDto());

            var result = await CreateSut().GetDocumentSequenceByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDocumentSequenceAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<DocumentSequenceLookupDto>)new List<DocumentSequenceLookupDto>());

            var result = await CreateSut().GetDocumentSequenceAutoCompleteAsync("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDocumentSequenceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created", Data = 1 });

            var result = await CreateSut().CreateDocumentSequence(new CreateDocumentSequenceCommand
            {
                TransactionTypeId = 1,
                FinancialYearId = 1,
                DocNo = 100
            });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateDocumentSequenceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Updated", Data = 1 });

            var result = await CreateSut().UpdateDocumentSequence(new UpdateDocumentSequenceCommand
            {
                Id = 1,
                TransactionTypeId = 1,
                FinancialYearId = 1,
                DocNo = 100,
                IsActive = 1
            });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteDocumentSequenceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteDocumentSequence(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteDocumentSequenceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteDocumentSequence(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteDocumentSequenceCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
