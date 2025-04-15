-- DROP SCHEMA public;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_namespace WHERE nspname = 'public') THEN
        CREATE SCHEMA public AUTHORIZATION pg_database_owner;
    END IF;
END $$;


COMMENT ON SCHEMA public IS 'standard public schema';
-- public.account_type definition

-- Drop table

-- DROP TABLE public.account_type;

-- This will only create the table if it doesn't already exist
CREATE TABLE IF NOT EXISTS public.account_type (
	"Type" varchar NOT NULL,
	CONSTRAINT account_type_pk PRIMARY KEY ("Type")
);

-- public.user_information definition

-- Drop table

-- DROP TABLE public.user_information;

CREATE TABLE IF NOT EXISTS public.user_information (
	user_pk int4 NOT NULL,
	username varchar NOT NULL,
	email varchar NULL,
	"password" varchar NOT NULL,
	CONSTRAINT user_information_pk PRIMARY KEY (user_pk),
	CONSTRAINT user_information_unique UNIQUE (username),
	CONSTRAINT user_information_unique_1 UNIQUE (email)
);


-- public.accounts definition

-- Drop table

-- DROP TABLE public.accounts;

CREATE TABLE IF NOT EXISTS public.accounts (
	"AccountPK" int4 NOT NULL,
	"Owner" int4 NOT NULL,
	"InstitutionName" varchar NOT NULL,
	"AccountName" varchar NULL,
	"SortCode" varchar NULL,
	"IBAN" varchar NULL,
	"BIC" varchar NULL,
	"Reference" varchar NULL,
	"CreatedAt" timestamptz NOT NULL,
	"Balance" numeric NOT NULL,
	"Overdraft" bool DEFAULT false NOT NULL,
	"OverdraftAmount" numeric NULL,
	"InterestRate" numeric DEFAULT 0 NOT NULL,
	"OverdraftInterestRate" numeric NULL,
	"AccountNickname" varchar NOT NULL,
	CONSTRAINT accounts_pk PRIMARY KEY ("AccountPK"),
	CONSTRAINT accounts_unique UNIQUE ("AccountName"),
	CONSTRAINT accounts_user_information_fk FOREIGN KEY ("Owner") REFERENCES public.user_information(user_pk)
);


-- public.transactions definition

-- Drop table

-- DROP TABLE public.transactions;

CREATE TABLE IF NOT EXISTS public.transactions (
	"TransactionPK" int4 NOT NULL,
	"AccountFK" int4 NOT NULL,
	"TransactionSum" numeric NOT NULL,
	"TransactionTime" timestamptz NOT NULL,
	CONSTRAINT transactions_pk PRIMARY KEY ("TransactionPK"),
	CONSTRAINT transactions_accounts_fk FOREIGN KEY ("AccountFK") REFERENCES public.accounts("AccountPK") ON UPDATE CASCADE
);