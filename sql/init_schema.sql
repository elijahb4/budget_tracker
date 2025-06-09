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

-- public.accounts definition

-- Drop table

-- DROP TABLE public.accounts;

CREATE TABLE IF NOT EXISTS public.accounts (
	accountpk int4 GENERATED ALWAYS AS IDENTITY( INCREMENT BY 1 MINVALUE 1 MAXVALUE 2147483647 START 1 CACHE 1 NO CYCLE) NOT NULL,
	accounttype varchar(50) NULL,
	institutionname varchar(100) NULL,
	accountnickname varchar(100) NULL,
	accountnumber varchar(20) NULL,
	sortcode varchar(10) NULL,
	reference varchar(255) NULL,
	balance numeric(15, 2) NULL,
	"owner" int4 NULL,
	createdat timestamp DEFAULT CURRENT_TIMESTAMP NULL,
	interestrate numeric NULL,
	CONSTRAINT accounts_pkey PRIMARY KEY (accountpk),
	CONSTRAINT accounts_unique UNIQUE (accountnickname)
);


-- public.accounts foreign keys

-- ALTER TABLE public.accounts ADD CONSTRAINT accounts_user_information_fk FOREIGN KEY ("owner") REFERENCES public.user_information(user_pk);


-- public.transactions definition

-- DROP TABLE public.transactions;

CREATE TABLE IF NOT EXISTS public.transactions (
	transactionpk int4 GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	accountfk int4 NOT NULL,
	transactionSum numeric NOT NULL,
	transactionTime timestamptz NOT NULL,
	balanceprior numeric NOT NULL,
	balanceafter numeric NOT NULL,
	reference varchar(255) NULL,
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