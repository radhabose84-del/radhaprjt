# SalesManagement — Functional Story Catalogue

Functional (acceptance) tests verify multi-step **business workflows** that cross entities —
behaviour the per-entity CRUD tests in `SalesManagement.QATests` do **not** cover.

This catalogue is the **review gate**: each acceptance criterion (AC) is tagged
✅ [implementable now] · ⚠️ [verify against live] · 🚫 [blocked — needs seeded data] **before**
test code is trusted. Run a slice with `dotnet test --filter "Story=US-SALES-01"`.

> ⚠️ **This catalogue is awaiting approval.** No workflow test classes are authored yet.
> On approval, ✅/⚠️ ACs become active ordered steps; 🚫 ACs become `[Fact(Skip="needs seeded data: …")]`.

## Module flow (the sales lifecycle the stories follow)

```
1. Organisation & territory
   SalesOrganisation → SalesChannel · BusinessUnit → SalesSegment (composite of the three)
   SalesOrganisation → SalesOffice → SalesGroup → MarketingOfficer
        │
2. Reference & policy masters
   MiscTypeMaster → MiscMaster · SalesOrderTypeMaster · MovementTypeConfig → StoTypeMaster
   DiscountMaster · CommissionSplit · AgentCommissionConfig
        │
3. Channel partners & pricing
   MarketingOfficer → OfficerAgent (agents) → AgentCustomerMapping
   ItemPriceMaster (per SalesSegment)
        │
4. Pre-sales (lead → quote → agreement)
   SalesLead → SalesEnquiry → SalesQuotation → SalesQuotationAmendment / SalesAgreement
        │
5. Order management
   SalesOrder → ProformaInvoice → SalesOrderAmendment / cancel / foreclose
        │
6. Fulfilment & logistics
   DispatchAdvice → DeliveryChallan → TripSheet → Invoice (→ e-invoice / e-waybill)
   StoHeader → DeliveryChallan → StoReceipt → StockLedger
        │
7. After-sales
   Complaint → ComplaintQCReview → ComplaintDepartmentFeedback → ComplaintResolution → SalesReturn
        │
8. Engagement & insight
   CustomerVisit · AgentPortal · LeadConversionFunnel · SalesProjection · AuditLog
```

Key data facts (verified in source during QA authoring):
- Most masters' reads are filtered only by `IsDeleted = 0` (not company-scoped) → created rows are visible in GET-all/by-id within the run.
- `SalesSegment` is a composite of (SalesOrganisation, SalesChannel, BusinessUnit) — those three are immutable after create.
- Deactivate semantics: `PUT … IsActive=0` keeps the row in GET-all (`IsDeleted=0`) but removes it from `/by-name` autocomplete (`IsActive=1 AND IsDeleted=0`).
- Transactional documents (SalesOrder, Invoice, DispatchAdvice, DeliveryChallan, StoHeader, Complaint, SalesReturn …) need cross-module seeded data (Party customers/agents, Inventory items/lots/HSN, Warehouse plants/bins, packed stock) and prior workflow rows that the QA clone does **not** guarantee — their create steps are blocked.

---

## US-SALES-01 — Sales organisation & segment hierarchy  *(IMPLEMENTABLE)*

> As a sales administrator I build the org backbone (SalesOrganisation → SalesChannel + BusinessUnit → SalesSegment) so transactions can be classified by segment.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A SalesOrganisation can be created and returns a new id | ✅ |
| 2 | A SalesChannel can be created | ✅ |
| 3 | A BusinessUnit can be created | ✅ |
| 4 | A SalesSegment can be created from (organisation, channel, business unit) | ✅ |
| 5 | Re-creating the same (org, channel, BU) combination is rejected (composite unique) | ✅ |
| 6 | Deactivating the SalesSegment removes it from autocomplete but keeps it in GetAll | ⚠️ verify autocomplete exclusion |
| 7 | Teardown leaf-first (segment → BU/channel → organisation), each delete blocked while linked | ⚠️ verify dependent-delete block |

---

## US-SALES-02 — Sales territory setup  *(IMPLEMENTABLE)*

> As a sales administrator I set up the field hierarchy (SalesOrganisation → SalesOffice → SalesGroup) used to route sales activity.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A SalesOrganisation exists (created in-flow) | ✅ |
| 2 | A SalesOffice can be created under the organisation (city FK resolved at runtime) | ✅ |
| 3 | A SalesGroup can be created under the office | ✅ |
| 4 | Both are readable by id and appear in autocomplete | ✅ |
| 5 | Deactivating the SalesGroup excludes it from autocomplete, keeps it in GetAll | ⚠️ verify |
| 6 | Teardown leaf-first (group → office → organisation) | ✅ |

