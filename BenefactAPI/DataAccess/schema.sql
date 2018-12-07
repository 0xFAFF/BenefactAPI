
create table cards (
	id           SERIAL PRIMARY KEY NOT NULL,
	title        TEXT            NOT NULL,
	description  TEXT            NOT NULL,
	columnid     INT
);

create table categories (
	id   SERIAL PRIMARY KEY  NOT NULL,
	name TEXT             NOT NULL,
	color varchar(9),
	character varchar(1)
);

create table category_mapping (
	cardid int references cards(id),
	categoryid int references categories(id)
);

create table columns (
	ID   SERIAL PRIMARY KEY  NOT NULL,
	TITLE TEXT            NOT NULL
);
