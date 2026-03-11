-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: mysql71.unoeuro.com
-- Generation Time: Mar 11, 2026 at 08:08 PM
-- Server version: 8.4.7-7
-- PHP Version: 8.4.18

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `agileastronaut_com_db_laks`
--

-- --------------------------------------------------------

--
-- Table structure for table `Catch`
--

CREATE TABLE `Catch` (
  `Id` int NOT NULL,
  `PersonId` int NOT NULL,
  `Date` date NOT NULL,
  `Time` time NOT NULL,
  `Weight` decimal(10,2) NOT NULL,
  `Location` text NOT NULL,
  `Weather` text NOT NULL,
  `WaterLevel` decimal(5,3) DEFAULT NULL,
  `Bait` text NOT NULL,
  `Latitude` double NOT NULL,
  `Longitude` double NOT NULL,
  `Comment` text NOT NULL,
  `Type` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Triggers `Catch`
--
DELIMITER $$
CREATE TRIGGER `AddParticipant` AFTER INSERT ON `Catch` FOR EACH ROW BEGIN
   DECLARE vYear INT;
   DECLARE vPersonId INT;
   BEGIN
      SET vYear := YEAR(NEW.`Date`);
      SET vPersonId := NEW.`PersonId`;

      IF NOT EXISTS(SELECT * FROM `Participant` WHERE `Year` = vYear AND `PersonId` = vPersonId) THEN
   
         INSERT INTO `Participant` (`Year`, `PersonId`)
              VALUES (vYear, vPersonId);
   
      END IF;
   END;
END
$$
DELIMITER ;

-- --------------------------------------------------------

--
-- Table structure for table `Participant`
--

CREATE TABLE `Participant` (
  `Year` int NOT NULL,
  `PersonId` int NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `Person`
--

CREATE TABLE `Person` (
  `Id` int NOT NULL,
  `Name` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `WaterLevel`
--

CREATE TABLE `WaterLevel` (
  `MeasuredTime` datetime NOT NULL,
  `MeasuredLevel` decimal(5,3) NOT NULL,
  `InsertTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `WaterLevelUpdateAttempt`
--

CREATE TABLE `WaterLevelUpdateAttempt` (
  `AttemptTime` datetime NOT NULL,
  `Inserts` int NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `Catch`
--
ALTER TABLE `Catch`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `PersonId` (`PersonId`,`Date`,`Time`,`Weight`);

--
-- Indexes for table `Participant`
--
ALTER TABLE `Participant`
  ADD PRIMARY KEY (`Year`,`PersonId`);

--
-- Indexes for table `Person`
--
ALTER TABLE `Person`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `Name` (`Name`);

--
-- Indexes for table `WaterLevel`
--
ALTER TABLE `WaterLevel`
  ADD PRIMARY KEY (`MeasuredTime`);

--
-- Indexes for table `WaterLevelUpdateAttempt`
--
ALTER TABLE `WaterLevelUpdateAttempt`
  ADD PRIMARY KEY (`AttemptTime`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `Catch`
--
ALTER TABLE `Catch`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `Person`
--
ALTER TABLE `Person`
  MODIFY `Id` int NOT NULL AUTO_INCREMENT;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
