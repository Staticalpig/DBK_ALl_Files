<?php
if (session_status() === PHP_SESSION_NONE) {
    session_start();
}

require 'PHP/db_login.php';
include 'PHP/nav_header.php';


if (!in_array($_SESSION['role'], ['agent', 'group_leader', 'admin'])) {
    http_response_code(403);
    echo "<a href='login.php'>Logga in</a> innan du kan vara här";
    exit('Forbidden');
}

try {
    $pdo = login_specific_db($_SESSION['db_user'], $_SESSION['db_pass']);
    $reports = [];
    if ($_SESSION['role'] === 'agent') {
        $reports = $pdo->prepare("SELECT * FROM vw_test_rapport WHERE Användarnamn = ?");
        $reports->execute([$_SESSION['username']]);
        $reports = $reports->fetchAll(PDO::FETCH_ASSOC);
    }
    else{
        $reports = $pdo->query("SELECT * FROM vw_test_rapport;")->fetchAll(PDO::FETCH_ASSOC);
    }
} catch (Exception $e) {
    http_response_code(500);
    exit('Connection error: ' . htmlspecialchars($e->getMessage()));
}



/*function getReportCommentsHTML($pdo, $datum, $nr): string
{
    $statment = $pdo->prepare("select * from wv_Rapport_Kommentar_Full 
                                where Rapport_Datum = ? 
                                  and Rapport_Nr = ?;");
    $statment->execute([$datum, $nr]);
    $comments = $statment->fetchAll(PDO::FETCH_ASSOC);

    if (!$comments) {
        return "<tr><td>Inga kommentarer hittades för denna rapport.</td></tr>";
    }

    $html = "<tr><th>Kommentarer</th></tr>";

    $html .= "<td><h3>Lägg till kommentar:</h3>
               
    <form method='POST''>
                        <textarea name='comment_text' rows='4' cols='50' required></textarea><br>
                        <input type='submit' value='Lägg till kommentar'>
                    </form>
                </td></tr>";

    foreach ($comments as $comment) {
        $html .= "<tr><td>";
        $html .= "<p><strong>" . htmlspecialchars($comment['GjordAv']) . " skrev:</strong></p>";
        $html .= "<p>" . htmlspecialchars($comment['Text']) . "</p>";
        $html .= "</td></tr>";
    }


    return $html;
}
*/
function getReportCommentsHTML($pdo, $datum, $nr):string
{
    $statement = $pdo->prepare("
        SELECT * FROM c24elipe.wv_Rapport_Kommentar_Full 
        WHERE Rapport_Datum = ? 
          AND Rapport_Nr = ?;
    ");

    $statement->execute([$datum, $nr]);
    $comments = $statement->fetchAll(PDO::FETCH_ASSOC);



    $html = "<tr><th>Kommentarer</th></tr>";
    $html .= "<tr><td>
                <h3>Lägg till kommentar:</h3>
                <form method='POST'>
                    <textarea name='comment_text' rows='4' cols='50' required></textarea><br>
                    <input type='submit' value='Lägg till kommentar'>
                </form>
            </td></tr>";

    if (!$comments) {
        $html .= "<tr><td>Inga kommentarer hittades för denna rapport.</td></tr>";
        return $html;
    }


    foreach ($comments as $comment) {
        $html .= "
            <tr><td>
                <form method='POST' style='margin:0;'>
                    <input type='hidden' name='selected_comment_id' value='" . htmlspecialchars($comment['nr']) . "'>
                    <input type='hidden' name='selected_comment_text' value='" . htmlspecialchars($comment['Text']) . "'>";

            if ($_SESSION['role'] === 'admin' || $_SESSION['username'] === $comment['GjordAv'] || $comment['Incidentledare'] == $_SESSION['username']) {
                $html .= "
                    <button type='submit' style='background:none;border:none;padding:0;text-align:left;width:100%;cursor:pointer;'>
                        <p><strong>" . htmlspecialchars($comment['GjordAv']) . " skrev:</strong></p>
                        <p>" . nl2br(htmlspecialchars($comment['Text'])) . "</p>
                    </button>
                ";
            } else {
                $html .= "
                    <div>
                        <p><strong>" . htmlspecialchars($comment['GjordAv']) . " skrev:</strong></p>
                        <p>" . nl2br(htmlspecialchars($comment['Text'])) . "</p>
                    </div>
                ";
            }
        $html .= "</form></td></tr>";
    }


    return  $html;
}
function getRader($pdo, $datum, $nr): string
{
    $get_rapport_owners = "SELECT Användarnamn, Incidentledare 
                             FROM Rapport
                             WHERE Datum = ?
                               AND Nr = ?
                             LIMIT 1;";

    $get_Rapport_Paragraphs = $pdo->prepare("SELECT rr.nr, rr.Text
                                             FROM Rapport_Rader as rr
                                             WHERE Rapport_Datum = ?
                                                AND Rapport_Nr = ?
                                             ORDER BY rr.nr;");
    
    
    $get_Rapport_Paragraphs->execute([$datum, $nr]);
    $report_paragraphs = $get_Rapport_Paragraphs->fetchAll(PDO::FETCH_ASSOC);

    $report_owners_stmt = $pdo->prepare($get_rapport_owners);
    $report_owners_stmt->execute([$datum, $nr]);
    $report_owners = $report_owners_stmt->fetch(PDO::FETCH_ASSOC);

    $raportRader = "";

    $isAllowedToEdit = ($_SESSION['role'] === 'admin' || $_SESSION['username'] === $report_owners['Användarnamn'] || $_SESSION['username'] === $report_owners['Incidentledare']);


    if ($isAllowedToEdit) {
        $raportRader .= "<form method='POST'>
            <input type='hidden' name='report_datum' value='" . htmlspecialchars($datum) . "'>
            <input type='hidden' name='report_nr' value='" . htmlspecialchars($nr) . "'>
            <textarea name='radd_new_added_text' rows='4' cols='50' required placeholder='Ny rad'></textarea><br>
            <input type='submit' value='Lägg till ny rad'>
        </form>";
    }


    if ($isAllowedToEdit) {
        $raportRader .= "<form method='POST' style='margin-bottom:1rem;'>";
        $raportRader .= "<input type='hidden' name='report_datum' value='" . htmlspecialchars($datum) . "'>";
        $raportRader .= "<input type='hidden' name='report_nr' value='" . htmlspecialchars($nr) . "'>";
    }
    if ($report_paragraphs) {
        foreach ($report_paragraphs as $row) {
            if ($isAllowedToEdit) {
                $raportRader .= "<input type='hidden' name='radd_nr' value='" . htmlspecialchars($row['nr']) . "'>";
                $raportRader .= "<input type='hidden' name='radd_text' value='" . htmlspecialchars($row['Text']) . "'>";
                $raportRader .= "<button type='submit' style='background:none;border:none;padding:0;text-align:left;width:100%;cursor:pointer;'>
                                    <p>" . htmlspecialchars($row['Text']) . "</p>
                                </button>";
            } else {
                $raportRader .= "<p>" . htmlspecialchars($row['Text']) . "</p>";
            }
        }
    }else{
        $raportRader .= "<p>Inga rader hittades för denna rapport.</p>";
    }



    if ($isAllowedToEdit) {
        $raportRader .= "</form>";
    }

    return $raportRader;
}

function showReport_Details($pdo): string
{

    $get_Rapport_Details = $pdo->prepare("SELECT * FROM Rapport WHERE CONCAT(Datum,'-', Nr) = ? LIMIT 1;");


    $get_Rapport_Details->execute([$_GET['report']]);
    $report_details = $get_Rapport_Details->fetch(PDO::FETCH_ASSOC);

    if (!$report_details) {
        return "<p>Rapporten hittades inte.</p>";
    }

    $html = "<h3>Rapport Detaljer för raporten: {$_GET['report']}</h3>";
    $html .= "<table border='1'><tr>";

    // The header name to the left, value to the right
    // Value to include Datum, Nr, AgentAnvändarnamn, GroupledareAnvändarnamn, slutdatum, Incident
    $html .= "
            <td>
                <table border='1'>
                    <tr><td>Datum</td><td>" . htmlspecialchars($report_details['Datum']) . "</td></tr>
                    <tr><td>Nr</td><td>" . htmlspecialchars($report_details['Nr']) . "</td></tr>
                    <tr><td>Agent Användarnamn</td><td>" . htmlspecialchars($report_details['Användarnamn']) . "</td></tr>
                    <tr><td>Incidentledare</td><td>" . htmlspecialchars($report_details['Incidentledare']) . "</td></tr>
                    <tr><td>Slutdatum</td><td>" . htmlspecialchars($report_details['Slutdatum']) . "</td></tr>
                    <tr><td>Incident Namn</td><td>" . htmlspecialchars($report_details['Incident_Namn']) . "</td></tr>
                    <tr><td>Incident Nr</td><td>" . htmlspecialchars($report_details['Incident_NR']) . "</td></tr>
                    <tr>
                        <td colspan='2'>
                            <table>
                                <tr>
                                    <td>
                                        <a href='incidents.php?incident=" . urlencode($report_details['Incident_Namn'] . '-' . $report_details['Incident_NR']) . "'>
                                            Till Incidenten
                                        </a>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>";

    $html .= "<td>
                <table border='1'>
                    <tr><th>Rader</th></tr>
                    <tr><td>" . getRader($pdo, $report_details['Datum'], $report_details['Nr']). "</td></tr>
                </table>
            </td>";

    $html .= "<td>
                <table border='1'>
                    ". getReportCommentsHTML($pdo, $report_details['Datum'], $report_details['Nr']) ."
                </table>

    </td>";




    $html .= "</tr></table>";

    return $html;
}
function getCreateRapportForm($pdo): string
{
    $stmt_get_incidents = $pdo->query("SELECT inc.Namn, inc.NR FROM Incident as inc;");
    $incidents = $stmt_get_incidents->fetchAll(PDO::FETCH_ASSOC);

    $getAllGroupLeaders = $pdo->query("SELECT Användarnamn FROM Agent_Open WHERE Avdelning = 'group_leader';");
    $groupLeaders = $getAllGroupLeaders->fetchAll(PDO::FETCH_ASSOC);

    $getAllAgents = $pdo->query("SELECT Användarnamn FROM Agent_Open WHERE Avdelning = 'agent';");
    $agents = $getAllAgents->fetchAll(PDO::FETCH_ASSOC);


    $html = "<h2>Skapa Ny Rapport</h2>
    <form method='POST'>
        <input type='hidden' name='create_rapport' value='1'>
        <label for='RapportDatum'>Datum:</label>
        
        <input type='date' id='RapportDatum' name='RapportDatum' required><br>
        <label for='RapportNr'>Nr</label>
        
        <input type='number' id='RapportNr' name='RapportNr' required><br>
        <label for='RapportTyp'>Rapport Typ:</label>
        
        <input type='text' id='RapportTyp' name='RapportTyp' required><br>
        <label for='AgentAnvändarnamn'>Agent Användarnamn :</label>
        <select id='AgentAnvändarnamn' name='AgentAnvändarnamn' required>";

    foreach ($agents as $agent) {
        $html .= "<option value='" . htmlspecialchars($agent['Användarnamn']) . "'>" . htmlspecialchars($agent['Användarnamn']) . "</option>";
    }

    $html .= "</select><br>
        <label for='IncidentledareAnvändarnamn'>Incidentledare Användarnamn :</label>
        <select id='IncidentledareAnvändarnamn' name='IncidentledareAnvändarnamn' required>";
    foreach ($groupLeaders as $leader) {
        $html .= "<option value='" . htmlspecialchars($leader['Användarnamn']) . "'>" . htmlspecialchars($leader['Användarnamn']) . "</option>";
    }
    $html .= "</select><br>
        <label for='RapportSlutDatum'>Slut Datum:</label>
        <input type='date' name='RapportSlutDatum' required> <br>
    
        <label>Välj incident</label>
        <select name='Incident'>";

    foreach ($incidents as $incident) {
        // Get each incident as Name-nr
        $incident_namne =$incident['Namn']."-".$incident['NR'];
        $html .= "<option value='". ($incident_namne)."'>".htmlspecialchars($incident_namne)."</option>";
    }

    $html .="</select><br>
    <input type='submit' placeholder='create'></form>";
    return $html;
}

if (!empty($_POST['comment_text'])) {
    $insertComment = $pdo->prepare("CALL sp_create_new_comment(?, ?, ?, ?);");

    $explode = explode("-", $_GET['report']);
    $reportDatum = implode("-", array_slice($explode, 0, count($explode)-1));
    $reportNr = $explode[count($explode)-1] ?? null;

    try {
        //    IN p_Rapport_Datum DATE,
        //    IN p_Rapport_Nr INT UNSIGNED,
        //    IN p_Text VARCHAR(500),
        //    IN p_UserName VARCHAR(10)
        $insertComment->execute([
            $reportDatum,
            $reportNr,
            $_POST['comment_text'],
            $_SESSION['username']
        ]);
        header("Location: reports.php?report=" . urlencode($_GET['report']));
        exit();
    } catch (PDOException $e) {
        echo "<p>Fel vid tillägg av kommentar: " . htmlspecialchars($e->getMessage()) . "</p>";
    }
}

if (!empty($_POST['comment_id']) && !empty($_POST['updated_comment_text'])) {
    $updateComment = $pdo->prepare("
    UPDATE Rapport_Kommentar
    SET Text = ?
    WHERE Rapport_Datum = ?
    AND Rapport_Nr = ?
    AND nr = ?;");

    $explode = explode("-", $_GET['report']);
    $reportDatum = implode("-", array_slice($explode, 0, count($explode)-1));
    $reportNr = $explode[count($explode)-1] ?? null;

    try {
        $updateComment->execute([
            $_POST['updated_comment_text'],
            $reportDatum,
            $reportNr,
            $_POST['comment_id']
        ]);
        header("Location: reports.php?report=" . urlencode($_GET['report']));
        exit();
    } catch (PDOException $e) {
        echo "<p>Fel vid uppdatering av kommentar: " . htmlspecialchars($e->getMessage()) . "</p>";
    }
}

if (!empty($_POST['radd_nr']) && !empty($_POST['radd_new_text'] )) {

    $query = "";
    if (isset($_POST['delete_radd'])) {
        $query = "
       DELETE FROM Rapport_Rader
        WHERE Rapport_Datum = ?
          AND Rapport_Nr = ?
          AND nr = ?;";
    } else {
        $query = "
       UPDATE Rapport_Rader
        SET Text = ?
        WHERE Rapport_Datum = ?
          AND Rapport_Nr = ?
          AND nr = ?;";
    }

    print_r($query);

    $updateRadd = $pdo->prepare($query);

    try {
        if (isset($_POST['delete_radd'])) {
            $updateRadd->execute([
                $_POST['report_datum'],
                $_POST['report_nr'],
                $_POST['radd_nr']
            ]);
            header("Location: reports.php?report=" . urlencode($_POST['report_datum']."-".$_POST['report_nr']));
            exit();
        }
        $updateRadd->execute([
                $_POST['radd_new_text'],
                $_POST['report_datum'],
                $_POST['report_nr'],
                $_POST['radd_nr']
        ]);
        header("Location: reports.php?report=" . urlencode($_POST['report_datum']."-".$_POST['report_nr']));
        exit();
    } catch (PDOException $e) {
        echo "<p>Fel vid uppdatering av rapport rad: " . htmlspecialchars($e->getMessage()) . "</p>";
    }
}

if (!empty($_POST['radd_new_added_text'])){
    $insertRadd = $pdo->prepare("CALL sp_add_Rapport_rad(?, ?, ?)");

    $explode = explode("-", $_GET['report']);
    $reportDatum = implode("-", array_slice($explode, 0, count($explode)-1));
    $reportNr = $explode[count($explode)-1] ?? null;
    try {
        $insertRadd->execute([
            $reportDatum,
            $reportNr,
            $_POST['radd_new_added_text']
        ]);

       header("Location: reports.php?report=" . urlencode($_POST['report_datum']."-".$_POST['report_nr']));
        exit();
    } catch (PDOException $e) {
        echo "<p>Fel vid tillägg av rapport rad: " . htmlspecialchars($e->getMessage()) . "</p>";
    }
}

if (!empty($_POST['create_rapport'])) {
    $stmt = $pdo->prepare("
        INSERT INTO Rapport (Datum, Nr, RapportTyp, Agent_ID, Användarnamn, Incidentledare, Slutdatum, Incident_Namn, Incident_NR)
        SELECT  ?, ?, ?, 
                (SELECT Agent_ID FROM Agent_Open WHERE Användarnamn = ? LIMIT 1),
                ?,
                ?,
                ?,
        (SELECT Namn FROM Incident WHERE Namn = 'UFO landning' LIMIT 1),
        (SELECT NR FROM Incident WHERE NR = 1 LIMIT 1);");
    try {
        $incident_explode = explode("-", $_POST['Incident']);
        $incident_nr = $incident_explode[count($incident_explode)-1];
        $incident_namn = implode("-", array_slice($incident_explode, 0, count($incident_explode)-1));

        $stmt->execute([
            $_POST['RapportDatum'],
            $_POST['RapportNr'],
            $_POST['RapportTyp'],
            $_POST['AgentAnvändarnamn'],
            $_POST['AgentAnvändarnamn'],
            $_POST['IncidentledareAnvändarnamn'],
            $_POST['RapportSlutDatum'],
            $incident_namn,
            $incident_nr
        ]);
        echo "<p>Rapport skapad framgångsrikt.</p>";
        header("Location: reports.php");
        exit();
    } catch (PDOException $e) {
        echo "<p>Fel vid skapande av rapport: " . htmlspecialchars($e->getMessage()) . "</p>";
    }
}

?>
<h2>Rapporter</h2>
<table border="1">
    <tr><th>Datum</th><th>Nr</th><th>Typ</th><th>Incident</th><th>SELECT</th></tr>
    <?php foreach ($reports as $r): ?>
        <tr>
            <td><?= htmlspecialchars($r['Datum']) ?></td>
            <td><?= htmlspecialchars($r['Nr']) ?></td>
            <td><?= htmlspecialchars($r['RapportTyp']) ?></td>
            <td><?= htmlspecialchars($r['Incident_Namn']) ?></td>
            <td>
                <?php
                    if (!empty($_GET['report']) && $_GET['report'] === urldecode($r['Datum']."-".$r['Nr'])) {
                        echo '<a href="reports.php">DESELECT</a>';
                        continue;
                    }
                    if ($_SESSION['role'] === 'admin' || $_SESSION['role'] === 'group_leader' or $_SESSION['username'] === $r['Användarnamn']) {
                        echo '<a href="reports.php?report=' . urldecode($r['Datum']."-".$r['Nr']) . '">Visa Rapport</a>';
                    }
                ?>
            </td>
        </tr>
    <?php endforeach; ?>
</table>

<?php if (!empty($_GET['report'])) echo showReport_Details($pdo);?>

<?php if (!empty($_POST['selected_comment_id'])) {
    echo "<h3>Uppdatera kommentar</h3>";
    echo "<form method='POST'>
            <input type='hidden' name='comment_id' value='" . htmlspecialchars($_POST['selected_comment_id']) . "'>
            <textarea name='updated_comment_text' rows='4' cols='50' required>" . htmlspecialchars($_POST['selected_comment_text']) . "</textarea><br>
            <input type='submit' value='Uppdatera kommentar'>
          </form>";
} ?>
<?php if (!empty($_POST['radd_nr'])) {
    $text = $_POST['radd_text'] ?? '';

    echo "<h3>Uppdatera Rapport Rad</h3>";
    echo "<form method='POST'>
            <input type='hidden' name='report_datum' value='" . htmlspecialchars($_POST['report_datum']) . "'>
            <input type='hidden' name='report_nr' value='" . htmlspecialchars($_POST['report_nr']) . "'>
            <input type='hidden' name='radd_nr' value='" . htmlspecialchars($_POST['radd_nr']) . "'>
            <textarea name='radd_new_text' rows='4' cols='50'>" . htmlspecialchars($text) . "</textarea><br>
            <input type='checkbox' name='delete_radd'>Radera raden<br>
            <input type='submit' value='Uppdatera Rad'>
          </form>";

}?>

<?php if ($_SESSION['role'] === 'admin' OR $_SESSION['role'] === 'group_leader') {
    echo getCreateRapportForm($pdo);
}?>