# BSOFT ERP - Workflow Configuration Guide

**Audience:** Admins, Team Leads, Business Analysts
**Purpose:** How to configure approval workflows without code changes
**Last Updated:** 2026-03-19

---

## 1. Overview

BSOFT uses a **configurable rule engine** for approval workflows. Most workflow changes (add levels, change approvers, add conditions) are **configuration-only** — no code deployment needed.

### Architecture Summary

```
WorkflowType                    → Which module/page has workflow
  └─ ApprovalStepDetail         → Each approval level (Step 1, Step 2, ...)
       ├─ ApprovalStepUnitMapping      → Which Units use this step
       ├─ ApprovalStepDepartmentMapping → Which Departments use this step
       └─ ApprovalRule                  → Conditions that trigger this step
            ├─ ApprovalRuleCondition    → Field-based conditions (Amount > 500000)
            └─ RuleTargetOverride       → Override default approver for this rule
```

### DB Schema

All workflow tables are in the **`AppData`** schema:

| Table | Purpose |
|---|---|
| `AppData.Modules` | Registered modules (Sales, Purchase, etc.) |
| `AppData.Menus` | Pages within each module (Sales Order, Purchase Indent, etc.) |
| `AppData.WorkflowType` | Links a Menu to a workflow definition |
| `AppData.ApprovalStepDetail` | Approval levels (steps) within a workflow |
| `AppData.ApprovalStepUnitMapping` | Maps steps to organizational units |
| `AppData.ApprovalStepDepartmentMapping` | Maps steps to departments |
| `AppData.ApprovalRule` | Rules with priority and effective dates |
| `AppData.ApprovalRuleCondition` | Field-based conditions for rules |
| `AppData.ApprovalDataField` | Defines evaluable fields (FieldKey, JsonPath, ValueType) |
| `AppData.RuleTargetOverride` | Overrides default approver for specific rules |
| `AppData.ApprovalRequest` | Runtime: tracks approval requests (header) |
| `AppData.ApprovalRequestLine` | Runtime: tracks line-level approval status |
| `AppData.MiscMaster` | Lookup values (Status, TargetType, Operator, etc.) |

---

## 2. Prerequisites

Before configuring a workflow, ensure:

1. **Module is registered** in `AppData.Modules` (e.g., ModuleId=26 for Sales)
2. **Menu/Page is registered** in `AppData.Menus` (e.g., MenuId=1217 for Sales Order)
3. **Developer has integrated** the module with workflow (outbox, consumer, dispatcher routing)
4. **MiscMaster** has required lookup values:
   - Approval statuses: Pending, Approved, Rejected
   - Target types: Role, User, Department
   - Operators: >, <, =, >=, <=, !=
   - Value types: Decimal, Integer, String
   - Scope: Header, Line

### Currently Integrated Modules

| Module | ModuleId | Pages with Workflow | ModuleTypeName |
|---|---|---|---|
| Purchase | 21 | PurchaseIndent, QuotationComparison, PriceMaster, POLocal, ServicePO, ServiceEntrySheet, IssueReturn | `PurchaseIndent`, `QuotationComparison`, etc. |
| Budget | 24 | BudgetRequest | `BudgetRequest` |
| Inventory | 22 | Material Requisition Slip | `Material Requisition Slip` |
| Party | — | Party | `Party` |
| Maintenance | 20 | (Partially integrated) | — |
| Sales | 26 | Sales Order (planned) | `SalesOrder` |

---

## 3. Step-by-Step Configuration

### Step 1: Create WorkflowType

Register which page needs approval.

**API:** `POST api/WorkflowType`

```json
{
  "moduleId": 26,
  "menuId": 1217,
  "hasLine": 0,
  "isMultiselect": 0
}
```

| Field | Type | Description |
|---|---|---|
| `moduleId` | int | From `AppData.Modules` (e.g., 26 = Sales) |
| `menuId` | int | From `AppData.Menus` (e.g., 1217 = Sales Order) |
| `hasLine` | byte | `0` = Header-only approval, `1` = Line-level approval (approve individual items) |
| `isMultiselect` | byte | `0` = Single approval, `1` = Batch approval supported |

**When to use `hasLine = 1`:**
- Purchase Indent: approve/reject individual line items independently
- Sales Order with item-level approval: each item can be approved separately

**When to use `hasLine = 0`:**
- Budget Request: approve/reject the entire request as a whole
- Sales Order (typical): approve/reject the entire order

---

### Step 2: Create Approval Steps (Levels)

Define how many levels of approval and who approves at each level.

**API:** `POST api/ApprovalStepDetail`

**Example: 2-Level Approval**

