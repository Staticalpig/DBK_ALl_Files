<?php
if (session_status() === PHP_SESSION_NONE) {
    session_start();
}

require 'PHP/db_login.php';
include 'PHP/nav_header.php';

$pdo = login_specific_db($_SESSION['db_user'], $_SESSION['db_pass']);

if (!in_array($_SESSION['role'], ['admin'])) {
    die("Ingen åtkomst.");
}

if (isset($_GET['mass_archive'])) {
    $mass_archive = $_GET['mass_archive'];
    $quary = "";

    if ($mass_archive === 'rapporter') {
        $quary = "CALL c24elipe.sp_archive_old_rapport();";
    } elseif ($mass_archive === 'aliens') {
        // Arkivera aliens som inte är inblandade i någon incident
        $quary = "DELETE A
                    FROM Alien AS A
                    WHERE NOT EXISTS(
                        SELECT 1
                        FROM Incident_Alien AS IA
                        WHERE IA.Alien_IdKod = A.IdKod
                    )";
    } else {
        die("Ogiltig arkiveringsåtgärd.");
    }
    try {
        $stmt = $pdo->prepare($quary);
        $stmt->execute();
        $affectedRows = $stmt->rowCount();
        echo "<p>Massarkivering slutförd.</p>";
    } catch (PDOException $e) {
        echo "<p>Fel vid massarkivering: {$e->getMessage()}</p>";
    }
}


function getAlienAkiv($pdo):string
{
    $stmt = $pdo->query("SELECT * FROM Alien_Arkiv 
                         LEFT JOIN c24elipe.Ras R on Alien_Arkiv.Ras = R.RasNamn
                         ORDER BY Alien_Arkiv.ArkivDatum DESC;");
    $aliens = $stmt->fetchAll(PDO::FETCH_ASSOC);


    $html = "<div><h2>Arkiverade Aliens</h2>";
    $html .= "<table border='1'>
                <tr>
                    <th>IdKod</th>
                    <th>PNR</th>
                    <th>Ras</th>
                    <th>Hemplanet</th>
                    <th>KändaNamn</th>
                    <th>ÄrRegistrerad</th>
                    <th>Farlighet</th>
                    <th>Arkiv Datum</th>
                </tr>";

    foreach ($aliens as $alien) {
        $html .= "<tr>
                    <td>" . htmlspecialchars($alien['IdKod']) . "</td>
                    <td>" . htmlspecialchars($alien['PNR']) . "</td>
                    <td>" . htmlspecialchars($alien['Ras']) . "</td>
                    <td>" . htmlspecialchars($alien['Hemplanet']) . "</td>
                    <td>" . htmlspecialchars($alien['KändaNamn']) . "</td>
                    <td>" . ($alien['ÄrRegistrerad'] ? 'Ja' : 'Nej') . "</td>
                    <td>" . htmlspecialchars($alien['Farlighet']) . "</td>
                    <td>" . htmlspecialchars($alien['ArkivDatum']) . "</td>
                  </tr>";
    }


    return $html . "</table></div>";
}

function getRaportKomments($PDO, $rapportDatum, $rapportNr):string
{
    $stmt = $PDO->prepare("SELECT ao.Användarnamn, rk.Text FROM Rapport_Kommentar as rk
                            LEFT JOIN Agent_Open as ao ON rk.GjordAv = ao.Agent_ID
                            WHERE Rapport_Datum = ?
                            AND Rapport_Nr = ?;");

    $stmt->execute([$rapportDatum, $rapportNr]);
    $komments = $stmt->fetchAll(PDO::FETCH_ASSOC);

    $html = "<table border='1'>";
    $html .= "<tr><th>Gjord av</th><th>Kommentar text</th></tr>";
    if (empty($komments)) {
        $html .= "<tr><td colspan='2'>Inga kommentarer</td></tr>";
    }
    foreach ($komments as $komment) {
        $html .= "<tr><td> ".htmlspecialchars($komment['Användarnamn'])."</td> <td>" . htmlspecialchars($komment['Kommentar']) . "</td></tr>";
    }
    $html .= "</table>";

    return $html;

}

function getAmountOfRows($PDO, $rapportDatum, $rapportNr):int
{
    $stmt = $PDO->prepare("SELECT COUNT(*) as AntalRader FROM Rapport_Rader_Arkiv
                            WHERE Rapport_Datum = ?
                              AND Rapport_Nr = ?;");

    $stmt->execute([$rapportDatum, $rapportNr]);
    $result = $stmt->fetch(PDO::FETCH_ASSOC);

    return (int)$result['AntalRader'];
}

function getRapportArkiv($pdo):string
{
    $stmt = $pdo->query("SELECT * FROM Rapport_Arkiv ORDER BY ArkivDatum DESC;");
    $rapporter = $stmt->fetchAll(PDO::FETCH_ASSOC);

    $html = "<div><h2>Arkiverade Rapporter</h2>";
    $html .= "<table border='1'>
                <tr>
                    <th>Datum</th>
                    <th>Nr</th>
                    <th>Typ</th>
                    <th>Ansvarig agent</th>
                    <th>Ansvraig Ledare</th>
                    <th>Slut Datum</th>
                    <th>Incident</th>
                    <th>Arkiv Datum</th>
                    <th>Arkiv Anledning</th>
                    <th>Kommentarer</th>
                    <th>Antal rader</th>
                </tr>";
    foreach ($rapporter as $rapport) {
        $html .= "<tr>
                        <td>".htmlspecialchars($rapport['Datum'])."</td>
                        <td>".htmlspecialchars($rapport['Nr'])."</td>
                        <td>".htmlspecialchars($rapport['RapportTyp'])."</td>
                        <td>".htmlspecialchars($rapport['Användarnamn'])."</td>
                        <td>".htmlspecialchars($rapport['Incidentledare'])."</td>
                        <td>".htmlspecialchars($rapport['ArkivDatum'])."</td>
                        <td>".htmlspecialchars($rapport['ArkivAnledning'])."</td>
                        <td> ". htmlspecialchars($rapport['ArkivDatum'])."</td>
                        <td> ".htmlspecialchars($rapport['ArkivAnledning'])."</td>
                        <td> ".getRaportKomments($pdo, $rapport['Datum'], $rapport['Nr'])."</td>
                        <td> ".getAmountOfRows($pdo, $rapport['Datum'], $rapport['Nr'])."</td>
                    </tr>";
    }
    $html .= "</table></div>";


    return $html;
}

?>

<h2>MASS Arkivering</h2>
<a href="archive.php?mass_archive=rapporter">Rapporter</a> <p style="display: inline-block;">Som är 5+ år</p>
<br>
<a href="archive.php?mass_archive=aliens">Aliens </a><p style="display: inline-block;">Som inte är inblandad med en Incident</p>

<div style="margin: 2rem 0;">
    <?PHP echo getAlienAkiv($pdo); ?>
</div>
<div>
    <?PHP echo getRapportArkiv($pdo); ?>
</div>
