namespace FinanceManagement.Application.Common.Options
{
    // US-GL02-10 Multi-Company COA — bound from the "MultiCompanyCoa" config section.
    // The global/group COA template is the set of GlAccountMaster rows with IsGlobal = 1 owned by the
    // TemplateCompanyId (the group company the Group Finance Controller maintains). Subsidiaries are the
    // other companies sharing that company's EntityId; they inherit copies of the template (AC1) and
    // global edits propagate to those copies unless a local override exists (AC3).
    public class MultiCompanyCoaOptions
    {
        public const string SectionName = "MultiCompanyCoa";

        // Company that owns the global COA template. Not seeded — the deployment sets this to the
        // group/holding CompanyId in AppData.Company. 0 = multi-company COA features are effectively off.
        public int TemplateCompanyId { get; set; }
    }
}
