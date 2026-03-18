CREATE TABLE IF NOT EXISTS `season_config` (
  `Year` INT NOT NULL,
  `GroupNumber` INT NOT NULL,
  `StartDate` DATE NOT NULL,
  `EndDate` DATE NOT NULL,
  PRIMARY KEY (`Year`, `GroupNumber`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
