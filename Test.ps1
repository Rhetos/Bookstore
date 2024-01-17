# Runs all unit tests.
# Prerequisites: Build.ps1.
# See Readme.md for more info.

"=== Setup ==="
$ErrorActionPreference = 'Stop'
.\tools\Build\Test-RhetosPrerequisites.ps1

"=== Deploy: Database update ==="
& '.\src\Bookstore.Service\bin\Debug\net8.0\rhetos.exe' dbupdate '.\src\Bookstore.Service\bin\Debug\net8.0\Bookstore.Service.dll'
if ($LastExitCode -ne 0) { throw "rhetos dbupdate failed." }

"=== Run all tests ==="
& dotnet test Bookstore.sln --no-build
if ($LastExitCode -ne 0) { throw "dotnet test failed." }
