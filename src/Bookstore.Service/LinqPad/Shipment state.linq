<Query Kind="Program">
  <Reference Relative="..\bin\Debug\net8.0\Bookstore.Service.dll">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\Bookstore.Service.dll</Reference>
  <Reference Relative="..\bin\Debug\net8.0\Bookstore.Service.deps.json">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\Bookstore.Service.deps.json</Reference>
  <Reference Relative="..\bin\Debug\net8.0\Bookstore.Service.runtimeconfig.json">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\Bookstore.Service.runtimeconfig.json</Reference>
  <Reference Relative="..\bin\Debug\net8.0\Bookstore.RhetosExtensions.dll">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\Bookstore.RhetosExtensions.dll</Reference>
  <Reference Relative="..\bin\Debug\net8.0\EntityFramework.dll">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\EntityFramework.dll</Reference>
  <Reference Relative="..\bin\Debug\net8.0\EntityFramework.SqlServer.dll">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\EntityFramework.SqlServer.dll</Reference>
  <Reference Relative="..\bin\Debug\net8.0\runtimes\win\lib\netcoreapp2.1\System.Data.SqlClient.dll">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\runtimes\win\lib\netcoreapp2.1\System.Data.SqlClient.dll</Reference>
  <Reference Relative="..\bin\Debug\net8.0\Autofac.dll">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\Autofac.dll</Reference>
  <Reference Relative="..\bin\Debug\net8.0\Microsoft.CodeAnalysis.CSharp.dll">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\Microsoft.CodeAnalysis.CSharp.dll</Reference>
  <Reference Relative="..\bin\Debug\net8.0\Microsoft.CodeAnalysis.dll">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\Microsoft.CodeAnalysis.dll</Reference>
  <Reference Relative="..\bin\Debug\net8.0\Microsoft.Extensions.Localization.Abstractions.dll">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\Microsoft.Extensions.Localization.Abstractions.dll</Reference>
  <Reference Relative="..\bin\Debug\net8.0\Newtonsoft.Json.dll">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\Newtonsoft.Json.dll</Reference>
  <Reference Relative="..\bin\Debug\net8.0\NLog.dll">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\NLog.dll</Reference>
  <Reference Relative="..\bin\Debug\net8.0\Rhetos.CommonConcepts.dll">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\Rhetos.CommonConcepts.dll</Reference>
  <Reference Relative="..\bin\Debug\net8.0\Rhetos.Core.dll">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\Rhetos.Core.dll</Reference>
  <Reference Relative="..\bin\Debug\net8.0\Rhetos.Core.DslParser.dll">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\Rhetos.Core.DslParser.dll</Reference>
  <Reference Relative="..\bin\Debug\net8.0\Rhetos.Core.Integration.dll">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\Rhetos.Core.Integration.dll</Reference>
  <Reference Relative="..\bin\Debug\net8.0\runtimes\win-x64\native\sni.dll">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\runtimes\win-x64\native\sni.dll</Reference>
  <Reference>..\runtimes\win\lib\net8.0\System.Diagnostics.EventLog.dll</Reference>
  <Reference>..\runtimes\win\lib\net8.0\System.Diagnostics.EventLog.Messages.dll</Reference>
  <Reference Relative="..\bin\Debug\net8.0\runtimes\win\lib\net8.0\System.Runtime.Caching.dll">C:\My Projects\RhetosPackages\Bookstore\src\Bookstore.Service\bin\Debug\net8.0\runtimes\win\lib\net8.0\System.Runtime.Caching.dll</Reference>
  <Namespace>Autofac</Namespace>
  <Namespace>Rhetos</Namespace>
  <Namespace>Rhetos.Configuration.Autofac</Namespace>
  <Namespace>Rhetos.Dom</Namespace>
  <Namespace>Rhetos.Dom.DefaultConcepts</Namespace>
  <Namespace>Rhetos.Dsl</Namespace>
  <Namespace>Rhetos.Dsl.DefaultConcepts</Namespace>
  <Namespace>Rhetos.Logging</Namespace>
  <Namespace>Rhetos.Persistence</Namespace>
  <Namespace>Rhetos.Processing</Namespace>
  <Namespace>Rhetos.Processing.DefaultCommands</Namespace>
  <Namespace>Rhetos.Security</Namespace>
  <Namespace>Rhetos.Utilities</Namespace>
  <Namespace>System.Data.Entity</Namespace>
  <Namespace>System.DirectoryServices</Namespace>
  <Namespace>System.Runtime.Serialization.Json</Namespace>
  <IncludeAspNet>true</IncludeAspNet>
</Query>

void Main()
{
    ConsoleLogger.MinLevel = EventType.Trace; // Use EventType.Trace for more detailed log.
    string rhetosHostAssemblyPath = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), @"..\bin\Debug\net8.0\Bookstore.Service.dll");
    using (var scope = LinqPadRhetosHost.CreateScope(rhetosHostAssemblyPath))
    {
        var context = scope.Resolve<Common.ExecutionContext>();
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
        
        //scope.CommitAndClose(); // Database transaction is rolled back by default.
    }
}