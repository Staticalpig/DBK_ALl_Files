<?php
if (session_status() === PHP_SESSION_NONE) {
    session_start();
}

require 'PHP/db_login.php';
include 'PHP/nav_header.php';

if ($_SESSION['role'] === 'agent') die("Endast gruppledare eller admin kan skapa incidenter.");
try {
    $pdo = login_specific_db($_SESSION['db_user'], $_SESSION['db_pass']);
}
catch (Exception $e) {
    http_response_code(500);
    exit('Connection error: ' . htmlspecialchars($e->getMessage()));
}

if ($_SERVER['REQUEST_METHOD'] === 'POST' && !isset($_POST['Anslut_Incident_Namn'], $_POST['Anslut_Alien_IdKod'])) {
    try {

        $stmt = $pdo->prepare("INSERT INTO Incident(Namn, NR, Säkerhetsgrad) value (?, ?, ?);");
        $stmt->execute([$_POST['Namn'], $_POST['NR'], $_POST['Säkerhetsgrad']]);
    } catch (PDOException $e) {
        echo "<p>Fel: {$e->getMessage()}</p>";
    }
}

if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['Anslut_Incident_Namn'], $_POST['Anslut_Alien_IdKod'])) {
    try {
        $stmt = $pdo->prepare("INSERT INTO Incident_Alien(Incident_Namn, Incident_NR, Alien_IdKod) VALUES (?, ?, ?);");

        $incidentName = explode("-", $_POST['Anslut_Incident_Namn'])[0] ?? null;
        $incidentNr = explode("-", $_POST['Anslut_Incident_Namn'])[1] ?? null;

        $stmt->execute([$incidentName,$incidentNr, $_POST['Anslut_Alien_IdKod']]);
        header("Location: incidents.php");

    } catch (PDOException $e) {
        echo "<p>Fel vid anslutning av alien till incident: {$e->getMessage()}</p>";
    }
}

$ALlIncidents = [];
function getIncidentAndAlien($pdo): String
{
    global $ALlIncidents;
    $HTML = "<table border='1'>
                <tr><th>Incident</th><th>ALIEN</th></tr>";

    $GetAliens = "SELECT ia.Incident_Namn, a.PNR, a.KändaNamn as Alias
              FROM Incident_Alien as ia
              LEFT JOIN Alien as a on ia.Alien_IdKod = a.IdKod
              ORDER BY Incident_Namn;";

    $stmt = $pdo->query($GetAliens);
    $alien_results = $stmt->fetchAll(PDO::FETCH_ASSOC);

    $incident_results = $pdo->query("SELECT * FROM Incident")->fetchAll(PDO::FETCH_ASSOC);

    foreach ($incident_results as $incident) {


        $incidentName = htmlspecialchars($incident['Namn']);
        $incidentNr = htmlspecialchars($incident['NR']);

        $ALlIncidents[] = $incident['Namn'] . "-" . $incident['NR'] ;

        $sakerhetsgrad = htmlspecialchars($incident['Säkerhetsgrad']);

        $noAliens = true;
        $alienTable = "<table border='1' style='border-collapse: collapse; width: 100%;'>";
        foreach ($alien_results as $alien) {
            if ($alien['Incident_Namn'] === $incident['Namn']) {
                $noAliens = false;
                $alienTable .= "
                <tr>
                    <td>
                        <a href='aliens.php?PNR=" . urlencode($alien['PNR']) . "' style='text-decoration: none; color: inherit; display: block;'>
                            <table style='margin-bottom: 0.25rem; border: 1px solid #000; width: 100%; border-collapse: collapse;'>
                                <tr><td>PNR: <strong>" . htmlspecialchars($alien['PNR']) . "</strong></td></tr>
                                <tr><td>Alias: " . htmlspecialchars($alien['Alias']) . "</td></tr>
                            </table>
                        </a>
                    </td>
                </tr>";
            }
        }
        $alienTable .= "</table>";
        $alienTable = !$noAliens ? $alienTable : "Inga aliens kopplade";

        $HTML .= "
        <tr>
            <td>
                <table border='1' style='border-collapse: collapse;'>
                    <tr><td><strong>Namn: {$incidentName}</strong></td></tr>
                    <tr><td>PNR: {$incidentNr}</td></tr>
                    <tr><td>Säkerhetsgrad: {$sakerhetsgrad}</td></tr>
                </table>
            </td>
            <td>{$alienTable}</td>
        </tr>";
    }

    $HTML .= "</table>";
    return $HTML;
}

function getAllAliens($pdo): array
{
    $AllAliens = [];
    $stmt = $pdo->query("SELECT IdKod, KändaNamn FROM Alien;");
    $aliens = $stmt->fetchAll(PDO::FETCH_ASSOC);
    foreach ($aliens as $alien) {
        $AllAliens[$alien['IdKod']] = $alien['KändaNamn'];
    }

    if (!$AllAliens) {
        $AllAliens = [];
    }
    return $AllAliens;
}

?>


<h2>Alla Incidenter</h2>
<?=getIncidentAndAlien($pdo)?>
<h3>Ny incident</h3>
<form method="POST">
    <input name="Namn" placeholder="Incidentnamn" required>
    <input name="NR" type="number" required>
    <select name="Säkerhetsgrad">
        <option value="Oproblematisk">Oproblematisk</option>
        <option value="Låg">Låg</option>
        <option value="Mellan">Mellan</option>
        <option value="Hög">Hög</option>
        <option value="Mycket hög">Mycket hög</option>
        <option value="Kritisk">Kritisk</option>
    </select>
    <button>Skapa</button>
</form>

<!-- IF admin or group_leader -->
<?php if ($_SESSION['role'] === 'admin' || $_SESSION['role'] === 'group_leader'): ?>

<h3>Anslut alien till Incident</h3>
<form method="POST"">
    <select name="Anslut_Incident_Namn" required>
        <?php foreach ($ALlIncidents as $incidentName): ?>
            <option value="<?=htmlspecialchars($incidentName)?>"><?=htmlspecialchars($incidentName)?></option>
        <?php endforeach; ?>
    </select>
    <select name="Anslut_Alien_IdKod" required>
        <?php
        $allAliens = getAllAliens($pdo);
        foreach ($allAliens as $idKod => $kandaNamn): ?>
            <option value="<?=htmlspecialchars($idKod)?>"><?= htmlspecialchars($idKod)." : ".htmlspecialchars($kandaNamn)?></option>
        <?php endforeach; ?>
    </select>
    <button>Anslut</button>
</form>


<?php endif; ?>