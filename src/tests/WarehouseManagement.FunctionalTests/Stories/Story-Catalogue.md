# WarehouseManagement — Functional Story Catalogue

Functional (acceptance) tests verify multi-step **business workflows** that cross entities —
behaviour the per-entity CRUD tests in `WarehouseManagement.QATests` do **not** cover.

This catalogue is the **review gate**: each acceptance criterion (AC) is tagged
✅ [implementable now] · ⚠️ [verify against live] · 🚫 [blocked — needs seeded data] before test
code is trusted. Run a slice with `dotnet test --filter "Story=US-WH-01"`.

## Module flow

```
WarehouseMaster (warehouse type/storage/area/operation + location + capacity)
        │
        ├── RackMaster (rack within a warehouse; optional floor/aisle/level slot)
        │        │
        │        └── BinMaster (bin within a warehouse, optionally on a rack)
        │
AuditLog (read-only trail)
```

Key live facts (verified during QA authoring): the `BannariERP_QATest` clone has **0 warehouses
visible to `testsales`**, and WarehouseMaster create needs four warehouse-type MiscMaster ids
(WarehouseType / StorageType / AreaType / OperationType) + a capacity UOM that are **not
resolvable from a list endpoint on the clone**. So the create chain is blocked until that
reference data is seeded; reachability + the read surface are exercised now.

---

## US-WH-01 — Warehouse → rack → bin hierarchy  *(PARTIAL — create chain blocked)*

> As a warehouse administrator I set up a warehouse, add a rack, and add a bin so stock can be
> located.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | The warehouse list + parent-warehouse + by-unit reads are reachable | ✅ |
| 2 | A WarehouseMaster can be created (type/storage/area/operation + location + capacity) | 🚫 needs warehouse-type MiscMaster + UOM ids on the clone |
| 3 | A RackMaster can be created under that warehouse | 🚫 depends on AC2 |
| 4 | A BinMaster can be created in that warehouse (optionally on the rack) | 🚫 depends on AC2/AC3 |
| 5 | Each created node is readable by id | 🚫 depends on the create chain |
| 6 | Teardown leaf-first (bin → rack → warehouse) | 🚫 depends on the create chain |

---

## US-WH-02 — Audit trail (read-only)  *(IMPLEMENTABLE)*

> As a warehouse administrator I review the audit trail of warehouse actions.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | All warehouse audit logs can be listed | ✅ (raw list; status-only, tolerant 200/404) |
| 2 | Audit logs can be searched by a pattern (`searchPattern=`) | ⚠️ verify endpoint shape |

---

### Implementation status summary

| Story | Implementable now | Blocked |
|---|---|---|
| US-WH-01 Warehouse hierarchy | ✅ read/reachability | create chain — warehouse-type MiscMaster + UOM seed, then rack/bin |
| US-WH-02 Audit trail | ✅ read-only | — |
