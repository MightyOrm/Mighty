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


-- Function: public.test_vars(integer)

-- DROP FUNCTION public.test_vars(integer);

CREATE OR REPLACE FUNCTION public.test_vars(
    OUT v integer,
    INOUT w integer,
    OUT x integer)
  RETURNS record AS
$BODY$
BEGIN
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

--

CREATE TABLE tab (y int);
INSERT INTO tab VALUES (1), (3), (5), (7);

CREATE FUNCTION sum_n_product_with_tab (x int, OUT sum int, OUT product int)
RETURNS SETOF record
AS $$
    SELECT $1 + tab.y, $1 * tab.y FROM tab;
$$ LANGUAGE SQL;

SELECT * FROM sum_n_product_with_tab(10);