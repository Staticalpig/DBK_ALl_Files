DROP DATABASE IF EXISTS c24elipe;
CREATE DATABASE IF NOT EXISTS c24elipe;
USE c24elipe;


-- Table creation
CREATE TABLE IF NOT EXISTS Agent_Secret
(
    Agent_ID       INT PRIMARY KEY AUTO_INCREMENT,
    Fornamn        VARCHAR(50)    NOT NULL,
    Efternamn      VARCHAR(50)    NOT NULL,
    HelaNamnet     VARCHAR(101) GENERATED ALWAYS AS (CONCAT(Fornamn, ' ', Efternamn)) VIRTUAL,
    Lön            DECIMAL(10, 2) NOT NULL DEFAULT 1300.00,
    kontoActiverat BOOLEAN        NOT NULL DEFAULT TRUE,
    CONSTRAINT chk_lön CHECK ( Lön >= 1200.00 )
);

CREATE TABLE IF NOT EXISTS Agent_Open
(
    Agent_ID     INT PRIMARY KEY,
    Användarnamn VARCHAR(10)  NOT NULL UNIQUE,
    Avdelning    VARCHAR(50)  NOT NULL,
    Lösenord     VARCHAR(255) NOT NULL DEFAULT '$2y$10$WFPjpxE3JSolzpKjT9uueON1qDSH8Cbsvz751bXjrMeTOHMaG3HuS', -- Default password: 'PASSWORD' (hashed using bcrypt)
    CONSTRAINT fk_agent_open_agent_secret FOREIGN KEY (Agent_ID) REFERENCES Agent_Secret (Agent_ID)
);

CREATE TABLE IF NOT EXISTS Incident
(
    Namn             VARCHAR(100) NOT NULL,
    NR               INT UNSIGNED NOT NULL,
    Säkerhetsgrad    ENUM (
        'Oproblematisk', 'Låg', 'Mellan', 'Hög', 'Mycket hög', 'Kritisk'
        )                         NOT NULL DEFAULT 'Oproblematisk',
    AntalInvolverade INT UNSIGNED NOT NULL DEFAULT 0,


    CONSTRAINT pk_incident PRIMARY KEY (Namn, NR)
);

