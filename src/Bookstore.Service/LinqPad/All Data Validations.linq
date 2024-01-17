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

/*
The code in this script is writted for Rhetos v5 and higher.

It reports all records from database that contain some invalid data, based on InvalidData concepts in DSL scritps.

In order to be useful on large applications, this script should be extended to handle any errors or performance issues from validation filters.
*/

void Main()
{
	string rhetosHostAssemblyPath = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), @"..\bin\Debug\net8.0\Bookstore.Service.dll");
	using (var scope = LinqPadRhetosHost.CreateScope(rhetosHostAssemblyPath))
	{
		var dslModel = scope.Resolve<IDslModel>();
		var genericRepositories = scope.Resolve<GenericRepositories>();
		
		var validations = dslModel.FindByType<InvalidDataInfo>();
		validations.Count().Dump("Total validations count");
		
		foreach (var validation in validations)
		{
			$"{validation.Source.FullName}: {validation.FilterType}: {validation.ErrorMessage}".Dump("Validation");
			
			var queryInvalidData = genericRepositories
				// GenericRepository is a helper for using repository classes with entity names, without referencing the entity type.
				.GetGenericRepository(validation.Source.FullName)
				// FilterCriteria is a helper for using filters with filter/property/operation names, without referencing the filter type.
				.Query(new FilterCriteria { Filter = validation.FilterType });
			
			// Preview SQL query that will be executed on database.
			queryInvalidData.ToString().Dump("SQL query");
			
			// Report the existing records with invalid data from database:
			queryInvalidData.GenericToSimple<IEntity>().ToList().Dump("Invalid records");
		}
	}
}