---

## US-SALES-03 — Reference & document-type masters  *(IMPLEMENTABLE)*

> As a sales administrator I define misc reference values and a sales-order type so documents can reference them.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A MiscTypeMaster can be created | ✅ |
| 2 | A MiscMaster value can be created under that type (FK MiscTypeId) | ✅ |
| 3 | The value is reachable via `by-name?MiscTypeCode=` | ⚠️ verify filter |
| 4 | A SalesOrderTypeMaster can be created (soType + taxType combo) | ⚠️ verify valid soType/taxType seed |
| 5 | Teardown (misc value → misc type; type delete blocked while linked) | ⚠️ verify block |

---

## US-SALES-04 — Stock-movement configuration  *(IMPLEMENTABLE)*

> As a sales administrator I configure movement types and an STO type so stock-transfer documents post correctly.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | Two distinct stock-type misc values exist (for from/to) | ⚠️ needs ≥2 MiscMaster rows |
| 2 | A MovementTypeConfig can be created (from ≠ to stock type) | ⚠️ depends on AC1 |
| 3 | Creating with from == to stock type is rejected | ✅ |
| 4 | An StoTypeMaster can be created referencing two distinct movement types (PGI ≠ GR) | ⚠️ needs ≥2 movement configs |
| 5 | Teardown leaf-first | ⚠️ verify |

---

## US-SALES-05 — Discount & commission policy  *(PARTIAL)*

> As a sales administrator I define a discount scheme, a commission split, and an agent commission config.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A DiscountMaster can be created with one slab (MiscMaster FKs resolved at runtime) | ⚠️ needs ≥1 MiscMaster row |
| 2 | A CommissionSplit can be created with exactly 2 role rows summing to 100% | 🚫 needs 2 distinct role MiscMaster ids |
| 3 | An AgentCommissionConfig can be created for an agent + commission split | 🚫 needs a Party agent + a CommissionSplit |
| 4 | Each policy is readable by id | ⚠️ depends on creates above |

---

## US-SALES-06 — Marketing officer & channel-partner mapping  *(PARTIAL)*

> As a sales administrator I onboard a marketing officer, attach agents, and map an agent to a customer.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A MarketingOfficer can be created under a SalesOffice with ≥1 SalesGroup | ⚠️ needs SalesOffice + SalesGroup (created in-flow) + valid officer fields |
| 2 | OfficerAgent can attach agents to the officer | 🚫 needs Party agent ids |
| 3 | An AgentCustomerMapping can link a customer to the agent under a SalesGroup | 🚫 needs Party customer + agent |
| 4 | Mappings are queryable by customer / by officer | 🚫 depends on AC3 |

---

## US-SALES-07 — Item pricing per segment  *(PARTIAL)*

> As a pricing administrator I publish an item price for a sales segment over a validity window.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A SalesSegment exists (created in-flow) | ✅ |
| 2 | An ItemPriceMaster can be created (item + segment + currency + validFrom/validTo) | 🚫 needs an Inventory item id + Currency |
| 3 | Creating with validFrom > validTo is rejected | ✅ |
| 4 | The price is retrievable via `by-item-date` / `exmill-rate` | 🚫 depends on AC2 |

---

## US-SALES-08 — Lead to quotation  *(PARTIAL)*

> As a sales executive I capture a lead, raise an enquiry, and issue a quotation.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A SalesLead can be created against a marketing officer | ⚠️ needs a MarketingOfficer (created in-flow) |
| 2 | The lead can be closed via `PUT close` | ⚠️ depends on AC1 |
| 3 | A SalesEnquiry can be raised (party + items) | 🚫 needs Party + Inventory item |
| 4 | A SalesQuotation can be issued from the enquiry | 🚫 needs party + item + HSN + terms |
| 5 | A SalesQuotationAmendment can amend an approved quotation | 🚫 needs an approved quotation |

---

## US-SALES-09 — Sales order lifecycle  *(BLOCKED — needs seeded transactional data)*

> As a sales executive I create a sales order, raise a proforma invoice, and amend/cancel/foreclose it.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A SalesOrder can be created (party + items + discounts + commission) | 🚫 large nested DTO; needs party/item/agreement/discount chain |
| 2 | A ProformaInvoice can be raised against the order | 🚫 needs the order |
| 3 | Proforma payment can be recorded (`update-payment`) | 🚫 needs the proforma |
| 4 | A SalesOrderAmendment can amend the order | 🚫 needs the order |
| 5 | The order can be cancelled / foreclosed | 🚫 needs the order |

---

## US-SALES-10 — Dispatch & invoicing  *(BLOCKED — needs packed stock)*

