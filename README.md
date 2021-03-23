# pg-efcore-playground

Demonstrates some common patterns we use at work in EF Core and PG

## Soft Deletes

We like to use a `deleted_at` column to indicate a soft-deleted record. If `deleted_at` is null,
then the record is still live. If it has a `timestamptz` value in it, then it should be 
considered dead.

Entity Framework has a `HasQueryFilter` method that can automatically add a WHERE clause 
to each query. We add this to all classes inheriting from the `BaseModel`. See `ConfigureBaseModel`
in the `PlaygroundContext` class.

We also override `PlaygroundContext::SaveChangesAsync` to change `Removed` entities to
`Updated` with the `CreatedAt` set to the current time.

If you want to look up a deleted record, you would need to make a different Context instance,
perhaps passing in a constructor parameter for `seeDeletedRecords`. If true, don't configure
the `HasQueryFilter`. This instance could be used in administrator paths with dependency
injection

## CreatedAt & UpdatedAt timestamps
We can also leverage the ORM to stamp `timestamptz` values into `created_at` and `updated_at`
columns during persistance. See `PlaygroundContext::SaveChangesAsync`

## Public `key`s and Internal `id`s

We like to avoid exposing the native incrementing id that is the primary key for our records.
That would disclose information like our number of customers, or could be used for enumeration.

Instead we expose a `Guid` called the `key`. The natural `id` is used for foreign key relationships
and joins, but is not mapped to public response models.

## Version Number

We also keep a version number on each entity to serve as a concurrency check. Previous versions
are not kept in the main transactional tables, but a trigger could be configured to copy old rows
to a history table.

The version number avoids a situation where two clients read a record, independently mutate it, and
save it back. Without a concurrency check, the last one in wins, and the first one to save will
have their changes lost. 

EntityFramework increments the version number on each Update -- see `PlaygroundContext::SaveChangesAsync`.

The value is also configured as EF's `ConcurrencyToken`.

In our public APIs, mutating endpoints are expected to pass the `key` and the `version`. They
may receive a 404 if their version has been deleted (though maybe a 409 Conflict) would be
more appropriate.

## Enums

Some tables represent data that almost never changes, and certainly requires corresponding
code changes. For example, things like a Job Status will almost always require code updates
to handle a new status. We call these "lib" tables. We like to model them as enums.

This project demonstrates configuration for using enums with EntityFramework and System.Text.Json
and Swagger.

## NodaTime

We also use NodaTime instead of DateTime because it is more expressive and points the developer
toward writing correct date and time handling.

The project configures NodaTime for Swagger, Entity Framework, Dapper, and System.Text.Json

## AutoMapper

Automapper is used to enforce conventions between entity field names and public model field names.
If the public fields are named the same as the entities, less mapping and configuration code
has to be written.

It also is configured to take care of some helpful modeling exercises like bundling the `key`
and `version` into a single entity (`ModelKey`).

Its use in EF queries using `ProjectTo` further helps avoid over-loading columns. EF will only
issue queries for the data being mapped to the public API model. Internal data like the natural `id`
and `deleted_at` are automatically skipped from the query

## MiniProfiler

MiniProfiler provides simple metrics for developers to keep an eye on performance. It
exposes a UI at https://localhost:5001/profiler/results-index where recent requests can
be inspected

## Dapper

For expensive queries, Dapper can be optionally used to execute hand-tuned SQL.

## Mediatr
I'm exploring using CQRS here instead of a more traditional N-Tier architecture. For a
high performance need application, I want to be able to customize the SQL for each endpoint.
Having a common "Service" or "Repository" layer forces you to return fully hydrated objects
because you don't know what your consumers are going to do with them. This leads to making joins
or full column queries when its often not needed. Tuning requires introducing more methods or
overloads which are only used in one place anyway.

A Handler for each endpoint lets us be more specific: each endpoint only needs to query exactly
the data it needs. Those queries can be initially written in EF when convenient, and then
migrated to hand-tuned SQL when necessary.

It also gives us some valuable hooks for cross-cutting concerns via Mediatr pipelines, or
decorators configured in the DI layer.

### Validations

We're using FluentValidations, which could plug in to the standard ASP.NET Core model binding
path. However, I kind of like them as a Mediatr pipeline because we could implement async
validations: like checking if something already exists.

### Perf Monitoring

A Pipeline wraps each handler with MiniProfiler timings

### Logging

A Pipeline can log commands that are completed

### Authorization

A pipeline can perform some kinds of authorization automatically.

### Caching

A pipeline can cache responses based on keys determined by the Request
