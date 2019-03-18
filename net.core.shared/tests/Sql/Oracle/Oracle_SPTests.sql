-- Types
create or replace type       EMP_TAB_TYPE is object
("EMPNO" NUMBER(4,0), 
	"ENAME" VARCHAR2(10 BYTE), 
	"JOB" VARCHAR2(9 BYTE), 
	"MGR" NUMBER(4,0), 
	"HIREDATE" DATE, 
	"SAL" NUMBER(7,2), 
	"COMM" NUMBER(7,2), 
	"DEPTNO" NUMBER(2,0));
  
create or replace type       EMP_TAB_TYPE_COLL is table of EMP_TAB_TYPE;

create or replace type my_tab_type is object
(prodid number, a varchar2(1), b varchar2(1), 
  c varchar2(1), d varchar2(1), e varchar2(1));
  
create or replace type my_tab_type_coll is table of my_tab_type;

-- Procedures
create or replace PROCEDURE findMin(x IN number, y IN number, z OUT number) IS
BEGIN
   IF x < y THEN
      z:= x;
   ELSE
      z:= y;
   END IF;
END;

create or replace procedure mixedresults (prc1 out sys_refcursor, prc2 out sys_refcursor, num1 out number, num2 out number)
is
begin
   num1 := 1;
   open prc1 for select * from emp;
   open prc2 for select * from dept;
   num2 := 2;
end;

create or replace procedure myproc (prc out sys_refcursor)
is
begin
   open prc for select * from emp;
end;

create or replace PROCEDURE squareNum(x IN OUT number) IS
BEGIN
  x := x * x;
END;

create or replace procedure tworesults (prc1 out sys_refcursor, prc2 out sys_refcursor)
is
begin
   open prc1 for select * from emp;
   open prc2 for select * from dept;
end;

-- Functions
create or replace FUNCTION findMax(x IN number, y IN number) 
RETURN number
IS
    z number;
BEGIN
   IF x > y THEN
      z:= x;
   ELSE
      Z:= y;
   END IF;

   RETURN z;
END;

create or replace function get_dept_emps(p_deptno in number) return sys_refcursor is
      v_rc sys_refcursor;
    begin
      open v_rc for 'select empno, ename, mgr, sal from emp where deptno = :deptno' using p_deptno;
      return v_rc;
    end;
	
create or replace function       GET_EMP (p_EMPNO in number) 
return SCOTT.EMP_TAB_TYPE_COLL pipelined is
begin
  FOR i in (select * from SCOTT.EMP where EMPNO=p_EMPNO) loop
    pipe row(SCOTT.EMP_TAB_TYPE(i."EMPNO", 
	i."ENAME", 
	i."JOB", 
	i."MGR", 
	i."HIREDATE", 
	i."SAL", 
	i."COMM", 
	i."DEPTNO"));
  end loop;
  return;
end;

create or replace function get_some_data (p_val in number) 
return my_tab_type_coll pipelined is
begin
  FOR i in (select * from my_table where prodid=p_val) loop
    pipe row(my_tab_type(i.prodid,i.a,i.b,i.c,i.d,i.e));
  end loop;
  return;
end;

create or replace function pipe_back_emp_cursor (prc in sys_refcursor) 
return emp_tab_type_coll pipelined is
  l_emp emp_tab_type;
begin
  loop
    fetch prc into l_emp;
    exit when prc%notfound;
    --pipe row(l_emp);
  end loop;
  return;
end;

-- Oracle cursor_in_out demo code, slightly modified to access SCOTT.EMP instead of HR.EMPLOYEE:
-- https://blogs.oracle.com/oraclemagazine/cursor-in-cursor-out
create table processing_result
(
  status varchar2(64)
);

create or replace package cursor_in_out as
  type emp_cur_type is ref cursor return emp%rowtype;

  procedure process_cursor(p_cursor in emp_cur_type);
end;
/

create or replace package body cursor_in_out as
  procedure process_cursor(p_cursor in emp_cur_type) is
    employee emp%rowtype;
  begin
    loop
      fetch p_cursor into employee;
      exit when p_cursor%notfound;
      insert into processing_result
        values('Processed employee #' ||
               employee.empno || ': ' ||
               employee.ename);
    end loop;
  end;
end;
/