**Level 1 — Sales Manager:**
```json
{
  "workFlowTypeId": 1,
  "stepOrder": 1,
  "stopOnFirstMatch": 1,
  "approvalStepId": 101,
  "targetTypeId": 201,
  "targetValueId": 301,
  "isEdit": 0
}
```

**Level 2 — Director:**
```json
{
  "workFlowTypeId": 1,
  "stepOrder": 2,
  "stopOnFirstMatch": 1,
  "approvalStepId": 102,
  "targetTypeId": 201,
  "targetValueId": 302,
  "isEdit": 0
}
```

| Field | Type | Description |
|---|---|---|
| `workFlowTypeId` | int | FK to WorkflowType created in Step 1 |
| `stepOrder` | int | Execution order: 1 = first level, 2 = second level, etc. |
| `stopOnFirstMatch` | byte | `1` = Stop evaluating rules after first match at this step |
| `approvalStepId` | int | FK to `AppData.MiscMaster` — the step label (e.g., "Level 1 Approval") |
| `targetTypeId` | int | FK to `AppData.MiscMaster` — who approves: Role, User, Department |
| `targetValueId` | int | The specific role/user/department ID |
| `isEdit` | byte | `1` = Approver can edit the transaction before approving |

**Target Type Examples:**

| TargetTypeId (MiscMaster) | Meaning | TargetValueId |
|---|---|---|
| Role | A specific role approves | RoleId from UserManagement |
| User | A specific user approves | UserId from UserManagement |
| Department | Department head approves | DepartmentId from UserManagement |

---

### Step 3: Map Steps to Units (Optional)

Restrict which organizational units use each step.

**Table:** `AppData.ApprovalStepUnitMapping`

```sql
INSERT INTO AppData.ApprovalStepUnitMapping (ApprovalStepDetailId, UnitId)
VALUES
    (1, 101),  -- Step 1 applies to Unit 101 (Plant A)
    (1, 102),  -- Step 1 applies to Unit 102 (Plant B)
    (2, 101);  -- Step 2 applies only to Unit 101
```

If no unit mapping exists for a step, the step applies to **all units**.

### Step 3b: Map Steps to Departments (Optional)

**Table:** `AppData.ApprovalStepDepartmentMapping`

```sql
INSERT INTO AppData.ApprovalStepDepartmentMapping (ApprovalStepDetailId, DepartmentId)
VALUES
    (1, 10),  -- Step 1 applies to Department 10 (Engineering)
    (2, 10);  -- Step 2 applies to Department 10
```

---

### Step 4: Create Approval Rules with Conditions

Rules define **when** a step is triggered and **what conditions** must be met.

**API:** `POST api/ApprovalRule`

```json
{
  "approvalStepDetailId": 2,
  "priority": 1,
  "actionId": 401,
  "effectiveFrom": "2026-01-01",
  "effectiveTo": "2026-12-31"
}
```

| Field | Type | Description |
|---|---|---|
| `approvalStepDetailId` | int | FK to the step this rule belongs to |
| `priority` | int | Lower = higher priority. Rules evaluated in priority order |
| `actionId` | int | FK to `AppData.MiscMaster` — what action this rule triggers |
| `effectiveFrom` | DateOnly | Rule is active from this date |
| `effectiveTo` | DateOnly | Rule expires after this date |

**Time-Bound Rules:** Set `effectiveFrom`/`effectiveTo` for seasonal or temporary approvals (e.g., stricter approval during audit period).

---

### Step 5: Add Rule Conditions

Conditions evaluate fields from the transaction's **Payload JSON**.

**Table:** `AppData.ApprovalRuleCondition`

```sql
INSERT INTO AppData.ApprovalRuleCondition (RuleId, FieldId, OperatorId, RightTypeId, RightValue, Aggregate)
VALUES
    (1, 5, 301, 401, '500000', NULL);
```

| Field | Type | Description |
|---|---|---|
| `RuleId` | int | FK to ApprovalRule |
| `FieldId` | int | FK to `AppData.ApprovalDataField` — which field to evaluate |
| `OperatorId` | int | FK to MiscMaster — comparison operator (>, <, =, >=, <=, !=) |
| `RightTypeId` | int | FK to MiscMaster — Static (fixed value) or Field (another field) |
| `RightValue` | string | The threshold value (e.g., "500000") or another FieldKey |
| `Aggregate` | string? | Optional: SUM, MAX, MIN, AVG for line-level aggregation |

**Multiple conditions on the same rule = AND logic** (all must be true).

---

### Step 6: Define Data Fields (If New Fields Needed)

Data fields tell the rule engine **where to find values** in the Payload JSON.

**Table:** `AppData.ApprovalDataField`