> As a dispatch user I advise a dispatch, generate a delivery challan, build a trip sheet, and invoice it.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A DispatchAdvice can be created against a sales order with pack ranges | 🚫 needs order + packed stock |
| 2 | A DeliveryChallan can be generated (STO or order) | 🚫 needs STO/order + stock |
| 3 | A TripSheet can group dispatch advices | 🚫 needs a dispatch advice |
| 4 | An Invoice can be raised from a dispatch advice | 🚫 needs an un-invoiced dispatch advice |
| 5 | An e-invoice / e-waybill can be generated | 🚫 needs an invoice + external API |

---

## US-SALES-11 — Stock transfer order (STO)  *(BLOCKED — needs plants/stock)*

> As a warehouse user I raise an STO, ship it on a delivery challan, and receive it.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | An StoHeader can be created (plants + storage locations + items) | 🚫 needs Org plants + Warehouse storage + Inventory items |
| 2 | A DeliveryChallan ships the STO | 🚫 needs the STO + stock |
| 3 | An StoReceipt receives the challan (accepted/damage qty) | 🚫 needs the challan |
| 4 | The StockLedger reflects the movement | 🚫 needs posted movement |

---

## US-SALES-12 — Complaint to resolution & return  *(BLOCKED — needs invoiced customer)*

> As a service user I log a complaint, run QC review, collect department feedback, resolve it, and process a sales return.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A Complaint can be logged for a customer with invoice lines | 🚫 needs customer with invoices |
| 2 | A ComplaintQCReview can be submitted with department assignments | 🚫 needs the complaint |
| 3 | ComplaintDepartmentFeedback (RCA) can be submitted per assignment | 🚫 needs an assignment |
| 4 | A ComplaintResolution of type 'Sales Return' can be recorded | 🚫 needs the complaint past QC |
| 5 | A SalesReturn can be processed against that resolution | 🚫 needs the resolution + invoice pack ranges |

---

## US-SALES-13 — Customer engagement & dashboards  *(PARTIAL — reads reachable)*

> As a marketing user I record a customer visit and review portal/dashboard insights.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A CustomerVisit can be recorded (officer + customer + products) | 🚫 needs Party customer accessible to the officer |
| 2 | The AgentPortal dashboard is reachable for the logged-in agent | ✅ read-only (agent-scoped, may be empty) |
| 3 | The LeadConversionFunnel report is reachable | ✅ |
| 4 | The SalesProjection report is reachable (Monthly/Quarterly/Yearly) | ✅ |

---

## US-SALES-14 — Audit trail  *(IMPLEMENTABLE — read-only)*

> As a sales administrator I review the audit trail of sales actions.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | All sales audit logs can be listed | ✅ |
| 2 | Audit logs can be searched by a pattern | ⚠️ verify search endpoint shape |

---

### Implementation status summary

| Story | Implementable now | Blocked on seeded data |
|---|---|---|
| US-SALES-01 Org & segment hierarchy | ✅ full | — |
| US-SALES-02 Territory setup | ✅ full | — |
| US-SALES-03 Reference & doc-type masters | ✅ mostly (⚠️ soType/taxType seed) | — |
| US-SALES-04 Movement config | ⚠️ needs ≥2 misc/movement rows | — |
| US-SALES-05 Discount & commission | partial (DiscountMaster) | CommissionSplit roles, agent config |
| US-SALES-06 Marketing & agents | partial (MarketingOfficer) | Party agents/customers |
| US-SALES-07 Item pricing | partial (segment + date-rule) | Inventory item + Currency |
| US-SALES-08 Lead to quotation | partial (Lead + close) | enquiry/quotation chain |
| US-SALES-09 Order lifecycle | reachability reads only | full order chain |
| US-SALES-10 Dispatch & invoicing | reachability reads only | packed stock |
| US-SALES-11 Stock transfer | reachability reads only | plants/stock |
| US-SALES-12 Complaint to return | reachability reads only | invoiced customer |
| US-SALES-13 Engagement & dashboards | ✅ dashboards read-only | CustomerVisit create |
| US-SALES-14 Audit trail | ✅ full (read-only) | — |

On approval: US-SALES-01/02/03/14 implement as fully-active workflows; US-SALES-04…08/13 implement
with their reachable + creatable steps active and blocked steps `[Fact(Skip="needs seeded data: …")]`;
US-SALES-09/10/11/12 implement as reachability + negative skeletons with the transactional create chain
`[Fact(Skip=…)]`, to be un-skipped once a unit-scoped QA user with seeded Party / Inventory / Warehouse /
packed-stock data is available on `BannariERP_QATest`.
