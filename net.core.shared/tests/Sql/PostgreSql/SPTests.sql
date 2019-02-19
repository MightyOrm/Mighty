-- Table: public.tab

DROP TABLE public.tab;

CREATE TABLE public.tab
(
  y integer
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public.tab
  OWNER TO postgres;

INSERT INTO tab VALUES (1);
INSERT INTO tab VALUES (3);
INSERT INTO tab VALUES (5);
INSERT INTO tab VALUES (7);

-- Table: public.large

DROP TABLE public.large;

-- Insert values 1-1000000 into public.large
SELECT * INTO public.large FROM generate_series(1, 1000000) AS id;

-- Function: public.add_em(integer, integer)

-- DROP FUNCTION public.add_em(integer, integer);

CREATE OR REPLACE FUNCTION public.add_em(
    integer,
    integer)
  RETURNS integer AS
$BODY$
    SELECT $1 + $2;
$BODY$
  LANGUAGE sql VOLATILE
  COST 100;
ALTER FUNCTION public.add_em(integer, integer)
  OWNER TO postgres;

-- Function: public.cbreaktest()

-- DROP FUNCTION public.cbreaktest();

CREATE OR REPLACE FUNCTION public.cbreaktest()
  RETURNS SETOF refcursor AS
$BODY$
DECLARE ref1 refcursor; ref2 refcursor;
BEGIN
OPEN ref1 FOR SELECT * FROM generate_series(1, 10) a;
RETURN NEXT ref1;
OPEN ref2 FOR SELECT * FROM generate_series(100, 110) c;
RETURN next ref2;
RETURN;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION public.cbreaktest()
  OWNER TO postgres;

-- Function: public.ctest()

-- DROP FUNCTION public.ctest();

CREATE OR REPLACE FUNCTION public.ctest()
  RETURNS refcursor AS
$BODY$DECLARE
   c CURSOR FOR SELECT y FROM tab;
BEGIN
   c := 'c"';
   OPEN c;
   RETURN c;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION public.ctest()
  OWNER TO postgres;

-- Function: public.cursor_employees()

-- DROP FUNCTION public.cursor_employees();

CREATE OR REPLACE FUNCTION public.cursor_employees()
  RETURNS refcursor AS
$BODY$
    DECLARE
      ref refcursor;                                                     -- Declare a cursor variable
    BEGIN
      OPEN ref FOR SELECT * FROM employees;   -- Open a cursor
      RETURN ref;                                                       -- Return the cursor to the caller
    END;
    $BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION public.cursor_employees()
  OWNER TO postgres;

-- Function: public.cursor_mix()

-- DROP FUNCTION public.cursor_mix();

CREATE OR REPLACE FUNCTION public.cursor_mix(
    OUT mycursor1 refcursor,
    OUT myvalue integer)
  RETURNS SETOF record AS
$BODY$
DECLARE ref1 refcursor;
BEGIN
    OPEN ref1 FOR SELECT 11 as a, 22 as b;
    
    RETURN QUERY
    SELECT ref1, 42;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION public.cursor_mix()
  OWNER TO postgres;

-- Function: public.cursornbyone()

-- DROP FUNCTION public.cursornbyone();

CREATE OR REPLACE FUNCTION public.cursornbyone(
    OUT mycursor1 refcursor,
    OUT mycursor2 refcursor)
  RETURNS SETOF record AS
$BODY$
DECLARE ref1 refcursor; ref2 refcursor;
BEGIN
    OPEN ref1 FOR SELECT 11 as a, 22 as b;
    OPEN ref2 FOR SELECT 33 as c, 44 as d;
    
    RETURN QUERY
    SELECT ref1, ref2;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION public.cursornbyone()
  OWNER TO postgres;

-- Function: public.cursoronebyn()

-- DROP FUNCTION public.cursoronebyn();

CREATE OR REPLACE FUNCTION public.cursoronebyn()
  RETURNS SETOF refcursor AS
$BODY$
DECLARE ref1 refcursor; ref2 refcursor;
BEGIN
OPEN ref1 FOR SELECT 1 AS a, 2 AS b;
RETURN NEXT ref1;
OPEN ref2 FOR SELECT 3 AS c, 4 AS d;
RETURN next ref2;
RETURN;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION public.cursoronebyn()
  OWNER TO postgres;

-- Function: public.fetch_next_ints_from_cursor(refcursor)

-- DROP FUNCTION public.fetch_next_ints_from_cursor(refcursor);

CREATE OR REPLACE FUNCTION public.fetch_next_ints_from_cursor(
    IN mycursor refcursor,
    OUT myint1 integer,
    OUT myint2 integer)
  RETURNS SETOF record AS
$BODY$
DECLARE
    a INT;
    b INT;
BEGIN
    FETCH NEXT FROM mycursor INTO a, b;
    RETURN QUERY SELECT a, b;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION public.fetch_next_ints_from_cursor(refcursor)
  OWNER TO postgres;

-- Function: public.find_max(integer, integer)

-- DROP FUNCTION public.find_max(integer, integer);

CREATE OR REPLACE FUNCTION public.find_max(
    x integer,
    y integer)
  RETURNS integer AS
$BODY$
BEGIN
IF x < y THEN
	RETURN y;
ELSE
	RETURN x;
END IF;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION public.find_max(integer, integer)
  OWNER TO postgres;

-- Function: public.find_min(integer, integer)

-- DROP FUNCTION public.find_min(integer, integer);

CREATE OR REPLACE FUNCTION public.find_min(
    IN x integer,
    IN y integer,
    OUT z integer)
  RETURNS integer AS
$BODY$
BEGIN
IF x < y THEN
	z := x;
ELSE
	z := y;
END IF;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION public.find_min(integer, integer)
  OWNER TO postgres;

-- Function: public.get_date()

-- DROP FUNCTION public.get_date();

CREATE OR REPLACE FUNCTION public.get_date()
  RETURNS timestamp without time zone AS
$BODY$
BEGIN
RETURN CURRENT_TIMESTAMP;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION public.get_date()
  OWNER TO postgres;

-- Function: public.lump()

-- DROP FUNCTION public.lump();

CREATE OR REPLACE FUNCTION public.lump()
  RETURNS refcursor AS
$BODY$DECLARE
   -- sorting here creates a large PostgreSQL server-side buffer
   c CURSOR FOR SELECT id FROM large;
BEGIN
   c := 'c';
   OPEN c;
   RETURN c;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION public.lump()
  OWNER TO postgres;

-- Function: public.lump2()

-- DROP FUNCTION public.lump2();

CREATE OR REPLACE FUNCTION public.lump2(
    OUT c refcursor,
    OUT d refcursor)
  RETURNS SETOF record AS
$BODY$
DECLARE ref1 refcursor; ref2 refcursor;
BEGIN
    -- sorting here creates large PostgreSQL server-side buffers
    OPEN ref1 FOR SELECT id FROM large;
    OPEN ref2 FOR SELECT id FROM large;
    
    RETURN QUERY
    SELECT ref1, ref2;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION public.lump2()
  OWNER TO postgres;

-- Function: public.square_num(integer)

-- DROP FUNCTION public.square_num(integer);

CREATE OR REPLACE FUNCTION public.square_num(INOUT x integer)
  RETURNS integer AS
$BODY$
BEGIN
	x := x * x;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION public.square_num(integer)
  OWNER TO postgres;

-- Function: public.sum_n_product_with_tab(integer)

-- DROP FUNCTION public.sum_n_product_with_tab(integer);

CREATE OR REPLACE FUNCTION public.sum_n_product_with_tab(
    IN x integer,
    OUT sum integer,
    OUT product integer)
  RETURNS SETOF record AS
$BODY$
    SELECT $1 + tab.y, $1 * tab.y FROM tab;
$BODY$
  LANGUAGE sql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION public.sum_n_product_with_tab(integer)
  OWNER TO postgres;

-- Function: public.test_vars(integer)

-- DROP FUNCTION public.test_vars(integer);

CREATE OR REPLACE FUNCTION public.test_vars(
    OUT v integer,
    INOUT w integer,
    OUT x integer)
  RETURNS record AS
$BODY$
BEGIN
	w := COALESCE(w, 0);
	v := w + 1;
	w := w + 2;
	x := w + 1;
	--return w + 2;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION public.test_vars(integer)
  OWNER TO postgres;
