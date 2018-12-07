psql -U postgres -c "create database benefact"
psql -U postgres benefact < schema.sql

pause