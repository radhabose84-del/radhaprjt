using FinanceManagement.Application.GlAccountImport.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IGlAccountImport
{
    /// <summary>
    /// Two-pass COA validation (groups then accounts) against pre-loaded reference data.
    /// Pure in-memory logic: no partial commit, every failure is collected into the result (AC1).
    /// </summary>
    public interface IGlAccountImportValidator
    {
        GlAccountImportValidationResult Validate(
            IReadOnlyList<GlAccountImportRowDto> rows,
            IReadOnlyList<GlAccountImportErrorDto> parseErrors,
            GlAccountImportReferenceData referenceData);
    }
}
