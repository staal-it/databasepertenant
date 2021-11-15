# Database-per-tenant architecture
This repository contains a demo application and ARM-templates to show a database-per-tenant architecture. It has an API, written in .NET Core, that reads data from a tenant specific database. It uses the SQL Elastic Pools on Microsoft Azure to host all the databases and uses the Elastic Database Tooling libraries to interact with the databases.

Blog: https://www.erwinstaal.nl/posts/database-per-tenant-series/

## Credentials for the app to create databases
The app creates new databases using ARM templates. To do that, it needs a service principal in Azure with the correct permission. Use this steps to create one:
- run az ad sp create-for-rbac --sdk-auth
Some of that result needs to be stored in either App Configuration of KeyVault

### KeyVault:
ClientId & ClientSecret

### App Configuration
ResourceManagerAppRegistration:ClientId --> pointing to KeyVault, ClientId
ResourceManagerAppRegistration:ClientSecret --> pointing to KeyVault, ClientSecret
SqlResourceGroup
SubscriptionId (use az account show)
TenantId (use az account show)

### Catalog DB credentials
The credentials to the catalog db are stored in Key Vault by the Infrastructure pipeline. What needs to be added is a reference to them in App Configuration:
DatabaseConfig:DatabasePassword --> pointing to KeyVault, CatalogDbPassword
DatabaseConfig:DatabaseUser --> pointing to KeyVault, CatalogDbUserName

## Ef and EF Migrations
The migrations on the tenant databases is automaticly run while connecting to them, the migrations for the catalog database is run in the deployment pipeline. You can also use below command to trigger each manually. When you don't run the pipeline for the catalog db, you need to run the migration manually.
Install dotnet-ef using 'dotnet tool install --global dotnet-ef'

### Add Migration
To add a migration to the Catalog context run: dotnet-ef migrations add Initial --project DatabasePerTenant.Data.Catalog/ --startup-project DatabasePerTenant.Data.MigrationApp --context CatalogDbContext

To add a migration to the Tenant context run: dotnet-ef migrations add Initial --project DatabasePerTenant.Data.Tenant/ --startup-project DatabasePerTenant.Data.MigrationApp --context TenantDatabaseContext

### Run Migration
Run
From the 'DatabasePerTenant.Data.Catalog' folder:
dotnet-ef database update --startup-project ../DatabasePerTenant.Data.MigrationApp --context CatalogDbContext --connection "connection string"

From the 'DatabasePerTenant.Data.Tenant' folder:
dotnet-ef database update --startup-project ../DatabasePerTenant.Data.MigrationApp --context TenantDatabaseContext --connection "connection string"