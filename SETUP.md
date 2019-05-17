Development Environment
=======================

Steps for configuring the development environment for this project

PostgreSQL
----------

The default run configuration expects a PostgreSQL database running on localhost, with the user `postgres` and
no password. Edit the `data/pg_hba.conf` file in the postgres installation folder to contain the following:

```
# TYPE  DATABASE        USER            ADDRESS                 METHOD

# IPv4 local connections:
host    all             all             127.0.0.1/32            trust
# IPv6 local connections:
host    all             all             ::1/128                 trust
# Allow replication connections from localhost, by a user with the
# replication privilege.
host    replication     all             127.0.0.1/32            trust
host    replication     all             ::1/128                 trust
```

Initiating the database
-----------------------

The easiest way to get a development environment with test data in it is to use the `MockData` run configuration
in the solution. This will create the database and insert a board with the URL name benefact, and include a user
'faff@faff.faff' with password 'fafffaff'.

Alternatively the database can be created without any data using the following command:

```bash
dotnet ef database update
```

Running
-------

Using the run configuration `BenefactBackend` will launch the API on localhost port 80.
