-- LAKS Database Schema (MySQL)
-- Run once on the target database to create tables and seed sample data.
-- Existing production data should NOT be replaced; this script uses
-- CREATE TABLE IF NOT EXISTS so it is idempotent.

CREATE TABLE IF NOT EXISTS species (
    id            INT          NOT NULL AUTO_INCREMENT PRIMARY KEY,
    name          VARCHAR(100) NOT NULL,
    norwegian_name VARCHAR(100)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS anglers (
    id      INT          NOT NULL AUTO_INCREMENT PRIMARY KEY,
    name    VARCHAR(100) NOT NULL,
    country VARCHAR(50)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS trips (
    id          INT          NOT NULL AUTO_INCREMENT PRIMARY KEY,
    year        YEAR         NOT NULL,
    start_date  DATE         NOT NULL,
    end_date    DATE         NOT NULL,
    river_name  VARCHAR(150) NOT NULL,
    location    VARCHAR(200) NOT NULL,
    description TEXT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS catches (
    id         INT            NOT NULL AUTO_INCREMENT PRIMARY KEY,
    trip_id    INT            NOT NULL,
    angler_id  INT            NOT NULL,
    species_id INT            NOT NULL,
    catch_date DATE           NOT NULL,
    weight_kg  DECIMAL(5,2)   NOT NULL DEFAULT 0.00,
    length_cm  DECIMAL(5,1)   NOT NULL DEFAULT 0.0,
    released   TINYINT(1)     NOT NULL DEFAULT 0,
    notes      TEXT,
    CONSTRAINT fk_catches_trip    FOREIGN KEY (trip_id)    REFERENCES trips   (id),
    CONSTRAINT fk_catches_angler  FOREIGN KEY (angler_id)  REFERENCES anglers (id),
    CONSTRAINT fk_catches_species FOREIGN KEY (species_id) REFERENCES species (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Indexes for common query patterns
CREATE INDEX IF NOT EXISTS idx_catches_trip_id    ON catches (trip_id);
CREATE INDEX IF NOT EXISTS idx_catches_angler_id  ON catches (angler_id);
CREATE INDEX IF NOT EXISTS idx_catches_catch_date ON catches (catch_date);

-- ---------------------------------------------------------------
-- Sample / seed data (skip if data already exists)
-- ---------------------------------------------------------------
INSERT IGNORE INTO species (id, name, norwegian_name) VALUES
    (1, 'Atlantic Salmon', 'Atlantisk laks'),
    (2, 'Sea Trout',       'Sjøørret'),
    (3, 'Arctic Char',     'Røye'),
    (4, 'Brown Trout',     'Ørret'),
    (5, 'Grayling',        'Harr');

INSERT IGNORE INTO anglers (id, name, country) VALUES
    (1, 'Erik Andersen',  'Norway'),
    (2, 'Lars Johansen',  'Norway'),
    (3, 'Ole Kristiansen','Norway'),
    (4, 'Bjørn Hansen',   'Norway');

INSERT IGNORE INTO trips (id, year, start_date, end_date, river_name, location, description) VALUES
    (1, 2020, '2020-06-15', '2020-06-22', 'Gaula',    'Støren, Trøndelag',    'First trip after pandemic restrictions eased.'),
    (2, 2021, '2021-06-14', '2021-06-21', 'Gaula',    'Støren, Trøndelag',    'Great conditions, high water levels.'),
    (3, 2022, '2022-06-13', '2022-06-20', 'Gaula',    'Støren, Trøndelag',    'Dry summer, low water.'),
    (4, 2023, '2023-06-12', '2023-06-19', 'Gaula',    'Støren, Trøndelag',    'Record season.'),
    (5, 2024, '2024-06-17', '2024-06-24', 'Gaula',    'Støren, Trøndelag',    'Good mix of salmon and sea trout.');
