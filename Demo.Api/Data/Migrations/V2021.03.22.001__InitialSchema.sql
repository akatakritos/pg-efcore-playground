create table if not exists customers
(
	id serial not null
		constraint customers_pk
			primary key,
	key uuid default gen_random_uuid() not null,
	version integer default 1 not null,
	name varchar(64) not null,
	created_at timestamp with time zone not null,
	updated_at timestamp with time zone not null,
	deleted_at timestamp with time zone
);

alter table customers owner to postgres;

create unique index if not exists customers_key_uindex
	on customers (key);

create table if not exists order_type_lib
(
	id integer not null
		constraint order_type_lib_pk
			primary key,
	description text not null
);

alter table order_type_lib owner to postgres;

create table if not exists orders
(
	id serial not null
		constraint orders_pk
			primary key,
	key uuid default gen_random_uuid() not null,
	version integer default 1 not null,
	customer_id integer not null
		constraint orders_customers_id_fk
			references customers
				on delete cascade,
	created_at timestamp with time zone not null,
	updated_at timestamp with time zone not null,
	deleted_at timestamp with time zone,
	order_type_id integer default 1 not null
		constraint orders_order_type_lib_id_fk
			references order_type_lib
				on delete cascade
);

alter table orders owner to postgres;

create unique index if not exists orders_key_uindex
	on orders (key);

create table if not exists line_items
(
	id serial not null
		constraint line_items_pk
			primary key,
	key uuid default gen_random_uuid() not null,
	version integer default 1 not null,
	order_id integer not null
		constraint line_items_orders_id_fk
			references orders
				on delete cascade,
	item_count integer not null,
	unit_price numeric(18,2) not null,
	created_at timestamp with time zone not null,
	updated_at timestamp with time zone not null,
	deleted_at timestamp with time zone,
	product varchar(128) not null
);

alter table line_items owner to postgres;

create unique index if not exists line_items_key_uindex
	on line_items (key);

