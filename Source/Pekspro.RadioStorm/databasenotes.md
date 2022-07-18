# Database notes

## Migrations

This is how to make add an migration:

    dotnet ef migrations add Init --context GeneralDatabaseContext

    dotnet ef migrations add Init --context CacheDatabaseContext

## Precompiled models

Precompiled models are used by performance reasons. These commands needs to be executed every time the database schema is changed:

    dotnet ef dbcontext optimize --context CacheDatabaseContext --output-dir CacheDatabase\CompiledModel --namespace Pekspro.RadioStorm.CacheDatabase.CompiledModel
    
    dotnet ef dbcontext optimize --context GeneralDatabaseContext --output-dir GeneralDatabase\CompiledModel --namespace Pekspro.RadioStorm.GeneralDatabase.CompiledModel
