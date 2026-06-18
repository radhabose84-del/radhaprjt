namespace PartyManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-PTY-01 — Party reference & bank masters
//   As a party administrator I define misc reference values and a bank.
// Workflow: MiscTypeMaster → MiscMaster + BankMaster → read-back → teardown.
// Routes verified from PartyManagement.QATests: /api/party/misctypemaster, /api/party/miscmaster
// (DELETE ROUTE /{id}); /api/bankmaster (Create body wrapped { dto: { bankName } }, DELETE ROUTE /{id}).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-PTY-01-ReferenceMasters")]
[Trait("Module", "PartyManagement")]
[Trait("Story", "US-PTY-01")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_PTY_01_ReferenceMasters_Tests
{
    private readonly QAServerFixture _f;

    private const string MiscTypeRoute = "/api/party/misctypemaster";
    private const string MiscRoute     = "/api/party/miscmaster";
    private const string BankRoute     = "/api/bankmaster";

    private static int _miscTypeId;
    private static int _miscId;
    private static int _bankId;
    private static string _miscCode = string.Empty;

    public US_PTY_01_ReferenceMasters_Tests(QAServerFixture fixture) => _f = fixture;

    private string Code() => _f.EntityCode[..10];

    [Fact, TestPriority(1)]
    public async Task Step1_CreateMiscType()
    {
        var resp = await _f.Client.PostAsJsonAsync(MiscTypeRoute, new
        {
            miscTypeCode = Code(),
            description  = "QA PTY MiscType"
        });
        await QAHelper.AssertOkAsync(resp);
        _miscTypeId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _miscTypeId.Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(2)]
    public async Task Step2_CreateMiscValue_UnderType()
    {
        _miscTypeId.Should().BeGreaterThan(0, "Step1 must have created the misc type");
        _miscCode = Code();
        var resp = await _f.Client.PostAsJsonAsync(MiscRoute, new
        {
            miscTypeId  = _miscTypeId,
            code        = _miscCode,
            description = "QA PTY Misc Value"
        });
        await QAHelper.AssertOkAsync(resp);
        _miscId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _miscId.Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(3)]
    public async Task Step3_CreateBankMaster()
    {
        // note (live, reconciled 2026-06-17): BankName has an effective MaxLength(20) — keep the
        // run-unique name <= 20 chars (no "QA PTY Bank " prefix). Create returns 201 Created
        // (controller does StatusCode(201, ...)), not 200 — accept both.
        var resp = await _f.Client.PostAsJsonAsync(BankRoute, new
        {
            dto = new { bankName = ("QAB" + Code()) }
        });
        ((int)resp.StatusCode).Should().BeOneOf(new[] { 200, 201 },
            await resp.Content.ReadAsStringAsync());
        _bankId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _bankId.Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(4)]
    public async Task Step4_EachMaster_IsReadableById()
    {
        if (_miscId > 0)  await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{MiscRoute}/{_miscId}"));
        if (_bankId > 0)  await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{BankRoute}/{_bankId}"));
    }

    [Fact, TestPriority(5)]
    public async Task Step5_Teardown()
    {
        if (_miscId > 0)     await _f.Client.DeleteAsync($"{MiscRoute}/{_miscId}");
        if (_miscTypeId > 0) await _f.Client.DeleteAsync($"{MiscTypeRoute}/{_miscTypeId}");
        if (_bankId > 0)     await _f.Client.DeleteAsync($"{BankRoute}/{_bankId}");
    }
}
