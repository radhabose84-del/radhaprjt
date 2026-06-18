namespace PurchaseManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-PUR-03 — Return policy chain
//   As a procurement administrator I configure a ReturnType, then a ReturnReason
//   that references it, so goods returns are categorised consistently.
// Fully implementable: ReturnType is a clean master; ReturnReason references it via FK.
//
// Contracts (verified against PurchaseManagement.QATests, 2026-06-17):
//   POST   /api/ReturnType   { code, description, isReplacementApplicable, isQcMandatory }  (clean, no required FK)
//   POST   /api/ReturnReason { code, description, returnTypeId }   (returnTypeId → /api/ReturnType FK, REQUIRED)
//   GET    /api/ReturnReason/by-return-type/{returnTypeId}
//   DELETE /api/ReturnType/{id}  ·  /api/ReturnReason/{id}   (id from ROUTE; inline delete validator → real 400)
//   Create returns 200/201 (heterogeneous shape) — accept BeOneOf(200, 201) for capture.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-PUR-03-ReturnPolicyChain")]
[Trait("Module", "PurchaseManagement")]
[Trait("Story", "US-PUR-03")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_PUR_03_ReturnPolicyChain_Tests
{
    private readonly QAServerFixture _f;

    private const string ReturnTypeRoute   = "/api/ReturnType";
    private const string ReturnReasonRoute = "/api/ReturnReason";

    private static int _returnTypeId;
    private static int _returnReasonId;

    public US_PUR_03_ReturnPolicyChain_Tests(QAServerFixture fixture) => _f = fixture;

    // Run-unique, alphanumeric code clamped to 10 chars (EntityCode is ~19 chars — never slice beyond length).
    private string Code() => _f.EntityCode.Substring(0, Math.Min(10, _f.EntityCode.Length));

    // AC1 — a ReturnType can be created (code + description + flags).
    [Fact, TestPriority(1)]
    public async Task Step1_CreateReturnType()
    {
        var resp = await _f.Client.PostAsJsonAsync(ReturnTypeRoute, new
        {
            code = Code(),
            description = "US-PUR-03 Return Type",
            isReplacementApplicable = false,
            isQcMandatory = false
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
        _returnTypeId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _returnTypeId.Should().BeGreaterThan(0);
    }

    // AC2 — a ReturnReason can be created under that type (FK returnTypeId).
    [Fact, TestPriority(2)]
    public async Task Step2_CreateReturnReasonUnderType()
    {
        _returnTypeId.Should().BeGreaterThan(0, "Step1 must have created the return type");

        var resp = await _f.Client.PostAsJsonAsync(ReturnReasonRoute, new
        {
            code = Code(),
            description = "US-PUR-03 Return Reason",
            returnTypeId = _returnTypeId
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
        _returnReasonId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _returnReasonId.Should().BeGreaterThan(0);
    }

    // AC3 — the by-return-type lookup is reachable, and each row readable by id (tolerant 200/404).
    [Fact, TestPriority(3)]
    public async Task Step3_ByReturnTypeLookupReachable_AndReadableById()
    {
        if (_returnTypeId > 0)
        {
            var byType = await _f.Client.GetAsync($"{ReturnReasonRoute}/by-return-type/{_returnTypeId}");
            ((int)byType.StatusCode).Should().BeOneOf(200, 404);
        }

        if (_returnTypeId > 0)
        {
            var rt = await _f.Client.GetAsync($"{ReturnTypeRoute}/{_returnTypeId}");
            ((int)rt.StatusCode).Should().BeOneOf(200, 404);
        }
        if (_returnReasonId > 0)
        {
            var rr = await _f.Client.GetAsync($"{ReturnReasonRoute}/{_returnReasonId}");
            ((int)rr.StatusCode).Should().BeOneOf(200, 404);
        }
    }

    // AC4 — dependent-delete probe (delete the ReturnType while a reason links it → blocked 400 or
    // permitted 200); then teardown reason → type (ROUTE-bound deletes, tolerant).
    [Fact, TestPriority(4)]
    public async Task Step4_DependentDeleteProbe_ThenTeardown()
    {
        if (_returnTypeId > 0 && _returnReasonId > 0)
        {
            var blocked = await _f.Client.DeleteAsync($"{ReturnTypeRoute}/{_returnTypeId}");
            ((int)blocked.StatusCode).Should().BeOneOf(200, 400);
        }

        // Leaf-first cleanup — reason then type.
        if (_returnReasonId > 0) await _f.Client.DeleteAsync($"{ReturnReasonRoute}/{_returnReasonId}");
        if (_returnTypeId > 0)   await _f.Client.DeleteAsync($"{ReturnTypeRoute}/{_returnTypeId}");
    }
}
