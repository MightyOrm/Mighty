IF OBJECT_ID ( 'dbo.uspCurrencyCursor', 'P' ) IS NOT NULL
    DROP PROCEDURE dbo.uspCurrencyCursor;
GO
CREATE PROCEDURE dbo.uspCurrencyCursor 
    @CurrencyCursor CURSOR VARYING OUTPUT
AS
    SET NOCOUNT ON;
    SET @CurrencyCursor = CURSOR
    FORWARD_ONLY STATIC FOR
      SELECT CurrencyCode, Name
      FROM Sales.Currency;
    OPEN @CurrencyCursor;
GO

IF OBJECT_ID ( 'dbo.uspCurrencySelect', 'P' ) IS NOT NULL
    DROP PROCEDURE dbo.uspCurrencySelect;
GO
CREATE PROCEDURE dbo.uspCurrencySelect 
AS
    SET NOCOUNT ON;

    SELECT CurrencyCode, Name
    FROM Sales.Currency;
GO

IF OBJECT_ID ( 'dbo.TestVars', 'P' ) IS NOT NULL
    DROP PROCEDURE dbo.TestVars;
GO
CREATE PROCEDURE dbo.TestVars
	@v INT = NULL OUTPUT,
	@w INT = NULL OUTPUT,
	@x INT = NULL OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET @w = ISNULL(@w, 0) + 2
	SET @v = @w - 1
	SET @x = @w + 1
END
GO