```sql
INSERT INTO AppData.ApprovalDataField (FieldKey, JsonPath, ValueTypeId, ScopeId)
VALUES
    ('TotalAmount',  '$.Header.TotalAmount',  501, 601),   -- Decimal, Header scope
    ('Rate',         '$.Lines[*].ExMillRate',  501, 602),   -- Decimal, Line scope
    ('Quantity',     '$.Lines[*].QtyInBags',   502, 602),   -- Integer, Line scope
    ('PartyId',      '$.Header.PartyId',       502, 601);   -- Integer, Header scope
```

| Field | Type | Description |
|---|---|---|
| `FieldKey` | string | Human-readable name (shown in UI) |
| `JsonPath` | string | JSONPath expression to extract value from Payload |
| `ValueTypeId` | int | FK to MiscMaster — Decimal, Integer, String, Date |
| `ScopeId` | int | FK to MiscMaster — Header (evaluate once) or Line (evaluate per line) |

**JsonPath Examples:**

| JsonPath | Meaning |
|---|---|
| `$.Header.TotalAmount` | Header-level field |
| `$.Lines[*].ExMillRate` | Line-level field (evaluated per line) |
| `$.Header.UnitId` | Header-level integer |

> **Important:** The Payload JSON structure is defined by the developer when integrating the module. New fields require a developer to add them to the handler's JSON serialization. See `WorkflowIntegrationGuide.md` Step 6.

---

### Step 7: Override Approvers (Optional)

Override the default approver for specific rules.

**Table:** `AppData.RuleTargetOverride`

```sql
INSERT INTO AppData.RuleTargetOverride (RuleId, Binding, Value)
VALUES
    (1, 'Role', '15');  -- For this rule, override approver to RoleId 15 (CFO)
```

| Field | Type | Description |
|---|---|---|
| `RuleId` | int | FK to ApprovalRule |
| `Binding` | string | Override target type: "Role", "User", "Department" |
| `Value` | string | The specific role/user/department ID |

**Use case:** Step 2 normally goes to Director (TargetValueId=302), but for amounts > 1M, override to CFO (RoleId 15).

---

## 4. Real-World Examples

### Example A: SalesOrder — 2-Level, Amount-Based

**Business Rule:**
- Level 1: Sales Manager approves all orders
- Level 2: Director approves only if TotalAmount > 500,000

**Configuration:**

```
WorkflowType:       ModuleId=26, MenuId=1217, HasLine=0

ApprovalStepDetail (Step 1):
  StepOrder=1, TargetType=Role, TargetValue=SalesManager

ApprovalStepDetail (Step 2):
  StepOrder=2, TargetType=Role, TargetValue=Director

ApprovalRule (for Step 2):
  Priority=1, EffectiveFrom=2026-01-01, EffectiveTo=2026-12-31

ApprovalRuleCondition:
  FieldId="TotalAmount", Operator=">", RightValue="500000"

ApprovalDataField:
  FieldKey="TotalAmount", JsonPath="$.Header.TotalAmount", ValueType=Decimal, Scope=Header
```

**Runtime Flow:**
1. Sales Order created (TotalAmount = 750,000) -> Status = "Pending"
2. `sp_EvaluateApproval` evaluates rules
3. Step 1: Sales Manager gets approval request
4. Sales Manager approves -> moves to Step 2
5. Step 2: Condition check — TotalAmount (750,000) > 500,000? **YES**
6. Director gets approval request
7. Director approves -> Status = "Approved"

**If TotalAmount = 200,000:**
- Step 1: Sales Manager approves
- Step 2: Condition check — 200,000 > 500,000? **NO** -> Step 2 skipped
- Status = "Approved" (only 1 level needed)

---

### Example B: PurchaseIndent — 3-Level, Line-Level Approval

**Business Rule:**
- Level 1: Department Head approves all indent lines
- Level 2: Purchase Manager approves lines where Rate > 10,000
- Level 3: VP approves lines where Quantity > 1,000

**Configuration:**

```
WorkflowType:       ModuleId=21, MenuId=X, HasLine=1  (line-level!)

Step 1: StepOrder=1, TargetType=Department, TargetValue=DeptHead
Step 2: StepOrder=2, TargetType=Role, TargetValue=PurchaseManager
Step 3: StepOrder=3, TargetType=Role, TargetValue=VP

Rule for Step 2: Condition → FieldKey="Rate", Operator=">", RightValue="10000"
Rule for Step 3: Condition → FieldKey="Quantity", Operator=">", RightValue="1000"

ApprovalDataField:
  FieldKey="Rate", JsonPath="$.Lines[*].Rate", Scope=Line
  FieldKey="Quantity", JsonPath="$.Lines[*].Quantity", Scope=Line
```

