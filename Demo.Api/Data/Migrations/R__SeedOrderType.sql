insert into "order_type_lib" ("id", "description") values
 (1, 'Normal'),
 (2, 'Employee')
 on conflict ("id") do update set "description" = excluded.description
