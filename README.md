# pg-efcore-playground

Demonstrates some common patterns we use at work in EF Core and PG

- soft delete with timestamptz
- created/edited dates with timestamptz
- key vs id (use id for join, use keys for public api)
- version number as concurrency check
- enum "lib" tables
- noda time
