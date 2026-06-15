namespace MaintenanceManagement.QATests.Fixtures;

// One xUnit collection per entity → each shares a single QAServerFixture (one testsales
// session). Collections are NOT parallelized (see AssemblyInfo) so sessions never clash.

[CollectionDefinition("MachineGroupCollection")] public sealed class MachineGroupCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("MachineGroupUserCollection")] public sealed class MachineGroupUserCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("MachineCollection")] public sealed class MachineCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("MachineSpecificationCollection")] public sealed class MachineSpecificationCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("MaintenanceCategoryCollection")] public sealed class MaintenanceCategoryCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("MaintenanceTypeCollection")] public sealed class MaintenanceTypeCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("CostCenterCollection")] public sealed class CostCenterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("WorkCenterCollection")] public sealed class WorkCenterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("FeederCollection")] public sealed class FeederCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("FeederGroupCollection")] public sealed class FeederGroupCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("ActivityMasterCollection")] public sealed class ActivityMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("ActivityCheckListMasterCollection")] public sealed class ActivityCheckListMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("ShiftMasterCollection")] public sealed class ShiftMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("ShiftMasterDetailCollection")] public sealed class ShiftMasterDetailCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("MaintMiscTypeMasterCollection")] public sealed class MaintMiscTypeMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("MaintMiscMasterCollection")] public sealed class MaintMiscMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("ItemCollection")] public sealed class ItemCollection : ICollectionFixture<QAServerFixture> { }

// ── Transactional ──
[CollectionDefinition("MaintenanceRequestCollection")] public sealed class MaintenanceRequestCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("MRSCollection")] public sealed class MRSCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("MainStoreStockCollection")] public sealed class MainStoreStockCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("StockLedgerCollection")] public sealed class StockLedgerCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PowerConsumptionCollection")] public sealed class PowerConsumptionCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PreventiveSchedulerCollection")] public sealed class PreventiveSchedulerCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("ServiceHistoryCollection")] public sealed class ServiceHistoryCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("WorkOrderCollection")] public sealed class WorkOrderCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SecurityCollection")] public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }
