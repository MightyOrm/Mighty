DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `mytest`()
BEGIN
DECLARE outvar INT;
CALL rewards_report_for_date(3, 20, '2005-5-1', outvar);
SELECT outvar;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `rewards_report_for_date`(
    IN min_monthly_purchases TINYINT UNSIGNED
    , IN min_dollar_amount_purchased DECIMAL(10,2) UNSIGNED
    , IN report_date DATE 
    , OUT count_rewardees INT
)
    READS SQL DATA
    COMMENT 'Provides a customizable report on best customers'
proc: BEGIN

    DECLARE last_month_start DATE;
    DECLARE last_month_end DATE;

    /* Some sanity checks... */
    IF min_monthly_purchases = 0 THEN
        SELECT 'Minimum monthly purchases parameter must be > 0';
        LEAVE proc;
    END IF;
    IF min_dollar_amount_purchased = 0.00 THEN
        SELECT 'Minimum monthly dollar amount purchased parameter must be > $0.00';
        LEAVE proc;
    END IF;

    /* Determine start and end time periods */
    SET last_month_start = report_date; #DATE_SUB(CURRENT_DATE(), INTERVAL 1 MONTH);
    SET last_month_start = STR_TO_DATE(CONCAT(YEAR(last_month_start),'-',MONTH(last_month_start),'-01'),'%Y-%m-%d');
    SET last_month_end = LAST_DAY(last_month_start);

    /*
        Create a temporary storage area for
        Customer IDs.
    */
    CREATE TEMPORARY TABLE tmpCustomer (customer_id SMALLINT UNSIGNED NOT NULL PRIMARY KEY);

    /*
        Find all customers meeting the
        monthly purchase requirements
    */
    INSERT INTO tmpCustomer (customer_id)
    SELECT p.customer_id
    FROM payment AS p
    WHERE DATE(p.payment_date) BETWEEN last_month_start AND last_month_end
    GROUP BY customer_id
    HAVING SUM(p.amount) > min_dollar_amount_purchased
    AND COUNT(customer_id) > min_monthly_purchases;

    /* Populate OUT parameter with count of found customers */
    SELECT COUNT(*) FROM tmpCustomer INTO count_rewardees;

    /*
        Output ALL customer information of matching rewardees.
        Customize output as needed.
    */
    SELECT c.*
    FROM tmpCustomer AS t
    INNER JOIN customer AS c ON t.customer_id = c.customer_id;

    /* Clean up */
    DROP TABLE tmpCustomer;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `testproc_in_out`(IN param1 INTEGER(11), OUT param2 INTEGER(11))
BEGIN
  SET param2=param1*2;
  END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `testproc_inout`(INOUT param1 INTEGER(11))
BEGIN
  SET param1=param1*2;
  END$$
DELIMITER ;
