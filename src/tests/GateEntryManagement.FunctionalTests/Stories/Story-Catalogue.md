# GateEntryManagement — Functional Story Catalogue

Functional (acceptance) tests verify multi-step **business workflows** that cross entities —
behaviour the per-entity CRUD tests in `GateEntryManagement.QATests` do **not** cover.

This catalogue is the **review gate**: each AC is tagged ✅ [implementable now] · ⚠️ [verify] ·
🚫 [blocked — needs seeded data]. Run a slice with `dotnet test --filter "Story=US-GE-01"`.

## Module flow

```
MiscTypeMaster → MiscMaster (gate reference values: receiving type, purpose of visit, VMR status, …)
VehicleMovementRecord (vehicle IN; auto VehicleMovementId via "Gate Entry" numbering)
        ├── GateInward  (inward receipt; "Gate Inward" numbering; PO/GRN bridge)
        └── GatePass    (vehicle OUT; "Gate Outward" numbering; flips VMR → OUT)
AuditLog (read-only trail)
```

Key live facts: the gate transactional documents auto-number via Finance TransactionType +
DocumentSequence ("Gate Entry" / "Gate Inward" / "Gate Outward") that are **not seeded for the
GateEntry module** on the clone, and GateInward additionally needs a warehouse + PO/GRN bridge. So
the transactional create chain is blocked; the reference-master chain + reads are exercised now.
Misc/MiscType DELETE binds id from **QUERY** `?id=`.

---

## US-GE-01 — Gate reference master setup  *(IMPLEMENTABLE)*

> As a gate administrator I define misc reference types/values (receiving type, purpose of visit,
> VMR status) used by gate documents.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A MiscTypeMaster can be created (`/api/gateentry/miscTypemaster`) | ✅ |
| 2 | A MiscMaster value can be created under that type | ✅ |
| 3 | The value is readable by id and reachable via `by-name` | ⚠️ verify filter |
| 4 | Deactivating the MiscMaster excludes it from active autocomplete, keeps it in GetAll | ⚠️ verify |
| 5 | Teardown leaf-first (misc value → misc type) | ✅ |

---

## US-GE-02 — Vehicle movement → gate documents  *(BLOCKED — needs gate numbering + upstream)*

> As a security/gate user I record a vehicle movement, raise an inward receipt, and issue a gate pass.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | The VMR / inward / gate-pass list + pending reads are reachable | ✅ |
| 2 | A VehicleMovementRecord can be created (purpose + unit; auto VehicleMovementId) | 🚫 "Gate Entry" doc-numbering not seeded for GateEntry module |
| 3 | A GateInward can be created against the VMR | 🚫 "Gate Inward" numbering + warehouse + PO/GRN bridge |
| 4 | A GatePass can be issued (flips VMR → OUT) | 🚫 "Gate Outward" numbering + a VMR |

---

## US-GE-03 — Audit trail (read-only)  *(IMPLEMENTABLE)*

> As a gate administrator I review the audit trail of gate actions.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | All gate audit logs can be listed | ✅ (raw list; status-only, tolerant 200/404) |
| 2 | Audit logs can be searched by a pattern (`searchPattern=`) | ⚠️ verify endpoint shape |

---

### Implementation status summary

| Story | Implementable now | Blocked |
|---|---|---|
| US-GE-01 Reference master setup | ✅ misc chain | — |
| US-GE-02 Vehicle movement → gate docs | ✅ read/reachability | create chain — GateEntry doc-numbering + warehouse/PO/GRN |
| US-GE-03 Audit trail | ✅ read-only | — |
