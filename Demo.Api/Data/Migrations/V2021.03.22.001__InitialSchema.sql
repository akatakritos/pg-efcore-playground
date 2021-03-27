create table recipes
(
    id          bigserial                not null
        constraint recipes_pk
            primary key,
    key         uuid                     not null,
    version     integer                  not null,
    name        varchar(256)             not null,
    description text,
    prep_time   interval                 not null,
    cook_time   interval                 not null,
    created_at  timestamp with time zone not null,
    updated_at  timestamp with time zone not null,
    deleted_at  timestamp with time zone
);

alter table recipes
    owner to postgres;

create unique index recipes_key_uindex
    on recipes (key);

create table ingredients
(
    id         bigserial                not null
        constraint ingredients_pk
            primary key,
    key        uuid                     not null,
    version    integer                  not null,
    name       varchar(256)             not null,
    created_at timestamp with time zone not null,
    updated_at timestamp with time zone not null,
    deleted_at timestamp with time zone
);

alter table ingredients
    owner to postgres;

create unique index ingredients_key_uindex
    on ingredients (key);

create unique index ingredients_name_uindex
    on ingredients (name);

create table unit_of_measure_lib
(
    id   serial      not null
        constraint unit_of_measure_lib_pk
            primary key,
    name varchar(64) not null
);

alter table unit_of_measure_lib
    owner to postgres;

create table recipe_ingredients
(
    recipe_id          bigint                   not null
        constraint recipe_ingredient_recipes_id_fk
            references recipes,
    ingredient_id      bigint                   not null
        constraint recipe_ingredient_ingredients_id_fk
            references ingredients,
    key                uuid                     not null,
    version            integer                  not null,
    unit_of_measure_id integer                  not null
        constraint recipe_ingredient_unit_of_measure_lib_id_fk
            references unit_of_measure_lib,
    quantity           numeric                  not null,
    created_at         timestamp with time zone not null,
    updated_at         timestamp with time zone not null,
    deleted_at         timestamp with time zone,
    id                 bigserial                not null
        constraint recipe_ingredients_pk
            primary key
);

alter table recipe_ingredients
    owner to postgres;

create unique index recipe_ingredient_key_uindex
    on recipe_ingredients (key);

create index recipe_ingredient_recipe_id_index
    on recipe_ingredients (recipe_id);

create unique index recipe_ingredients_recipe_id_ingredient_id_uindex
    on recipe_ingredients (recipe_id, ingredient_id);

