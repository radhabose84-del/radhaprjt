namespace UserManagement.QATests.Tests.AccessPolicy;

[Collection("AccessPolicyCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AccessPolicyQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/usermanagement/AccessPolicy";

    public AccessPolicyQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — CREATE  (runs FIRST — TC17 captures CreatedPolicyId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC17_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            policyCode = _f.EntityCode,
            policyName = "Test Access Policy",
            entityName = "TestEntity",
            fieldName  = "TestField"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
        var id = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC18_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            policyCode = "NOAUTH01",
            policyName = "No Auth Policy",
            entityName = "TestEntity",
            fieldName  = "TestField"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC19_Create_PolicyCodeEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            policyCode = "",
            policyName = "Test Policy",
            entityName = "TestEntity",
            fieldName  = "TestField"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(4)]
    public async Task TC20_Create_PolicyNameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            policyCode = "POLTEST03",
            policyName = "",
            entityName = "TestEntity",
            fieldName  = "TestField"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(5)]
    public async Task TC21_Create_EntityNameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            policyCode = "POLTEST04",
            policyName = "Test Policy",
            entityName = "",
            fieldName  = "TestField"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(6)]
    public async Task TC22_Create_FieldNameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            policyCode = "POLTEST05",
            policyName = "Test Policy",
            entityName = "TestEntity",
            fieldName  = ""
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(7)]
    public async Task TC23_Create_PolicyCodeContainsSpace_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            policyCode = "POL TEST",
            policyName = "Test Policy",
            entityName = "TestEntity",
            fieldName  = "TestField"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("alphanumeric");
    }

    [Fact, TestPriority(8)]
    public async Task TC24_Create_PolicyCodeContainsSpecialChar_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            policyCode = "POL@TEST!",
            policyName = "Test Policy",
            entityName = "TestEntity",
            fieldName  = "TestField"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("alphanumeric");
    }

    [Fact, TestPriority(9)]
    public async Task TC25_Create_PolicyCodeExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            policyCode = new string('A', 60),
            policyName = "Test Policy",
            entityName = "TestEntity",
            fieldName  = "TestField"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(10)]
    public async Task TC26_Create_PolicyNameExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            policyCode = "POLTEST06",
            policyName = new string('A', 120),
            entityName = "TestEntity",
            fieldName  = "TestField"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(11)]
    public async Task TC27_Create_EntityNameExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            policyCode = "POLTEST07",
            policyName = "Test Policy",
            entityName = new string('A', 120),
            fieldName  = "TestField"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(12)]
    public async Task TC28_Create_FieldNameExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            policyCode = "POLTEST08",
            policyName = "Test Policy",
            entityName = "TestEntity",
            fieldName  = new string('A', 120)
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(13)]
    public async Task TC29_Create_DuplicatePolicyCode_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            policyCode = _f.EntityCode,   // same code as TC17
            policyName = "Duplicate Policy",
            entityName = "TestEntity",
            fieldName  = "TestField"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("already exists");
    }

    [Fact, TestPriority(14)]
    public async Task TC30_Create_EmptyRequestBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — GET ALL
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(15)]
    public async Task TC01_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(0);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(15);
    }

    [Fact, TestPriority(16)]
    public async Task TC02_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(17)]
    public async Task TC03_GetAll_SearchByTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=QA");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(18)]
    public async Task TC04_GetAll_Page2PageSize5_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(2);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(5);
    }

    [Fact, TestPriority(19)]
    public async Task TC05_GetAll_NoMatchSearch_Returns200WithEmptyData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=ZZZNOMATCH999");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(0);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET BY ID
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    public async Task TC06_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId()
            .Should().Be(_f.CreatedId);
        doc.RootElement.GetProperty("data").GetProperty("policyCode").GetString()
            .Should().Be(_f.EntityCode);
    }

    [Fact, TestPriority(21)]
    public async Task TC07_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC08_GetById_NonExistentId_Returns404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact, TestPriority(23)]
    public async Task TC09_GetById_IdZero_Returns404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — AUTOCOMPLETE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(24)]
    public async Task TC10_AutoComplete_WithTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(25)]
    public async Task TC11_AutoComplete_EmptyTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(26)]
    public async Task TC12_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — UPDATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(27)]
    public async Task TC31_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id         = _f.CreatedId,
            policyName = "Updated Access Policy",
            entityName = "UpdatedEntity",
            fieldName  = "UpdatedField",
            isActive   = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
    }

    [Fact, TestPriority(28)]
    public async Task TC32_Update_Inactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id         = _f.CreatedId,
            policyName = "Updated Access Policy",
            entityName = "UpdatedEntity",
            fieldName  = "UpdatedField",
            isActive   = 0
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
    }

    [Fact, TestPriority(29)]
    public async Task TC31B_Update_ReActivate_Returns200()
    {
        // Re-activate before AssignRole section
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id         = _f.CreatedId,
            policyName = "Updated Access Policy",
            entityName = "UpdatedEntity",
            fieldName  = "UpdatedField",
            isActive   = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
    }

    [Fact, TestPriority(30)]
    public async Task TC33_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id         = _f.CreatedId,
            policyName = "Updated Policy",
            entityName = "UpdatedEntity",
            fieldName  = "UpdatedField",
            isActive   = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(31)]
    public async Task TC34_Update_PolicyNameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id         = _f.CreatedId,
            policyName = "",
            entityName = "UpdatedEntity",
            fieldName  = "UpdatedField",
            isActive   = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(32)]
    public async Task TC35_Update_EntityNameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id         = _f.CreatedId,
            policyName = "Updated Policy",
            entityName = "",
            fieldName  = "UpdatedField",
            isActive   = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(33)]
    public async Task TC36_Update_FieldNameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id         = _f.CreatedId,
            policyName = "Updated Policy",
            entityName = "UpdatedEntity",
            fieldName  = "",
            isActive   = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(34)]
    public async Task TC37_Update_PolicyNameExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id         = _f.CreatedId,
            policyName = new string('A', 120),
            entityName = "UpdatedEntity",
            fieldName  = "UpdatedField",
            isActive   = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(35)]
    public async Task TC38_Update_IsActiveInvalid_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id         = _f.CreatedId,
            policyName = "Updated Policy",
            entityName = "UpdatedEntity",
            fieldName  = "UpdatedField",
            isActive   = 2
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("0 or 1");
    }

    [Fact, TestPriority(36)]
    public async Task TC39_Update_IdZero_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id         = 0,
            policyName = "Updated Policy",
            entityName = "UpdatedEntity",
            fieldName  = "UpdatedField",
            isActive   = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(37)]
    public async Task TC40_Update_NonExistentId_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id         = 999999,
            policyName = "Updated Policy",
            entityName = "UpdatedEntity",
            fieldName  = "UpdatedField",
            isActive   = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("not found");
    }

    [Fact, TestPriority(38)]
    public async Task TC41_Update_EmptyRequestBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(39)]
    public async Task TC42_Verify_PolicyCodeImmutableAfterUpdate_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        // PolicyCode must still be the original value from TC17
        doc.RootElement.GetProperty("data").GetProperty("policyCode").GetString()
            .Should().Be(_f.EntityCode);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 8 — ASSIGN ROLE  (TC48 captures RoleAssignmentId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC48_AssignRole_HappyPath_Returns200_AndCapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/assign-role", new
        {
            accessPolicyId = _f.CreatedId,
            roleId         = _f.ValidRoleId,
            valueId        = _f.ValidValueId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
        var assignmentId = doc.RootElement.CreatedId();
        assignmentId.Should().BeGreaterThan(0);

        _f.SecondaryId = assignmentId;
    }

    [Fact, TestPriority(41)]
    public async Task TC49_AssignRole_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync($"{BaseRoute}/assign-role", new
        {
            accessPolicyId = _f.CreatedId,
            roleId         = _f.ValidRoleId,
            valueId        = _f.ValidValueId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(42)]
    public async Task TC50_AssignRole_AccessPolicyIdZero_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/assign-role", new
        {
            accessPolicyId = 0,
            roleId         = _f.ValidRoleId,
            valueId        = _f.ValidValueId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(43)]
    public async Task TC51_AssignRole_RoleIdZero_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/assign-role", new
        {
            accessPolicyId = _f.CreatedId,
            roleId         = 0,
            valueId        = _f.ValidValueId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(44)]
    public async Task TC52_AssignRole_ValueIdZero_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/assign-role", new
        {
            accessPolicyId = _f.CreatedId,
            roleId         = _f.ValidRoleId,
            valueId        = 0
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(45)]
    public async Task TC53_AssignRole_NonExistentAccessPolicyId_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/assign-role", new
        {
            accessPolicyId = 999999,
            roleId         = _f.ValidRoleId,
            valueId        = _f.ValidValueId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(46)]
    public async Task TC54_AssignRole_NonExistentRoleId_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/assign-role", new
        {
            accessPolicyId = _f.CreatedId,
            roleId         = 999999,
            valueId        = _f.ValidValueId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(47)]
    public async Task TC55_AssignRole_DuplicateAssignment_Returns400()
    {
        // Same as TC48 — duplicate must be rejected
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/assign-role", new
        {
            accessPolicyId = _f.CreatedId,
            roleId         = _f.ValidRoleId,
            valueId        = _f.ValidValueId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("already exists");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — GET ROLE ASSIGNMENTS  (after TC48 so data exists)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(48)]
    public async Task TC13_GetRoleAssignments_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}/role-assignments");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(49)]
    public async Task TC14_GetRoleAssignments_FilterByRoleId_Returns200()
    {
        var resp = await _f.Client.GetAsync(
            $"{BaseRoute}/{_f.CreatedId}/role-assignments?roleId={_f.ValidRoleId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc  = await ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");
        data.GetArrayLength().Should().BeGreaterThan(0);
        data[0].GetProperty("roleId").GetInt32().Should().Be(_f.ValidRoleId);
    }

    [Fact, TestPriority(50)]
    public async Task TC15_GetRoleAssignments_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync(
            $"{BaseRoute}/{_f.CreatedId}/role-assignments");

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(51)]
    public async Task TC16_GetRoleAssignments_NonExistentPolicyId_Returns200WithEmptyArray()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999/role-assignments");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 9 — REMOVE ROLE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(52)]
    public async Task TC56_RemoveRole_HappyPath_Returns200()
    {
        var resp = await _f.Client.DeleteAsync(
            $"{BaseRoute}/remove-role/{_f.SecondaryId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
    }

    [Fact, TestPriority(53)]
    public async Task TC57_RemoveRole_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync(
            $"{BaseRoute}/remove-role/{_f.SecondaryId}");

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(54)]
    public async Task TC58_RemoveRole_IdZero_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/remove-role/0");

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(55)]
    public async Task TC59_RemoveRole_NonExistentId_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/remove-role/999999");

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("not found");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 7 — DELETE  (ALWAYS LAST)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(56)]
    public async Task TC43_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
    }

    [Fact, TestPriority(57)]
    public async Task TC44_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(58)]
    public async Task TC45_Delete_IdZero_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=0");

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(59)]
    public async Task TC46_Delete_NonExistentId_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=999999");

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("not found");
    }

    [Fact, TestPriority(60)]
    public async Task TC47_VerifySoftDelete_GetByIdReturns404()
    {
        // After soft delete, record must be hidden from the API
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper
    // ─────────────────────────────────────────────────────────────────────────

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }
}