CREATE TABLE IF NOT EXISTS Ras
(
    RasNamn     VARCHAR(50) PRIMARY KEY,
    Beskrivning TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Alien
(
    IdKod         CHAR(25) PRIMARY KEY,
    PNR           CHAR(12)    NOT NULL UNIQUE,
    Ras           VARCHAR(50) NOT NULL DEFAULT 'Okänd',
    Hemplanet     VARCHAR(50) NOT NULL,
    KändaNamn     TEXT        NOT NULL,
    ÄrRegistrerad BOOLEAN     NOT NULL DEFAULT FALSE,
    Farlighet     ENUM (
        'Harmlös', 'Halvt harmlös', 'Ofarlig', 'Neutral', 'Svagt farlig', 'Farlig', 'Extremt farlig', 'Spring för livet'
        )                     NOT NULL DEFAULT 'Neutral',
    CONSTRAINT chk_alien_pnr CHECK (PNR REGEXP '^[0-9]{6}-[0-9]{4}[A-Z]$'),
    CONSTRAINT fk_alien_ras FOREIGN KEY (Ras) REFERENCES Ras (RasNamn) ON DELETE NO ACTION
);

CREATE TABLE IF NOT EXISTS Alien_Arkiv
(
    IdKod          CHAR(25)     NOT NULL,
    PNR            CHAR(12)     NOT NULL,
    Ras            VARCHAR(50)  NOT NULL,
    Hemplanet      VARCHAR(50)  NOT NULL,
    KändaNamn      TEXT         NOT NULL,
    ÄrRegistrerad  BOOLEAN      NOT NULL,
    Farlighet      ENUM (
        'Harmlös', 'Halvt harmlös', 'Ofarlig', 'Neutral', 'Svagt farlig', 'Farlig', 'Extremt farlig', 'Spring för livet'
        )                       NOT NULL,
    ArkivDatum     DATE         NOT NULL,
    ArkivAnledning VARCHAR(255) NOT NULL,
    ArkivAv        INT          NULL,
    isDeleted      BOOLEAN      NOT NULL DEFAULT FALSE,
    CONSTRAINT pk_alien_arkiv PRIMARY KEY (IdKod, ArkivDatum),
    CONSTRAINT fk_alien_arkiv_agent FOREIGN KEY (ArkivAv) REFERENCES Agent_Open (Agent_ID) ON DELETE SET NULL,
    CONSTRAINT fk_alien_arkiv_ras FOREIGN KEY (Ras) REFERENCES Ras (RasNamn) ON DELETE NO ACTION
);

CREATE TABLE IF NOT EXISTS Incident_Alien
(
    Incident_Namn VARCHAR(100) NOT NULL,
    Incident_NR   INT UNSIGNED NOT NULL,
    Alien_IdKod   CHAR(25)     NOT NULL,
    CONSTRAINT pk_incident_alien PRIMARY KEY (Incident_Namn, Incident_NR, Alien_IdKod),
    CONSTRAINT fk_incident_alien_incident FOREIGN KEY (Incident_Namn, Incident_NR) REFERENCES Incident (Namn, NR) ON DELETE CASCADE,
    CONSTRAINT fk_incident_alien_alien FOREIGN KEY (Alien_IdKod) REFERENCES Alien (IdKod) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Incident_Alien_Arkiv
(
    Incident_Namn VARCHAR(100) NOT NULL,
    Incident_NR   INT UNSIGNED NOT NULL,
    Alien_IdKod   CHAR(25)     NOT NULL,

    CONSTRAINT pk_incident_alien_arkiv PRIMARY KEY (Incident_Namn, Incident_NR, Alien_IdKod),
    CONSTRAINT fk_incident_alien_arkiv_incident FOREIGN KEY (Incident_Namn, Incident_NR) REFERENCES Incident (Namn, NR) ON DELETE CASCADE,
    CONSTRAINT fk_incident_alien_arkiv_alien FOREIGN KEY (Alien_IdKod) REFERENCES Alien_Arkiv (IdKod) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Rapport
(
    Datum          DATE         NOT NULL,
    Nr             INT UNSIGNED NOT NULL,
    RapportTyp     VARCHAR(25)  NOT NULL,
    Agent_ID       INT          NOT NULL,
    Användarnamn   VARCHAR(10)  NOT NULL,
    Incidentledare VARCHAR(10)  NOT NULL,
    Slutdatum      DATE         NOT NULL,

    Incident_Namn  VARCHAR(100) NOT NULL,
    Incident_NR    INT UNSIGNED NOT NULL,
    CONSTRAINT pk_rapport PRIMARY KEY (Datum, Nr),
    CONSTRAINT fk_rapport_agent FOREIGN KEY (Agent_ID) REFERENCES Agent_Open (Agent_ID),
    CONSTRAINT fk_rapport_incident FOREIGN KEY (Incident_Namn, Incident_NR) REFERENCES Incident (Namn, NR) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS Rapport_Arkiv
(
    Datum          DATE         NOT NULL,
    Nr             INT UNSIGNED NOT NULL,
    RapportTyp     VARCHAR(25)  NOT NULL,
    Agent_ID       INT          NOT NULL,
    Användarnamn   VARCHAR(10)  NOT NULL,
    Incidentledare VARCHAR(10)  NOT NULL,
    Slutdatum      DATE         NOT NULL,
    Incident_Namn  VARCHAR(100) NOT NULL,
    Incident_NR    INT UNSIGNED NOT NULL,
    ArkivDatum     DATE         NOT NULL,
    ArkivAnledning VARCHAR(255) NOT NULL,
    ArkivAv        INT          NULL,
    CONSTRAINT pk_rapport_arkiv PRIMARY KEY (Datum, Nr, ArkivDatum),
    CONSTRAINT fk_rapport_arkiv_agent FOREIGN KEY (Agent_ID) REFERENCES Agent_Open (Agent_ID),
    CONSTRAINT fk_rapport_arkiv_incident FOREIGN KEY (Incident_Namn, Incident_NR) REFERENCES Incident (Namn, NR) ON DELETE RESTRICT,
    CONSTRAINT fk_rapport_arkiv_arkivav FOREIGN KEY (ArkivAv) REFERENCES Agent_Open (Agent_ID) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS Rapport_Rader
(
    Rapport_Datum DATE         NOT NULL,
    Rapport_Nr    INT UNSIGNED NOT NULL,
    nr            INT UNSIGNED NOT NULL,
    Text          VARCHAR(500) NOT NULL,
    CONSTRAINT pk_rapport_stycken PRIMARY KEY (Rapport_Datum, Rapport_Nr, nr),
    CONSTRAINT fk_rapport_stycken_rapport FOREIGN KEY (Rapport_Datum, Rapport_Nr) REFERENCES Rapport (Datum, Nr) ON DELETE RESTRICT
);


CREATE TABLE IF NOT EXISTS Rapport_Rader_Arkiv
(
    Rapport_Datum DATE         NOT NULL ,
    Rapport_Nr    INT UNSIGNED NOT NULL ,
    nr            INT UNSIGNED NOT NULL,
    Text          VARCHAR(500) NOT NULL,
    ArkivDatum    DATE         NOT NULL,
    ArkivAv       INT          NULL,
    CONSTRAINT pk_rapport_stycken_arkiv PRIMARY KEY (Rapport_Datum, Rapport_Nr, nr, ArkivDatum),
    CONSTRAINT fk_rapport_stycken_arkiv_rapport FOREIGN KEY (Rapport_Datum, Rapport_Nr, ArkivDatum) REFERENCES Rapport_Arkiv (Datum, Nr, ArkivDatum),
    CONSTRAINT fk_rapport_stycken_arkiv_agent FOREIGN KEY (ArkivAv) REFERENCES Agent_Open (Agent_ID) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS Rapport_Kommentar
(
    Rapport_Datum      DATE         NOT NULL,
    Rapport_Nr         INT UNSIGNED NOT NULL,
    nr                 INT UNSIGNED NOT NULL,

    Text               VARCHAR(500) NOT NULL,

    GjordAv            INT          NULL DEFAULT 0,
    GjordDatum         TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,

    INDEX idx_kommentar_nr (nr) USING BTREE,
    CONSTRAINT pk_rapport_kommentar PRIMARY KEY (Rapport_Datum, Rapport_Nr, nr),
    CONSTRAINT fk_komentar_rapport FOREIGN KEY (Rapport_Datum, Rapport_Nr) REFERENCES Rapport (Datum, Nr) ON DELETE RESTRICT

);

CREATE TABLE IF NOT EXISTS Rapport_Kommentar_Arkiv
(
    Rapport_Datum      DATE         NOT NULL,
    Rapport_Nr         INT UNSIGNED NOT NULL,
    nr                 INT UNSIGNED NOT NULL,
    Text               VARCHAR(500) NOT NULL,

    GjordAv            INT          NULL DEFAULT 0,
    GjordDatum         TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,

    ArkivDatum         DATE         NOT NULL,
    ArkivAv            INT          NULL,


    INDEX idx_kommentar_arkiv_nr (nr) USING BTREE,
    CONSTRAINT pk_rapport_kommentar_arkiv PRIMARY KEY (Rapport_Datum, Rapport_Nr, nr, ArkivDatum),
    CONSTRAINT fk_komentar_rapport_arkiv FOREIGN KEY (Rapport_Datum, Rapport_Nr, ArkivDatum) REFERENCES Rapport_Arkiv (Datum, Nr, ArkivDatum) ON DELETE RESTRICT
);


-- Index creation TODO: TA BORT?
CREATE INDEX idx_alien_ras ON Alien (Ras) USING BTREE;
CREATE INDEX idx_incident_säkerhetsgrad ON Incident (Säkerhetsgrad) USING BTREE;
CREATE INDEX idx_rapport_incident ON Rapport (Incident_Namn, Incident_NR) USING BTREE;
CREATE INDEX idx_rapport_agent ON Rapport (Agent_ID) USING BTREE;
CREATE INDEX idx_rapport_arkiv_agent ON Rapport_Arkiv (ArkivAv) USING BTREE;
CREATE INDEX idx_rapport_stycken_rapport ON Rapport_Rader (Rapport_Datum, Rapport_Nr) USING BTREE;


CREATE OR REPLACE VIEW vw_Agent_Full AS
SELECT o.Användarnamn,
       s.Fornamn,
       s.Lön,
       o.Avdelning,
       o.Lösenord
FROM Agent_Secret s
         JOIN
     Agent_Open o ON s.Agent_ID = o.Agent_ID;

CREATE OR REPLACE VIEW vw_get_full_rapport
AS
SELECT * FROM Rapport;

CREATE OR REPLACE VIEW wv_get_agent_login AS
SELECT o.Användarnamn,
       o.Lösenord
FROM Agent_Open o
         JOIN Agent_Secret s ON o.Agent_ID = s.Agent_ID
WHERE s.kontoActiverat = TRUE;


CREATE OR REPLACE VIEW wv_Alien_Ras_Full AS
SELECT a.PNR,
       a.Farlighet,
       a.KändaNamn as Alias,
       a.Hemplanet,
       r.RasNamn,
       r.Beskrivning,
       a.ÄrRegistrerad

FROM Alien as a
         JOIN
     Ras as r on a.Ras = r.RasNamn;


CREATE OR REPLACE VIEW wv_Alien_Incident AS
SELECT ia.Incident_Namn,
       ia.Incident_NR,
       i.Säkerhetsgrad,
       ia.Alien_IdKod,
       a.PNR,
       a.Ras,
       a.Hemplanet,
       a.KändaNamn,
       a.ÄrRegistrerad,
       a.Farlighet

FROM Incident_Alien as ia
         JOIN Incident as i on ia.Incident_Namn = i.Namn
    AND ia.Incident_NR = i.NR
         JOIN Alien as a on ia.Alien_IdKod = a.IdKod;

CREATE OR REPLACE VIEW wv_Rapport_Kommentar_Full AS
SELECT rk.nr, rk.Text, ao.Användarnamn as GjordAv, rk.Rapport_Datum, rk.Rapport_Nr, r.Incidentledare
FROM Rapport_Kommentar as rk
         LEFT JOIN Agent_Open as ao on rk.GjordAv = ao.Agent_ID
         LEFT JOIN Rapport r on rk.Rapport_Datum = r.Datum AND rk.Rapport_Nr = r.Nr;




DELIMITER @@;

DROP PROCEDURE IF EXISTS sp_archive_old_rapport;
CREATE PROCEDURE IF NOT EXISTS sp_archive_old_rapport()
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
        BEGIN
            ROLLBACK;
            RESIGNAL;
        END;

    START TRANSACTION;


    INSERT INTO Rapport_Arkiv(Datum, Nr, RapportTyp, Agent_ID, Användarnamn, Incidentledare, Slutdatum, Incident_Namn, Incident_NR, ArkivDatum, ArkivAnledning, ArkivAv)
    SELECT
        r.Datum, r.Nr, r.RapportTyp, r.Agent_ID, r.Användarnamn, r.Incidentledare, r.Slutdatum, r.Incident_Namn, r.Incident_NR,
        current_date, 'Automatisk arkivering av rapport äldre än 5 år', 1
    from Rapport as r
    WHERE r.Slutdatum < (CURRENT_DATE - INTERVAL 5 YEAR);


    INSERT INTO Rapport_Rader_Arkiv(Rapport_Datum, Rapport_Nr, nr, Text, ArkivDatum, ArkivAv)
    select rapr.Rapport_Datum, rapr.Rapport_Nr, rapr.nr, rapr.Text, current_date, 1
    FROM Rapport_Rader as rapr
    WHERE (Rapport_Datum, Rapport_Nr) IN (
        SELECT r.Datum, r.Nr
        FROM Rapport as r
        WHERE r.Slutdatum < (CURRENT_DATE - INTERVAL 5 YEAR)
    );

    -- Archive report comments (child records) - must have parent archived first
    INSERT INTO Rapport_Kommentar_Arkiv(Rapport_Datum, Rapport_Nr, nr, Text, GjordAv, GjordDatum, ArkivDatum, ArkivAv)
    SELECT rk.Rapport_Datum, rk.Rapport_Nr, rk.nr, rk.Text, rk.GjordAv, rk.GjordDatum, CURRENT_DATE, 1
    FROM Rapport_Kommentar as rk
             JOIN Rapport as r on rk.Rapport_Datum = r.Datum AND rk.Rapport_Nr = r.Nr
    WHERE r.Slutdatum < (CURRENT_DATE - INTERVAL 5 YEAR);


    -- Delete comments
    DELETE rk
    FROM Rapport_Kommentar as rk
             JOIN Rapport as r on rk.Rapport_Datum = r.Datum AND rk.Rapport_Nr = r.Nr
    WHERE r.Slutdatum < (CURRENT_DATE - INTERVAL 5 YEAR);

    -- Delete report rows
    DELETE rr
    FROM Rapport_Rader as rr
             JOIN Rapport as r on rr.Rapport_Datum = r.Datum AND rr.Rapport_Nr = r.Nr
    WHERE r.Slutdatum < (CURRENT_DATE - INTERVAL 5 YEAR);

    -- Delete reports last (parent records)
    DELETE FROM Rapport
    WHERE Slutdatum < (CURRENT_DATE - INTERVAL 5 YEAR);

    COMMIT;
END @@;



CREATE PROCEDURE IF NOT EXISTS sp_add_Rapport_rad(
    IN p_Rapport_Datum DATE,
    IN p_Rapport_Nr INT UNSIGNED,
    IN p_Text VARCHAR(500)
)
BEGIN
    DECLARE v_new_nr INT UNSIGNED;
    SET v_new_nr = COALESCE(
                           (SELECT MAX(rr.nr)
                            FROM Rapport_Rader as rr
                            WHERE rr.Rapport_Datum = p_Rapport_Datum
                              AND rr.Rapport_Nr = p_Rapport_Nr), 0) + 1;

    INSERT INTO Rapport_Rader (Rapport_Datum, Rapport_Nr, nr, Text)
    VALUES (p_Rapport_Datum, p_Rapport_Nr, v_new_nr, p_Text);

end @@;

CREATE PROCEDURE IF NOT EXISTS sp_get_agent_login(
    IN p_Användarnamn VARCHAR(10)
)
BEGIN
    SELECT wv.Lösenord
    FROM wv_get_agent_login as wv
    WHERE Användarnamn = p_Användarnamn;
end @@;

CREATE PROCEDURE IF NOT EXISTS sp_give_info_to_login(
    IN p_Användarnamn VARCHAR(10)
)
BEGIN
    SELECT wv.Fornamn   as Fornamn,
           wv.Avdelning as Avdelning
    FROM vw_Agent_Full as wv
    WHERE Användarnamn = p_Användarnamn;
end @@;

CREATE PROCEDURE IF NOT EXISTS sp_add_agent(
    IN p_kodnamn char(3),
    IN p_Fornamn varchar(50),
    IN p_Efternamn varchar(50),
    in p_Lön DECIMAL(10, 2),
    IN p_Avdelning varchar(50),
    IN p_Lösenord varchar(255)
)
begin

    IF EXISTS(SELECT AO.Användarnamn
              from Agent_Open as AO
              WHERE AO.Användarnamn = p_kodnamn) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Agenten finns redan!';
    end if;

    IF p_Lösenord IS NULL THEN
        SET p_Lösenord = '$2y$10$XzrIN22OJau1p8NA2vqOoetMsGGOu1DnWip3DQB2T2VrzpYHUujcy'; -- Default
    end if;

    START TRANSACTION;

    INSERT INTO Agent_Secret (Fornamn, Efternamn, Lön)
    VALUES (p_Fornamn, p_Efternamn, p_Lön);

    SET @new_agent_id = LAST_INSERT_ID();

    INSERT INTO Agent_Open (Agent_ID, Användarnamn, Avdelning, Lösenord)
    VALUES (@new_agent_id, p_kodnamn, p_Avdelning, p_Lösenord);
end @@;

CREATE PROCEDURE IF NOT EXISTS sp_create_alien(
    In p_idkod CHAR(25),
    IN p_PNR CHAR(12),
    IN p_Ras VARCHAR(50),
    IN p_Hemplanet VARCHAR(50),
    IN p_KändaNamn TEXT,
    IN p_ÄrRegistrerad BOOLEAN,
    IN p_Farlighet ENUM (
        'Harmlös', 'Halvt harmlös', 'Ofarlig', 'Neutral',
        'Svagt farlig', 'Farlig', 'Extremt farlig', 'Spring för livet'
        )
)
BEGIN

    IF EXISTS(SELECT 1
              FROM Alien AS a
              WHERE a.IdKod = p_IdKod
                 OR a.PNR = p_PNR) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Alien finns redan!';
    END IF;

    IF p_Ras IS NULL THEN
        SET p_Ras = 'Okänd';
    END IF;

    INSERT INTO Alien (IdKod, PNR, Ras, Hemplanet, KändaNamn, ÄrRegistrerad, Farlighet)
    VALUES (p_IdKod, p_PNR, p_Ras, p_Hemplanet, p_KändaNamn, p_ÄrRegistrerad, p_Farlighet);
END@@;

CREATE PROCEDURE IF NOT EXISTS sp_app_create_alien(
    IN p_PNR CHAR(12),
    IN p_Ras VARCHAR(50),
    IN p_Hemplanet VARCHAR(50),
    IN p_KändaNamn TEXT,
    IN p_ÄrRegistrerad BOOLEAN,
    IN p_Farlighet ENUM (
        'Harmlös', 'Halvt harmlös', 'Ofarlig', 'Neutral',
        'Svagt farlig', 'Farlig', 'Extremt farlig', 'Spring för livet'
        )
)
BEGIN
    DECLARE v_IdKod CHAR(25);

    set v_IdKod = CONCAT(
            char((ascii('A') + floor(RAND()* 26))),
            '-',
            LPAD(FLOOR(RAND() * 00000000000000000000),20, '0'),
            LPAD(FLOOR(RAND() * 999),3, '0')
                  );

    CALL sp_create_alien(v_IdKod, p_PNR, p_Ras, p_Hemplanet, p_KändaNamn, p_ÄrRegistrerad, p_Farlighet);
end @@;

CREATE PROCEDURE IF NOT EXISTS sp_create_incident(
    IN p_Namn varchar(100),
    IN p_NR INT UNSIGNED,
    IN p_Säkerhetsgrad ENUM ('Oproblematisk', 'Låg', 'Mellan', 'Hög', 'Mycket hög', 'Kritisk')
)
begin
    IF EXISTS(SELECT *
              from Incident as I
              WHERE I.Namn = p_Namn
                AND I.NR = p_NR) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Incidenten finns redan!';
    end if;
    INSERT INTO Incident (Namn, NR, Säkerhetsgrad)
    VALUES (p_Namn, p_NR, p_Säkerhetsgrad);

    CAlL sp_Update_Incident_alien_info();
end@@;

CREATE PROCEDURE IF NOT EXISTS sp_add_alien_to_incident(
    IN p_Incident_Namn varchar(100),
    IN p_Incident_NR INT UNSIGNED,
    IN p_Alien_IdKod char(25)
)
begin
    IF NOT EXISTS(SELECT *
                  from Incident as I
                  WHERE I.Namn = p_Incident_Namn
                    AND I.NR = p_Incident_NR) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Incidenten finns inte!';
    end if;

    IF NOT EXISTS(SELECT *
                  from Alien as A
                  WHERE A.IdKod = p_Alien_IdKod) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Alien finns inte!';
    end if;

    IF EXISTS(SELECT *
              from Incident_Alien AS IA
              WHERE IA.Incident_Namn = p_Incident_Namn
                AND IA.Incident_NR = p_Incident_NR
                AND IA.Alien_IdKod = p_Alien_IdKod) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Alien är redan kopplad till incidenten!';
    end if;

    START TRANSACTION;

    INSERT INTO Incident_Alien (Incident_Namn, Incident_NR, Alien_IdKod)
    VALUES (p_Incident_Namn, p_Incident_NR, p_Alien_IdKod);
    COMMIT;
end@@;

CREATE PROCEDURE IF NOT EXISTS sp_achive_alien(
    IN p_IdKod char(25),
    IN p_ArkivAnledning varchar(255),
    IN p_ArkivAv int,
    IN p_isDeleted boolean
)
begin

    DECLARE exit HANDLER FOR SQLEXCEPTION
        begin
            ROLLBACK;
            RESIGNAL;
        end;

    IF NOT EXISTS(SELECT *
                  from Alien AS A
                  WHERE A.IdKod = p_IdKod) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Alien finns inte!';
    end if;

    START TRANSACTION;
    -- DELETUS MAXIMUS, The Rest is done by trigger
    DELETE FROM Alien WHERE IdKod = p_IdKod;

    -- IF no error, then we can add the final touch

    UPDATE Alien_Arkiv AS AA
    SET AA.ArkivAnledning = p_ArkivAnledning,
        AA.ArkivAv        = p_ArkivAv,
        AA.isDeleted      = p_isDeleted
    WHERE AA.IdKod = p_IdKod; -- Finns bara 1

    COMMIT;
end@@;

DELIMITER $$

-- Drop existing procedure first, since IF NOT EXISTS is not supported
DROP PROCEDURE IF EXISTS sp_Update_Incident_alien_info $$
CREATE PROCEDURE sp_Update_Incident_alien_info(
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
        BEGIN
            RESIGNAL;
        END;

    UPDATE Incident I
    SET I.AntalInvolverade = (SELECT COALESCE(
                                             (SELECT COUNT(*)
                                              FROM Incident_Alien IA
                                              WHERE IA.Incident_Namn = I.Namn
                                                AND IA.Incident_NR = I.NR)
                                                 +
                                             (SELECT COUNT(*)
                                              FROM Incident_Alien_Arkiv IAA
                                              WHERE IAA.Incident_Namn = I.Namn
                                                AND IAA.Incident_NR = I.NR),
                                             0
                                     ));


END $$


CREATE PROCEDURE IF NOT EXISTS sp_create_new_comment(
    IN p_Rapport_Datum DATE,
    IN p_Rapport_Nr INT UNSIGNED,
    IN p_Text VARCHAR(500),
    IN p_UserName VARCHAR(10)
)
BEGIN

    DECLARE v_new_nr INT UNSIGNED;
    SET v_new_nr = COALESCE(
                           (SELECT MAX(rr.nr)
                            FROM Rapport_Kommentar as rr
                            WHERE rr.Rapport_Datum = p_Rapport_Datum
                              AND rr.Rapport_Nr = p_Rapport_Nr), 0) + 1;

    INSERT INTO Rapport_Kommentar (Rapport_Datum, Rapport_Nr, nr, Text, GjordAv)
    VALUES (p_Rapport_Datum, p_Rapport_Nr, v_new_nr, p_Text, (
        SELECT ao.Agent_ID
        FROM Agent_Open as ao
        WHERE ao.Användarnamn = p_UserName LIMIT 1));

end $$

CREATE PROCEDURE IF NOT EXISTS sp_get_all_permissions(
    in p_role_name varchar(64)
)
BEGIN
    IF p_role_name like 'admin' THEN
        SELECT TABLE_NAME, TABLE_TYPE
        FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = DATABASE();
    ELSE

        SELECT GRANTEE, TABLE_NAME, PRIVILEGE_TYPE
        FROM information_schema.TABLE_PRIVILEGES
        WHERE TABLE_SCHEMA = 'c24elipe'
          AND GRANTEE LIKE CONCAT('\'c24elipe_', p_role_name, '\'@\'localhost\'');
    END IF;

end $$

DELIMITER $$
-- Triggers
DROP TRIGGER IF EXISTS trg_after_delete_alien;
CREATE TRIGGER trg_after_delete_alien
    AFTER DELETE
    ON Alien
    FOR EACH ROW
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
        BEGIN
            RESIGNAL;
        END;

    -- Insert into archive
    INSERT INTO Alien_Arkiv (IdKod, PNR, Ras, Hemplanet, KändaNamn, ÄrRegistrerad,
                             Farlighet, ArkivDatum, ArkivAnledning, ArkivAv, isDeleted)
    VALUES (OLD.IdKod, OLD.PNR, OLD.Ras, OLD.Hemplanet, OLD.KändaNamn,
            OLD.ÄrRegistrerad, OLD.Farlighet,
            CURRENT_DATE, 'Automatisk arkivering vid borttagning', NULL, FALSE);

END$$

DROP TRIGGER IF EXISTS trg_after_Incident_alien_delete$$;
CREATE TRIGGER IF NOT EXISTS trg_after_Incident_alien_delete
    AFTER DELETE
    ON Incident_Alien
    FOR EACH ROW
BEGIN

    INSERT INTO Incident_Alien_Arkiv (Incident_Namn, Incident_NR, Alien_IdKod) VALUES
    (OLD.Incident_Namn, OLD.Incident_NR, OLD.Alien_IdKod);


end$$;

DROP TRIGGER If EXISTS trg_before_insert_incident_alien$$
CREATE TRIGGER IF NOT EXISTS trg_before_insert_incident_alien
    BEFORE INSERT
    ON Incident_Alien
    FOR EACH ROW
BEGIN

    UPDATE Incident
    SET AntalInvolverade = (
        SELECT COUNT(*)
        FROM Incident_Alien
        WHERE Incident_Namn = NEW.Incident_Namn
          AND Incident_NR   = NEW.Incident_NR
    )
    WHERE Namn = NEW.Incident_Namn
      AND NR   = NEW.Incident_NR;

end $$



INSERT INTO Ras (RasNamn, Beskrivning)
values ('Okänd', 'Okänd ras');


call sp_add_agent('a-1', 'admin', 'istrator', 2000.00, 'admin', NULL);

call sp_create_alien('M-00000000000000000000001', '123456-7890A', NULL, 'Jorden', 'John Doe', TRUE, 'Svagt farlig');
call sp_create_alien('C-00000000000000000000002', '123456-7890B', NULL, 'Jorden', 'John Doe, ADASD, TV', FALSE,
                     'Spring för livet');

call sp_create_incident('UFO-nedslag i Sverige', 1, 'Oproblematisk');
call sp_create_incident('UFO landning', 1, 'Låg');
call sp_add_alien_to_incident('UFO-nedslag i Sverige', 1, 'M-00000000000000000000001');

CALL sp_achive_alien('M-00000000000000000000001', 'Test av arkivering', 1, TRUE);


CALL sp_create_alien('A-000000000000000000003', '123456-7890C', NULL, 'Mars', 'Jane Doe', TRUE, 'Farlig');
CALL sp_add_alien_to_incident('UFO-nedslag i Sverige', 1, 'A-000000000000000000003');
CALL sp_add_alien_to_incident('UFO landning', 1, 'C-00000000000000000000002');



FLUSH PRIVILEGES;

DROP USER IF EXISTS 'c24elipe_admin'@'localhost';
CREATE USER IF NOT EXISTS 'c24elipe_admin'@'localhost' IDENTIFIED BY 'adminPassword123!';

GRANT SELECT, INSERT, UPDATE, DELETE ON c24elipe.Ras TO 'c24elipe_admin'@'localhost';
GRANT SELECT ON c24elipe.vw_Agent_Full TO 'c24elipe_admin'@'localhost';
GRANT ALL PRIVILEGES ON c24elipe.* TO 'c24elipe_admin'@'localhost';
GRANT SELECT ON c24elipe.wv_Alien_Ras_Full TO 'c24elipe_admin'@'localhost';
GRANT EXECUTE ON PROCEDURE c24elipe.sp_app_create_alien TO 'c24elipe_admin'@'localhost';


DROP USER IF EXISTS 'c24elipe_login_service'@'localhost';
CREATE USER IF NOT EXISTS 'c24elipe_login_service'@'localhost' IDENTIFIED BY 'TestPassword123!';
GRANT EXECUTE ON PROCEDURE c24elipe.sp_get_agent_login TO 'c24elipe_login_service'@'localhost';
GRANT EXECUTE ON PROCEDURE c24elipe.sp_give_info_to_login TO 'c24elipe_login_service'@'localhost';


DROP USER IF EXISTS 'c24elipe_groupleader'@'localhost';
CREATE USER IF NOT EXISTS 'c24elipe_groupleader'@'localhost' IDENTIFIED BY 'GroupPass123!';

-- ALien Privileges:
GRANT SELECT ON c24elipe.wv_Alien_Ras_Full TO 'c24elipe_groupleader'@'localhost';
GRANT EXECUTE ON PROCEDURE c24elipe.sp_app_create_alien TO 'c24elipe_groupleader'@'localhost';

-- Incident Privileges:
GRANT SELECT, INSERT ON c24elipe.Incident TO 'c24elipe_groupleader'@'localhost';
GRANT SELECT, INSERT On c24elipe.Incident_Alien TO 'c24elipe_groupleader'@'localhost';
GRANT SELECT ON c24elipe.Alien TO 'c24elipe_groupleader'@'localhost';
GRANT SELECT ON c24elipe.wv_Alien_Incident TO 'c24elipe_groupleader'@'localhost';
GRANT EXECUTE ON PROCEDURE c24elipe.sp_add_alien_to_incident TO 'c24elipe_groupleader'@'localhost';

-- Rapport Privileges:
GRANT SELECT, INSERT, UPDATE ON c24elipe.Rapport TO 'c24elipe_groupleader'@'localhost';
GRANT SELECT ON c24elipe.vw_get_full_rapport TO 'c24elipe_groupleader'@'localhost';
GRANT SELECT ON c24elipe.Agent_Open TO 'c24elipe_groupleader'@'localhost';
GRANT SELECT On c24elipe.Rapport_Rader TO 'c24elipe_groupleader'@'localhost';
GRANT SELECT on c24elipe.wv_Rapport_Kommentar_Full TO 'c24elipe_groupleader'@'localhost';
GRANT EXECUTE ON PROCEDURE c24elipe.sp_add_Rapport_rad TO 'c24elipe_groupleader'@'localhost';
GRANT EXECUTE ON PROCEDURE c24elipe.sp_create_new_comment TO 'c24elipe_groupleader'@'localhost';

-- Arkiv Privileges:
-- NO!


DROP USER IF EXISTS 'c24elipe_agent'@'localhost';
CREATE USER IF NOT EXISTS 'c24elipe_agent'@'localhost' IDENTIFIED BY 'AgentPass123!';

-- Alien Privileges:
GRANT SELECT ON c24elipe.wv_Alien_Ras_Full TO 'c24elipe_agent'@'localhost';

-- Rapport Privileges:
GRANT SELECT On c24elipe.vw_get_full_rapport TO 'c24elipe_agent'@'localhost';
GRANT SELECT ON c24elipe.Rapport TO 'c24elipe_agent'@'localhost';
GRANT SELECT ON c24elipe.Rapport_Rader TO 'c24elipe_agent'@'localhost';



CALL sp_create_alien('NULL', '987654-3210B', NULL, 'Venus', 'Zorg Blorg', TRUE, 'Neutral');


call sp_app_create_alien('654321-1234G', NULL, 'Neptune', 'Xenon Yttrium', FALSE, 'Harmlös');
call c24elipe.sp_app_create_alien('987654-3210d', 'Okänd', 'Här', 'Där', '1', 'Spring för livet');

CALL sp_app_create_alien('123456-7890Z', 'Okänd', 'Pluto', 'Plutonians', TRUE, 'Ofarlig');


INSERT INTO Rapport (Datum, Nr, RapportTyp, Agent_ID, Användarnamn, Incidentledare, Slutdatum, Incident_Namn, Incident_NR) value
('2000-05-15', 1, 'FUCK', 1, 'admin', 'admin', '2018-06-15', 'UFO-nedslag i Sverige', 1);


CALL sp_add_Rapport_rad( '2000-05-15' , 1, 'Detta är en gammal rapport rad som ska arkiveras.');
CALL sp_create_new_comment( '2000-05-15' , 1, 'Detta är en gammal kommentar som ska tas bort.', 'a-1');

CALL sp_archive_old_rapport();

call sp_add_agent('b-2', 'group', 'leader', 2400.00, 'group_leader', NULL);
call sp_add_agent('c-3', 'agent', 'agentson', 1800.00, 'agent', NULL);
call sp_add_agent('d-4', 'agent2', 'DOS', 1801.00, 'agent', NULL);
call sp_add_agent('e-5', 'agent3', 'TRE', 1801.00, 'agent', NULL);

CALL sp_app_create_alien('192020-1234X', 'Okänd', 'Pluto', 'Plutonians', TRUE, 'Ofarlig');
CALL sp_add_alien_to_incident('UFO-nedslag i Sverige', 1, (SELECT a.IdKod FROM Alien as a WHERE PNR = "192020-1234X"));

INSERT INTO Rapport (Datum, Nr, RapportTyp, Agent_ID, Användarnamn, Incidentledare, Slutdatum, Incident_Namn, Incident_NR)
SELECT  '100-10-10', 24, 'bra typ', (SELECT Agent_ID FROM Agent_Open WHERE Användarnamn = 'c-3' LIMIT 1),
        'c-3',
        'b2',
        '1999-10-12',
        (SELECT Namn FROM Incident WHERE Namn = 'UFO landning' LIMIT 1),
        (SELECT NR FROM Incident WHERE NR = 1 LIMIT 1);

INSERT INTO Rapport (Datum, Nr, RapportTyp, Agent_ID, Användarnamn, Incidentledare, Slutdatum, Incident_Namn, Incident_NR)
SELECT  '1009-10-10', 4, 'bra typ', (SELECT Agent_ID FROM Agent_Open WHERE Användarnamn = 'c-3' LIMIT 1),
        'c-3',
        'b2',
        '1999-10-12',
        (SELECT Namn FROM Incident WHERE Namn = 'UFO landning' LIMIT 1),
        (SELECT NR FROM Incident WHERE NR = 1 LIMIT 1);

call c24elipe.sp_app_create_alien( '111111-2222A', 'Okänd', 'Testplanet', 'Testare', true, 'Neutral');

SELECT I.Namn, I.NR, I.Säkerhetsgrad, A.PNR, A.KändaNamn FROM
    Incident_Alien as IA
        JOIN Incident as I on IA.Incident_Namn = I.Namn
        JOIN Alien as A on IA.Alien_IdKod = A.IdKod;

CALL sp_get_agent_login("a-1");
CALL sp_give_info_to_login("a-1");


CALL sp_add_alien_to_incident('UFO landning', 1, (Select IdKod from Alien where PNR = '192020-1234X'));


INSERT INTO Rapport (Datum, Nr, RapportTyp, Agent_ID, Användarnamn, Incidentledare, Slutdatum, Incident_Namn, Incident_NR) value
('100-10-10', 24, 'bra typ', (SELECT Agent_ID FROM Agent_Open WHERE Användarnamn = 'c-3' LIMIT 1),
 'c-3',
 'b2',
 '1999-10-12',
'Testius Incidentus',
            '23');

INSERT INTO Rapport_Rader (Rapport_Datum, Rapport_Nr, nr, Text) VALUES
('100-10-10', 24, 1, 'Detta är rad 1'),
('100-10-10', 24, 2, 'Detta är rad 2'),
('100-10-10', 24, 3, 'Detta är rad 3');

INSERT INTO Rapport_Kommentar (Rapport_Datum, Rapport_Nr, nr, Text, GjordAv) VALUES
('100-10-10', 24, 1, 'Detta är kommentar 1', (SELECT Agent_ID FROM Agent_Open WHERE Användarnamn = 'd-4' LIMIT 1)),
('100-10-10', 24, 2, 'Detta är kommentar 2', (SELECT Agent_ID FROM Agent_Open WHERE Användarnamn = 'a-1' LIMIT 1));


INSERT INTO Rapport_Rader (Rapport_Datum, Rapport_Nr, nr, Text)
SELECT '100-10-10', 24,
       COALESCE(MAX(nr), 0) + 1,
       'Detta är rad 5'
FROM Rapport_Rader
WHERE Rapport_Datum = '100-10-10' AND Rapport_Nr = 24;

SELECT * from Rapport_Rader;

SELECT * FROM c24elipe.wv_Rapport_Kommentar_Full;


DELETE FROM Incident_Alien
WHERE Incident_Namn = 'UFO-nedslag i Sverige' AND Incident_NR = 1 AND Alien_IdKod = (SELECT IdKod FROM Alien WHERE PNR = '123456-7890B');

INSERT INTO Incident_Alien
(Incident_Namn, Incident_NR, Alien_IdKod) VALUES
('UFO-nedslag i Sverige', 1, (SELECT IdKod FROM Alien WHERE PNR = '123456-7890B'));
