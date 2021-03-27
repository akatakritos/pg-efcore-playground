insert into "unit_of_measure_lib" ("id", "name") values
 (1, 'Teaspoon'),
 (2, 'Tablespoon'),
 (3, 'Cup'),
 (4, 'Pint'),
 (5, 'Quart'),
 (6, 'Gallon'),
 (7, 'Ounce')
 on conflict ("id") do update set "name" = excluded.name
