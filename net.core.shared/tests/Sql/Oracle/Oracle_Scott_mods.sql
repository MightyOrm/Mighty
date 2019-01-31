-- Code to modify the stock Oracle SCOTT database so as to pass the Massive/Mighty test suite:

-- Upgrade DEPTNO primary key to allow a reasonable number of digits
ALTER TABLE DEPT MODIFY (DEPTNO NUMBER(6,0));

-- Remove rows that aren't in the default SCOTT db
DELETE FROM DEPT WHERE DEPTNO > 40;

-- Drop the sequence
-- TO DO: Make this a conditional drop
DROP SEQUENCE DEPT_SEQ;

-- Create the sequence which Massive tests use
CREATE SEQUENCE DEPT_SEQ
MINVALUE 10
START WITH 50
INCREMENT BY 10;

-- Insert some new departments to allow reasonable paging tests
INSERT INTO DEPT (DEPTNO, DNAME, LOC)
SELECT SCOTT.DEPT_SEQ.nextval, 'DEPARTMENT#' || (level+4) AS DNAME, 'Somewhere' AS LOC
FROM DUAL
CONNECT BY LEVEL <= 56;

-- Change the location for 9 of the new departments
UPDATE DEPT
SET LOC = 'Nowhere'
WHERE DEPTNO >= 520;

COMMIT;

SELECT *
FROM DEPT;

SELECT *
FROM DEPT
WHERE LOC = 'Nowhere';