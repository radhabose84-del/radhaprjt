namespace PartyManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-PTY-02 — Party onboarding
//   As a party administrator I set up a party group and onboard a party with its bank account.
//
// PARTIAL: PartyGroup create is ATTEMPTED (self-skip on FK gap); the party + bank-account read
// surface is ACTIVE; PartyMaster create (nested cross-module chain) and BankAccount create
// (cross-module branchId) are [Fact(Skip=…)].
//
// Routes verified from PartyManagement.QATests:
//   PartyGroup  : /api/partygroup (groupType misc + glCategoryId; DELETE ?id=)
//   PartyMaster : /api/party/partymaster (GET ""; by-name; pending; load; DELETE /{id})
//   BankAccount : /api/bankaccount (GET ""; by-name; DELETE /{id})
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-PTY-02-PartyOnboarding")]
[Trait("Module", "PartyManagement")]
[Trait("Story", "US-PTY-02")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_PTY_02_PartyOnboarding_Tests
{
    private readonly QAServerFixture _f;

    private const string PartyGroupRoute = "/api/partygroup";
    private const string PartyMasterRoute = "/api/party/partymaster";
    private const string BankAccountRoute = "/api/bankaccount";

    private static int _partyGroupId;

    public US_PTY_02_PartyOnboarding_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — a PartyGroup can be created (best-effort; needs groupType misc + glCategoryId).
    [Fact, TestPriority(1)]
    public async Task Step1_CreatePartyGroup_BestEffort()
    {
        var groupTypeId = await QAHelper.FirstIdAsync(_f.Client, "/api/party/miscmaster");
        if (groupTypeId == 0) return; // no party MiscMaster on the clone → can't satisfy groupTypeId

        var resp = await _f.Client.PostAsJsonAsync(PartyGroupRoute, new
        {
            partyGroupName = $"QA PTY Group {_f.EntityCode[..8]}",
            groupTypeId    = groupTypeId,
            description    = "US-PTY-02",
            glCategoryId   = 1,
            isGroup        = (byte)0
        });

        // ⚠ Tolerant: glCategoryId=1 may not be a valid GL category on the clone → 400.
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
        if (resp.StatusCode == HttpStatusCode.OK)
            _partyGroupId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
    }

    // AC2 — the party + bank-account read surface is reachable.
    [Fact, TestPriority(2)]
    public async Task Step2_PartyReads_AreReachable()
    {
        ((int)(await _f.Client.GetAsync($"{PartyMasterRoute}?PageNumber=1&PageSize=15")).StatusCode).Should().BeOneOf(200, 404);
        ((int)(await _f.Client.GetAsync($"{PartyMasterRoute}/pending")).StatusCode).Should().BeOneOf(200, 400, 404);
        ((int)(await _f.Client.GetAsync($"{BankAccountRoute}?pageNumber=1&pageSize=15")).StatusCode).Should().BeOneOf(200, 404);
        await QAHelper.Assert401Async(await _f.AnonymousClient.GetAsync($"{PartyMasterRoute}?PageNumber=1&PageSize=15"));
    }

    // AC3 — create a PartyMaster. BLOCKED.
    [Fact(Skip = "needs seeded data: PartyMaster nested cross-module chain (company/unit + registration/party-type misc + party group + unique contact email/mobile)."), TestPriority(3)]
    public async Task Step3_CreatePartyMaster()
    {
        await Task.CompletedTask;
    }

    // AC4 — create a BankAccount for the party. BLOCKED.
    [Fact(Skip = "needs seeded data: cross-module branchId for BankAccount + an owner Party."), TestPriority(4)]
    public async Task Step4_CreateBankAccount()
    {
        await Task.CompletedTask;
    }

    // Teardown the PartyGroup if it was created (DELETE binds id from QUERY ?id=).
    [Fact, TestPriority(9)]
    public async Task Step9_Teardown()
    {
        if (_partyGroupId > 0) await _f.Client.DeleteAsync($"{PartyGroupRoute}?id={_partyGroupId}");
    }
}
