-- DoubleUs local database setup (SQL Server 2025 / Azure SQL compatible). UNVERIFIED: not yet executed.
-- Run once as a sysadmin (works for a native install and for the optional container):
--   sqlcmd -S localhost -E -C -i infrastructure\scripts\local-sqlserver-setup.sql -v OwnerPassword="<pw>" AppPassword="<pw>"
-- Safe to re-run. Passwords are supplied on the command line; nothing is hard-coded here.
--
-- Creates the two logins required by docs/security/TENANT_ISOLATION.md (layer 4, Row-Level Security):
--   doubleus_owner : db_owner of the DoubleUs database; used by the migration step (DDL, security policies)
--   doubleus_app   : runtime login; DML only on dbo, cannot create or alter objects or policies

:on error exit
SET NOCOUNT ON;
GO

USE [master];
IF DB_ID(N'DoubleUs') IS NULL
    CREATE DATABASE [DoubleUs] COLLATE Latin1_General_100_CI_AS_SC;   -- _SC: supplementary characters (emoji in messages)
GO

IF SUSER_ID(N'doubleus_owner') IS NULL
    CREATE LOGIN [doubleus_owner] WITH PASSWORD = N'$(OwnerPassword)', CHECK_POLICY = ON, DEFAULT_DATABASE = [DoubleUs];
IF SUSER_ID(N'doubleus_app') IS NULL
    CREATE LOGIN [doubleus_app] WITH PASSWORD = N'$(AppPassword)', CHECK_POLICY = ON, DEFAULT_DATABASE = [DoubleUs];
GO

USE [DoubleUs];
IF USER_ID(N'doubleus_owner') IS NULL CREATE USER [doubleus_owner] FOR LOGIN [doubleus_owner];
ALTER ROLE db_owner ADD MEMBER [doubleus_owner];

IF USER_ID(N'doubleus_app') IS NULL CREATE USER [doubleus_app] FOR LOGIN [doubleus_app];
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO [doubleus_app];
GO

-- Verification (expected: doubleus_app, 0)
EXECUTE AS USER = N'doubleus_app';
SELECT USER_NAME() AS CurrentUser, IS_MEMBER(N'db_owner') AS IsDbOwner;
REVERT;
GO
