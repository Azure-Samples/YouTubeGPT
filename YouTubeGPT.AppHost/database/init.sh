#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
	CREATE DATABASE metadata;
EOSQL

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
	CREATE DATABASE vectors;
EOSQL

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname vectors <<-EOSQL
	CREATE EXTENSION vector;
EOSQL
