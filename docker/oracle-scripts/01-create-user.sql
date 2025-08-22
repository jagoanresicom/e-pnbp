-- Connect to PDB
ALTER SESSION SET CONTAINER = ORCLPDB1;

-- Create PNBP user
CREATE USER pnbp IDENTIFIED BY pnbp;

-- Grant necessary privileges
GRANT CREATE SESSION TO pnbp;
GRANT CREATE TABLE TO pnbp;
GRANT CREATE VIEW TO pnbp;
GRANT CREATE PROCEDURE TO pnbp;
GRANT CREATE SEQUENCE TO pnbp;
GRANT CREATE TRIGGER TO pnbp;
GRANT CREATE INDEX TO pnbp;
GRANT CREATE SYNONYM TO pnbp;
GRANT CREATE TYPE TO pnbp;
GRANT UNLIMITED TABLESPACE TO pnbp;

-- Grant additional privileges for web application
GRANT SELECT_CATALOG_ROLE TO pnbp;
GRANT EXECUTE_CATALOG_ROLE TO pnbp;
GRANT RESOURCE TO pnbp;
GRANT CONNECT TO pnbp;

-- Grant system privileges needed for membership provider
GRANT CREATE USER TO pnbp;
GRANT ALTER USER TO pnbp;
GRANT DROP USER TO pnbp;

-- Create KKPWEBDEV user (if needed based on config)
CREATE USER kkpwebdev IDENTIFIED BY kkpwebdev;
GRANT CONNECT, RESOURCE TO kkpwebdev;
GRANT UNLIMITED TABLESPACE TO kkpwebdev;

-- Commit changes
COMMIT;

-- Show created users
SELECT username, account_status, created FROM dba_users WHERE username IN ('PNBP', 'KKPWEBDEV');

EXIT;