**Runtime:** Each line item is evaluated independently:
- Line 1: Rate=15,000, Qty=500 -> Needs Level 1 + Level 2 (rate > 10K)
- Line 2: Rate=5,000, Qty=2,000 -> Needs Level 1 + Level 3 (qty > 1K)
- Line 3: Rate=3,000, Qty=100 -> Needs Level 1 only

---

### Example C: BudgetRequest — Time-Bound Rule

**Business Rule:**
- Normal: Finance Manager approves
- During audit period (Jan-Mar): CFO must also approve

**Configuration:**

```
WorkflowType:       ModuleId=24, MenuId=X, HasLine=0

Step 1: StepOrder=1, TargetType=Role, TargetValue=FinanceManager
  (No conditions — always applies)

Step 2: StepOrder=2, TargetType=Role, TargetValue=CFO
  Rule: EffectiveFrom=2026-01-01, EffectiveTo=2026-03-31
  (No field conditions — active only during audit period)
```

**Runtime:**
- April request: Step 1 only (Step 2 rule expired)
- February request: Step 1 + Step 2 (Step 2 rule is active)

---

### Example D: Multiple Conditions (AND Logic)

**Business Rule:**
- Director approval required if TotalAmount > 500,000 **AND** PaymentType = "Credit"

```
ApprovalRule (Step 2):
  Condition 1: FieldKey="TotalAmount", Operator=">", RightValue="500000"
  Condition 2: FieldKey="PaymentType", Operator="=", RightValue="Credit"
```

Both conditions must be true for the rule to match. If only one is true, the step is skipped.

---

## 5. Approval Actions (Runtime)

### Approve/Reject via API

**API:** `POST api/ApprovalRequest/approve`

**Header-Level Approval:**
```json
{
  "requests": [
    {
      "approvalRequestHeaderId": 123,
      "moduleTransactionId": 456,
      "remark": "Approved - within budget",
      "isApproved": 1
    }
  ]
}
```

**Line-Level Approval (specific lines):**
```json
{
  "requests": [
    {
      "approvalRequestHeaderId": 123,
      "moduleTransactionId": 456,
      "remark": "Line 1 approved, Line 2 rejected",
      "isApproved": 1,
      "approvalRequestLine": [
        { "approvalRequestLineId": 10, "isApproved": 1 },
        { "approvalRequestLineId": 11, "isApproved": 0 }
      ]
    }
  ]
}
```

| Field | Type | Description |
|---|---|---|
| `isApproved` | byte | `1` = Approve, `0` = Reject |
| `remark` | string | Approver's comment |
| `approvalRequestLine` | array | Only for line-level workflows (HasLine=1) |

### View Approval History

**API:** `GET api/ApprovalRequest/approved-history`

### View Approval Status by Module Transaction

**API:** `GET api/ApprovalRequest/by-module?moduleTransactionId=456&workflowTypeId=1`

### Upload Supporting Documents

**API:** `POST api/ApprovalRequest/upload`

---

## 6. Quick Reference: What Changes Need Code vs Config

| Change | Code? | Config? | Who |
|---|---|---|---|
| Add/remove approval level | NO | YES | Admin |
| Change approver (role/user/dept) | NO | YES | Admin |
| Add amount/rate condition | NO | YES | Admin |
| Change condition threshold | NO | YES | Admin |
| Set time-bound rule | NO | YES | Admin |
| Override approver for specific rule | NO | YES | Admin |
| Map step to specific units | NO | YES | Admin |
| Switch header-only to line-level | NO | YES | Admin (change HasLine) |
| Add new condition field (e.g., Discount%) | **YES** | YES | Developer + Admin |
| Add workflow to new module (e.g., Sales) | **YES** | YES | Developer + Admin |
| Change Payload JSON structure | **YES** | — | Developer |

> For code changes, see **`WorkflowIntegrationGuide.md`**.

---

## 7. Troubleshooting

| Problem | Cause | Fix |
|---|---|---|
| "Workflow not configured" validation error | No WorkflowType for this MenuId + UnitId | Create WorkflowType + ApprovalStepDetail + map to unit |
| Approval request not created | Outbox message not published | Check outbox table, verify handler publishes to outbox |
| Step skipped unexpectedly | Rule conditions not met OR rule expired | Check EffectiveFrom/To dates and condition values |
| Wrong approver assigned | TargetValueId incorrect or RuleTargetOverride active | Verify ApprovalStepDetail.TargetValueId |
| Line-level approval not working | WorkflowType.HasLine = 0 | Set HasLine = 1 |
| Condition field not evaluating | ApprovalDataField.JsonPath doesn't match Payload | Verify JsonPath matches the handler's serialized JSON |

---

**Version:** 1.0
**Last Updated:** 2026-03-19
**Related:** `WorkflowIntegrationGuide.md` (Developer guide)
