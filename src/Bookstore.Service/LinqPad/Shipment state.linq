<Query Kind="Program">
  <Reference Relative="..\bin\Autofac.dll">..\bin\Autofac.dll</Reference>
  <Reference Relative="..\bin\EntityFramework.dll">..\bin\EntityFramework.dll</Reference>
  <Reference Relative="..\bin\EntityFramework.SqlServer.dll">..\bin\EntityFramework.SqlServer.dll</Reference>
  <Reference Relative="..\bin\NLog.dll">..\bin\NLog.dll</Reference>
  <Reference Relative="..\bin\Oracle.ManagedDataAccess.dll">..\bin\Oracle.ManagedDataAccess.dll</Reference>
  <Reference Relative="..\bin\Rhetos.Configuration.Autofac.dll">..\bin\Rhetos.Configuration.Autofac.dll</Reference>
  <Reference Relative="..\bin\Rhetos.Dom.DefaultConcepts.dll">..\bin\Rhetos.Dom.DefaultConcepts.dll</Reference>
  <Reference Relative="..\bin\Rhetos.Dom.DefaultConcepts.Interfaces.dll">..\bin\Rhetos.Dom.DefaultConcepts.Interfaces.dll</Reference>
  <Reference Relative="..\bin\Rhetos.Dom.Interfaces.dll">..\bin\Rhetos.Dom.Interfaces.dll</Reference>
  <Reference Relative="..\bin\Rhetos.Dsl.DefaultConcepts.dll">..\bin\Rhetos.Dsl.DefaultConcepts.dll</Reference>
  <Reference Relative="..\bin\Rhetos.Dsl.Interfaces.dll">..\bin\Rhetos.Dsl.Interfaces.dll</Reference>
  <Reference Relative="..\bin\Rhetos.Interfaces.dll">..\bin\Rhetos.Interfaces.dll</Reference>
  <Reference Relative="..\bin\Rhetos.Logging.Interfaces.dll">..\bin\Rhetos.Logging.Interfaces.dll</Reference>
  <Reference Relative="..\bin\Rhetos.Persistence.Interfaces.dll">..\bin\Rhetos.Persistence.Interfaces.dll</Reference>
  <Reference Relative="..\bin\Rhetos.Processing.DefaultCommands.Interfaces.dll">..\bin\Rhetos.Processing.DefaultCommands.Interfaces.dll</Reference>
  <Reference Relative="..\bin\Rhetos.Processing.Interfaces.dll">..\bin\Rhetos.Processing.Interfaces.dll</Reference>
  <Reference Relative="..\bin\Rhetos.Security.Interfaces.dll">..\bin\Rhetos.Security.Interfaces.dll</Reference>
  <Reference Relative="..\bin\Rhetos.TestCommon.dll">..\bin\Rhetos.TestCommon.dll</Reference>
  <Reference Relative="..\bin\Rhetos.Utilities.dll">..\bin\Rhetos.Utilities.dll</Reference>
  <Reference Relative="..\bin\Bookstore.Service.dll">..\bin\Bookstore.Service.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.AccountManagement.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.dll</Reference>
  <Namespace>Oracle.ManagedDataAccess.Client</Namespace>
  <Namespace>Rhetos.Configuration.Autofac</Namespace>
  <Namespace>Rhetos.Dom</Namespace>
  <Namespace>Rhetos.Dom.DefaultConcepts</Namespace>
  <Namespace>Rhetos.Dsl</Namespace>
  <Namespace>Rhetos.Dsl.DefaultConcepts</Namespace>
  <Namespace>Rhetos.Logging</Namespace>
  <Namespace>Rhetos.Persistence</Namespace>
  <Namespace>Rhetos.Security</Namespace>
  <Namespace>Rhetos.Utilities</Namespace>
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Data.Entity</Namespace>
  <Namespace>System.DirectoryServices</Namespace>
  <Namespace>System.DirectoryServices.AccountManagement</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Reflection</Namespace>
  <Namespace>System.Runtime.Serialization.Json</Namespace>
  <Namespace>System.Text</Namespace>
  <Namespace>System.Xml</Namespace>
  <Namespace>System.Xml.Serialization</Namespace>
  <Namespace>Autofac</Namespace>
  <Namespace>Rhetos.TestCommon</Namespace>
  <Namespace>Rhetos</Namespace>
</Query>

void Main()
{
    string applicationFolder = Path.GetDirectoryName(Util.CurrentQueryPath);
    ConsoleLogger.MinLevel = EventType.Trace; // Use EventType.Trace for more detailed log.
    
    using (var container = ProcessContainer.CreateTransactionScopeContainer(applicationFolder))
    {
        var context = container.Resolve<Common.ExecutionContext>();
        var repository = context.Repository;

        var s1 = new Bookstore.Shipment { DeliveryDate = DateTime.Now, TargetAddress = "Shipment 1" };
        var s2 = new Bookstore.Shipment { DeliveryDate = DateTime.Now, TargetAddress = "Shipment 2" };
        repository.Bookstore.Shipment.Insert(s1, s2);

        var approved1 = new Bookstore.ApproveShipment { ShipmentID = s1.ID, Explanation = "all good" };
        repository.Bookstore.ApproveShipment.Insert(approved1);

        // Testing that the persisted "ShipmentCurrentState" is automatically updated:
        repository.Bookstore.ComputeShipmentCurrentState.Load().OrderBy(s => s.ID).Dump("ComputeShipmentCurrentState");
        repository.Bookstore.ShipmentCurrentState.Load().OrderBy(s => s.ID).Dump("ShipmentCurrentState");
        repository.Bookstore.ShipmentGrid.Load().OrderBy(s => s.ID).Dump("ShipmentGrid");

        // If we did not have ChangesOnChangedItems in DSL script, then ShipmentCurrentState
        // would be out-of-sync after inserting "approved1".
        // The following code shows what is going on in Rhetos when executing ChangesOnChangedItems,
        // and it can help developers to write and test the ChangesOnChangedItems code snippet.

        var changedItems = new[] { approved1 };
        Guid[] needsUpdating = changedItems
            .Select(item => item.ShipmentID.Value)
            .ToArray();
        needsUpdating.Dump("needsUpdating");
        
        // "needsUpdating" represents the resulting filter that is returned by ChangesOnChangedItems code snippet.
        // KeepSynchronized concept will compare the source (ComputeShipmentCurrentState) and target (ShipmentCurrentState)
        // date based on this filter:
        
        repository.Bookstore.ShipmentCurrentState.Load(needsUpdating).Dump("ShipmentCurrentState for sync");
        repository.Bookstore.ComputeShipmentCurrentState.Load(needsUpdating).Dump("ComputeShipmentCurrentState for sync");

        // KeepSynchronized automatically calls the following recompute method when ApproveShipment is written.
        // RecomputeFromComputeShipmentCurrentState method will:
        // 1. Compare the 2 results (above), loaded with the filter that was provided by ChangesOnChangedItems.
        // 2. Insert, update or delete records in ShipmentCurrentState to match the results from ComputeShipmentCurrentState.

        repository.Bookstore.ShipmentCurrentState.RecomputeFromComputeShipmentCurrentState(needsUpdating);
        
        //container.CommitChanges(); // Database transaction is rolled back by default.
    }
}