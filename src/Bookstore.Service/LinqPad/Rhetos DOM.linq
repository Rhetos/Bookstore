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
    ConsoleLogger.MinLevel = EventType.Info; // Use EventType.Trace for more detailed log.
    string rhetosHostAssemblyPath = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), @"..\bin\Debug\net8.0\Bookstore.Service.dll");
    using (var scope = LinqPadRhetosHost.CreateScope(rhetosHostAssemblyPath))
    {
        var context = scope.Resolve<Common.ExecutionContext>();
        var repository = context.Repository;

        // Query data from the `Common.Claim` table:
        
        var claims = repository.Common.Claim.Query()
            .Where(c => c.ClaimResource.StartsWith("Common.") && c.ClaimRight == "New")
            .ToSimple(); // Removes ORM navigation properties from the loaded objects.
            
        claims.ToString().Dump("Common.Claims SQL query");
        claims.Dump("Common.Claims items");
        
        // Add and remove a `Common.Principal`:
        
        var testUser = new Common.Principal { Name = "Test123", ID = Guid.NewGuid() };
        repository.Common.Principal.Insert(new[] { testUser });
        repository.Common.Principal.Delete(new[] { testUser });
        
        // Print logged events for the `Common.Principal`:
        
        repository.Common.LogReader.Query()
            .Where(log => log.TableName == "Common.Principal" && log.ItemId == testUser.ID)
            .ToList()
            .Dump("Common.Principal log");
            
        Console.WriteLine("Done.");
        
        //scope.CommitAndClose(); // Database transaction is rolled back by default.
    }
}
