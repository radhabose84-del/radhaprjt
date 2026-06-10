namespace MaintenanceManagement.FunctionalTests.Fixtures;

// One collection per workflow story. Steps within a story are ordered via [TestPriority]
// and share a single QAServerFixture (one login, one run-unique EntityCode).

[CollectionDefinition("US-MNT-01-MachineGroupSetup")]
public sealed class MachineGroupSetupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-MNT-02-MachineOnboarding")]
public sealed class MachineOnboardingCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-MNT-03-MaintenanceReferenceSetup")]
public sealed class MaintenanceReferenceSetupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-MNT-04-ActivityChecklistSetup")]
public sealed class ActivityChecklistSetupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-MNT-05-PreventiveScheduling")]
public sealed class PreventiveSchedulingCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-MNT-06-RequestToWorkOrder")]
public sealed class RequestToWorkOrderCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-MNT-07-SparesRequisition")]
public sealed class SparesRequisitionCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-MNT-08-PowerConsumption")]
public sealed class PowerConsumptionCollection : ICollectionFixture<QAServerFixture> { }
