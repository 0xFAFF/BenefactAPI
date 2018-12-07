psql -U postgres -c "drop database benefact"

psql -U postgres -c "create database benefact"
psql -U postgres benefact < schema.sql

psql -U postgres benefact < test_data.sql

pause