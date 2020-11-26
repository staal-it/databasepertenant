# Database-per-tenant architecture
This repository contains a demo application and ARM-templates to show a database-per-tenant architecture. It has an API, written in .NET Core, that reads data from a tenant specific database. It uses the SQL Elastic Pools on Microsoft Azure to host all the databases and uses the Elastic Database Tooling libraries to interact with the databases.

Blog: https://www.erwinstaal.nl/posts/database-per-tenant-series/
