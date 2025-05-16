-- DROP SCHEMA public;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_namespace WHERE nspname = 'public') THEN
        CREATE SCHEMA public AUTHORIZATION pg_database_owner;
    END IF;
END $$;

COMMENT ON SCHEMA public IS 'standard public schema';


-- public.user_information definition

-- DROP TABLE public.user_information;

CREATE TABLE IF NOT EXISTS public.user_information (
	user_pk int4 GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	username varchar NOT NULL,
	email varchar NULL,
	"password" varchar NOT NULL,
	tax_allowance numeric DEFAULT 1000 NOT NULL,
	CONSTRAINT user_information_unique UNIQUE (username),
	CONSTRAINT user_information_unique_1 UNIQUE (email)
);


-- public.accounts definition

-- DROP TABLE public.accounts;

CREATE TABLE IF NOT EXISTS public.accounts (
	accountpk INT4 GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    accounttype VARCHAR(50),
    institutionname VARCHAR(100),
    accountnickname VARCHAR(100),
    accountnumber VARCHAR(20),
    sortcode VARCHAR(10),
    reference VARCHAR(255),
    balance DECIMAL(15, 2),
    owner INT4,
    createdat TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	CONSTRAINT accounts_unique UNIQUE ("accountnickname"),
	CONSTRAINT accounts_user_information_fk FOREIGN KEY ("owner") REFERENCES public.user_information(user_pk)
);


-- public.transactions definition

-- DROP TABLE public.transactions;

CREATE TABLE IF NOT EXISTS public.transactions (
	transactionpk int4 GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	accountfk int4 NOT NULL,
	transactionSum numeric NOT NULL,
	transactionTime timestamptz NOT NULL,
	balancebefore numeric NOT NULL,
	balanceafter numeric NOT NULL,
	CONSTRAINT transactions_account_fk FOREIGN KEY ("accountfk") REFERENCES public.accounts("accountpk") ON UPDATE CASCADE
);


-- public.targets definition

-- DROP TABLE public.targets;

CREATE TABLE IF NOT EXISTS public.targets (
	targetpk int4 GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	ownerfk int4 NOT NULL,
	accountfk varchar NOT NULL,
	"type" varchar NOT NULL,
	amount numeric NOT NULL,
	startdate timestamptz NOT NULL,
	targetdate timestamptz NOT NULL,
	note text NULL
);


-- public.reminders definition

-- DROP TABLE public.reminders;

CREATE TABLE IF NOT EXISTS public.reminders (
	reminderpk int4 GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	"date" timestamptz NOT NULL,
	note text NULL,
	userfk int4 NOT NULL
);