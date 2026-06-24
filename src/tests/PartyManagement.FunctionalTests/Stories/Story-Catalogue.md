# PartyManagement — Functional Story Catalogue

Functional (acceptance) tests verify multi-step **business workflows** that cross entities —
behaviour the per-entity CRUD tests in `PartyManagement.QATests` do **not** cover.

This catalogue is the **review gate**: each AC is tagged ✅ [implementable now] · ⚠️ [verify] ·
🚫 [blocked — needs seeded data]. Run a slice with `dotnet test --filter "Story=US-PTY-01"`.

PartyManagement is the cross-module source of Customer/Agent/Vendor parties (consumed by Sales,
Purchase, etc.) plus bank + reference masters.

## Module flow

```
MiscTypeMaster → MiscMaster (party reference values: registration type, party type, …)
BankMaster → BankAccount (owner = Party/Company)
PartyGroup (group type) ── used by ──┐
                                     ▼
PartyMaster (party + types[]/contacts[]/addresses[]/unitCompanies[]; customer/agent/vendor)
GST (external GSTIN lookup) · AuditLog (read-only)
```

Key live facts: BankMaster, MiscType→Misc, and PartyGroup are clean masters (creatable);
PartyMaster create is a large nested cross-module payload (company/unit + registration/party-type
misc + party group + unique contact email/mobile) — blocked on the clone; BankAccount needs a
cross-module branchId. DELETE: Misc/BankMaster/BankAccount/PartyMaster ROUTE `/{id}`; PartyGroup
QUERY `?id=`. GST is an external-service proxy (tolerant).

---

## US-PTY-01 — Party reference & bank masters  *(IMPLEMENTABLE)*

> As a party administrator I define misc reference values and a bank so parties can reference them.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A MiscTypeMaster can be created (`/api/party/misctypemaster`) | ✅ |
| 2 | A MiscMaster value can be created under that type | ✅ |
| 3 | A BankMaster can be created (`/api/bankmaster`) | ✅ |
| 4 | Each is readable by id and reachable via autocomplete | ⚠️ verify |
| 5 | Teardown leaf-first | ✅ |

---

## US-PTY-02 — Party onboarding  *(PARTIAL — PartyMaster create blocked)*

> As a party administrator I set up a party group and onboard a party with its bank account.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A PartyGroup can be created (groupType misc) | ⚠️ needs a party MiscMaster groupType + glCategoryId |
| 2 | The party / bank-account list + autocomplete reads are reachable | ✅ |
| 3 | A PartyMaster can be created (types/contacts/addresses/unitCompanies) | 🚫 nested cross-module chain (company/unit + registration/party-type misc + party group + unique email/mobile) |
| 4 | A BankAccount can be created for the party (bank + branch + account type) | 🚫 cross-module branchId |

---

## US-PTY-03 — Audit trail & GST lookup  *(IMPLEMENTABLE — read-only)*

> As a party administrator I review the audit trail and look up a GSTIN.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | All party audit logs can be listed | ✅ (raw list; tolerant 200/404) |
| 2 | Audit logs can be searched by a pattern | ⚠️ verify endpoint shape |
| 3 | The GST auth + GSTIN-lookup endpoints are reachable | ⚠️ external service (tolerant 200/400/404/500) |

---

## US-PTY-04 — Broker / Ginner types + Ginner Location & Station  *(PARTIAL — exclusivity + reads active; full create blocked)*

> As a party administrator I type a party as BROKER (peer of AGENT, own config tab) or GINNER
> (peer of SUPPLIER, with a Location + Station on its address that OCR reads to prefill), under the
> rule that a party can never be both Agent and Broker, nor both Supplier and Ginner.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A party submitted as both AGENT and BROKER is rejected (400) | ✅ (exclusivity when BROKER seeded; nested-chain validation otherwise — either way 400) |
| 2 | A party submitted as both SUPPLIER and GINNER is rejected (400) | ✅ |
| 3 | The Ginner / Supplier+Ginner `by-name` list + Ginner group `load` (OCR prefill source) are reachable | ✅ |
| 4 | A full Broker (config tab) + Ginner (Location+Station on address) can be created | 🚫 needs BROKER/GINNER party-type + group seed + nested cross-module party chain |
| 5 | The picker reads reject anonymous callers (401) | ✅ |

---

### Implementation status summary

| Story | Implementable now | Blocked |
|---|---|---|
| US-PTY-01 Reference & bank masters | ✅ misc + bank | — |
| US-PTY-02 Party onboarding | ⚠️ PartyGroup + reads | PartyMaster nested chain; BankAccount branchId |
| US-PTY-03 Audit & GST lookup | ✅ read-only | — |
| US-PTY-04 Broker/Ginner + Location/Station | ✅ exclusivity negatives + reads | full Broker/Ginner create (party-type/group seed + nested chain) |
