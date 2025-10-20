<?php

if (basename(__FILE__) == basename($_SERVER['SCRIPT_FILENAME'])) { // Om denna fil körs direkt
    http_response_code(403);
    exit('Forbidden');
}

echo "<!-- Including DB login file -->";
// Om sessioner är inte startad
if (session_status() == PHP_SESSION_DISABLED) {
    session_start();
}


// dsn
CONST DSN  = "mysql:host=127.0.0.1;dbname=c24elipe;charset=utf8mb4";
CONST LOGIN_SERVICE_USER= "c24elipe_login_service";
CONST LOGIN_SERVICE_PASSWORD= "TestPassword123!";

function login_db(): PDO
{
    try {
        $pdo = new PDO(DSN, LOGIN_SERVICE_USER, LOGIN_SERVICE_PASSWORD, [
            PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,      // FUCKING klaga på mig!!!!!
            PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC, // Få en associativ array som standard
            PDO::ATTR_EMULATE_PREPARES   => false,
        ]);
    } catch (PDOException $e) {
        throw new PDOException($e->getMessage(), (int)$e->getCode());
    } catch (Exception $e) {
        throw new Exception($e->getMessage(), (int)$e->getCode());
    }

    return $pdo;
}

function login_specific_db(string $db_name, string $db_password): PDO
{

    try {
        $pdo = new PDO(DSN, $db_name, $db_password, [
            PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,        // FUCKING klaga på mig!!!!!
            PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,   // GE RIKTIG DATA FOR FUCK SAKS
            PDO::ATTR_EMULATE_PREPARES => false,                // Denna gör
            PDO::MYSQL_ATTR_USE_BUFFERED_QUERY => true,         // SLÄNG DET I RAM
        ]);
    } catch (PDOException $e) {
        throw new PDOException($e->getMessage(), (int)$e->getCode());
    } catch (Exception $e) {
        throw new Exception($e->getMessage(), (int)$e->getCode());
    }

    return $pdo;
}

?>
