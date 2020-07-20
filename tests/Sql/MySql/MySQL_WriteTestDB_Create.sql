-- ----------------------------------------------------------------------------------------------------------------
-- Schema 'MassiveWriteTests'
-- ----------------------------------------------------------------------------------------------------------------

DROP SCHEMA IF EXISTS `MassiveWriteTests`;
CREATE SCHEMA `MassiveWriteTests`;
USE `MassiveWriteTests`;

-- -------` Tables `-----------------------------------------------------------------------------------------------

CREATE TABLE `Categories` 
(
    `CategoryID` int unsigned NOT NULL AUTO_INCREMENT, 
    `CategoryName` varchar(15) NOT NULL, 
    `Description` text NULL, 
    `Picture` blob NULL,
    PRIMARY KEY (`CategoryID`)
);

CREATE TABLE `Products` 
(
    `ProductID` int unsigned NOT NULL AUTO_INCREMENT, 
    `ProductName` varchar(40) NOT NULL, 
    `CategoryID` int unsigned NULL, 
    `QuantityPerUnit` varchar(20) NULL, 
    `UnitPrice` decimal(13,2) NULL DEFAULT 0, 
    `UnitsInStock` smallint NULL DEFAULT 0, 
    `UnitsOnOrder` smallint NULL DEFAULT 0, 
    `ReorderLevel` smallint NULL DEFAULT 0, 
    `Discontinued` bit NOT NULL DEFAULT 0,
    PRIMARY KEY (`ProductID`),
    CONSTRAINT `FK_Products_Categories` FOREIGN KEY (`CategoryID`) REFERENCES `Categories` (`CategoryID`)
);

DELIMITER //
CREATE PROCEDURE `pr_clearAll`()
BEGIN
    DELETE FROM Products;
    DELETE FROM Categories;
END //
DELIMITER ;

CREATE TABLE `bittest` (
  `id` smallint(5) unsigned NOT NULL AUTO_INCREMENT,
  `tinyint_one` tinyint(1) NOT NULL,
  `tinyint_three` tinyint(3) unsigned NOT NULL,
  `tinyint_bool` bool NOT NULL,
  `bit_one` bit(1) NOT NULL,
  `bit_two` bit(2) NOT NULL,
  `bit_eight` bit(8) NOT NULL,
  `bit_sixtyfour` bit(64) NOT NULL,
  PRIMARY KEY (`id`)
);

INSERT INTO `bittest` (
  `tinyint_one`,
  `tinyint_three`,
  `tinyint_bool`,
  `bit_one`,
  `bit_two`,
  `bit_eight`,
  `bit_sixtyfour`
)
VALUES (0,0,0,0,0,0,0),
(1,1,1,1,1,1,1),
(2,2,2,0,2,2,2);
