using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalImport;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.JournalImport.Commands.ImportJournals;
using FinanceManagement.Application.JournalMaster.JournalImport.Commands.ImportJournalsFile;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FinanceManagement.UnitTests.Application.JournalImport
{
    public sealed class ImportJournalsFileCommandHandlerTests
    {
        private readonly Mock<IJournalImportFileService> _fileSvc = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Strict);

        private ImportJournalsFileCommandHandler CreateSut() => new(_fileSvc.Object, _mediator.Object);

        private static Mock<IFormFile> FileMock(string name = "import.xlsx", long len = 100)
        {
            var f = new Mock<IFormFile>();
            f.Setup(x => x.FileName).Returns(name);
            f.Setup(x => x.Length).Returns(len);
            f.Setup(x => x.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            return f;
        }

        [Fact]
        public async Task NoFile_Fails()
        {
            var result = await CreateSut().Handle(new ImportJournalsFileCommand { File = null }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ParseErrors_ReturnsFailed_AndDoesNotImport()
        {
            var file = FileMock();
            _fileSvc.Setup(s => s.IsSupported("import.xlsx")).Returns(true);
            _fileSvc.Setup(s => s.Parse(It.IsAny<Stream>(), "import.xlsx"))
                .Returns((
                    (IReadOnlyList<JournalImportRowInputDto>)new List<JournalImportRowInputDto>(),
                    (IReadOnlyList<JournalImportErrorDto>)new List<JournalImportErrorDto>
                    {
                        new() { RowNo = 2, ColumnName = "VoucherDate", Message = "bad date" }
                    }));

            var result = await CreateSut().Handle(new ImportJournalsFileCommand { File = file.Object }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data!.Status.Should().Be("FAILED");
            result.Data.Errors.Should().ContainSingle();
            _mediator.Verify(m => m.Send(It.IsAny<ImportJournalsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ValidFile_DelegatesToImportCommand()
        {
            var file = FileMock();
            var rows = new List<JournalImportRowInputDto> { new() { GroupNo = 1, VoucherTypeId = 1, GlAccountId = 5, CurrencyId = 1, DrAmount = 1000 } };
            _fileSvc.Setup(s => s.IsSupported("import.xlsx")).Returns(true);
            _fileSvc.Setup(s => s.Parse(It.IsAny<Stream>(), "import.xlsx"))
                .Returns((
                    (IReadOnlyList<JournalImportRowInputDto>)rows,
                    (IReadOnlyList<JournalImportErrorDto>)new List<JournalImportErrorDto>()));
            _mediator.Setup(m => m.Send(It.IsAny<ImportJournalsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<ImportJournalsResultDto>
                {
                    IsSuccess = true,
                    Message = "Import committed 1 draft journal(s).",
                    Data = new ImportJournalsResultDto { Status = "COMMITTED", Committed = true }
                });

            var result = await CreateSut().Handle(new ImportJournalsFileCommand { File = file.Object }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _mediator.Verify(m => m.Send(
                It.Is<ImportJournalsCommand>(c => c.Rows.Count == 1 && c.FileName == "import.xlsx"),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
