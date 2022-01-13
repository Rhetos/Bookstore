# Bookstore

Bookstore is a demo application for Rhetos development platform.
This project can be used as a *prototype* for new Rhetos applications,
regarding its project structure and content.

For the most part, this application is developed by following tutorial articles
from official [Rhetos Wiki](https://github.com/Rhetos/Rhetos/wiki).

The Bookstore application in this demo is **a business service** (Bookstore.Service)
that implements business features and database.

* It does not contain front-end implementation. This is typically a part of the N-tier
  enterprise system where a front-end is developed as a separate application
  (for example, an Angular web app).
* It exposes all business features through REST API.
  Other web protocols can be included with additional Rhetos plugins packages,
  or by implementing custom controllers.

Note that this application uses Rhetos v5.
For demo application on Rhetos v4, use branch [rhetos-4](https://github.com/Rhetos/Bookstore/tree/rhetos-4).

## Build and initial setup

Initial database setup:

* Create an empty test database and enter the database connection string
  in the machine-specific configuration file `src\Bookstore.Service\rhetos-app.local.settings.json`.
  You can find the template for that file in `tools\Configs\Templates`.

To build the application from source, run `.\Build.ps1` and `.\Test.ps1` in PowerShell console.

## Run the application

Open Bookstore.sln in Visual Studio, right-click project "Bookstore.Service" and select "Set as Startup Project".
Start the web application in Visual Studio with Debug => Start Debugging **(F5)**.

* Web browser should open automatically, displaying Swagger UI with available REST API methods.

Testing in web browser:

* To test the application, in the browser append `/rest/Common/Claim/` to the base URL.
  It should return a list of records from the database table Common.Claim in JSON format.
* Note that **anonymous** authentication is configured by default.
  User authentication may be added to the application in order to test Rhetos authorization features
  such as row permissions.

## Project structure (Bookstore.sln)

* **Bookstore.Service** - Main application (business service).
* Bookstore.Service.Test - Unit tests and integration tests.
* Bookstore.Concepts - Custom concepts library that extend Rhetos DSL programming language
  with features specific to this application.
* Bookstore.Playground - Example of a command-line utility that uses the generated business
  object model from Bookstore.Service.

Aside from the project structure, please note the following key components that
most Rhetos applications should contain:

1. The build script `Build.ps1`, that does everything needed to produce the application binaries from the source in command prompt,
   such as verifying installed prerequisites and running dotnet build.
   * Note that the build script disables automatic database updated with `-p:RhetosDeploy=False`,
     in order to cleanly separate building binaries from testing phase.
     The database is updated later in `Test.ps1` by running `rhetos.exe dbupdate`.
     See [Rhetos CLI documentation](https://github.com/Rhetos/Rhetos/wiki/Rhetos-CLI#msbuild-integration-with-rhetosmsbuild-nuget-package) for more info.
2. The test script `Test.ps1`, that updates the database, runs the automated unit tests and the integration tests.

## Unit testing

Automated tests are executed by running `.\Test.ps1` in PowerShell console.

The tests are located in projects *Bookstore.Service.Test*, within Bookstore solution.

There are two kinds of tests in this project:

1. **Standard unit tests**
   that test the features implemented in stand-alone classes and methods, without accessing the database.
    * These tests are very fast and independent of the deployment environment.
    * For example see RatingSystemTest class.
2. **Integration tests**
   that test the application together with the database.
    * These tests can test full business processes, including the business logic
      that is implemented in the database, but are slower and need a database to run
      (the database connection is set up in the application settings).
    * For example see BookTest class.

There are some important considerations for the integration tests
that we apply to keep their complexity under control:

* Each test should insert its own data and not rely on pre-existing data in the database,
  or on the data inserted by other tests.
* Each test should clear its data when finished.
  This is done automatically by `TestScope`: SQL transaction is rolled back by default
  at the end of the `using` block, unless `CommitAndClose` method is called on the scope
  (at the end of the using block).
* The tests should not be affected by the existing data in the database.
  They can be executed on an empty database (after the application is deployed to it),
  or on a database with some data previously entered by user.

## Contributions

Before creating a pull request, please run `tools\Build\Test-CleanBuild.ps1` in PowerShell console,
to make sure there are not any errors when running a clean full build without cached output binaries.
