<?php
if (session_status() === PHP_SESSION_NONE) {
    session_start();
}

require 'PHP/db_login.php';
include 'PHP/nav_header.php';

if (empty($_SESSION['db_user']) || empty($_SESSION['db_pass'])) {
    header('Location: index.php?redirected=true&reason='.urlencode('Du måste logga in'));
    exit();
}

try {
    $pdo = login_specific_db($_SESSION['db_user'], $_SESSION['db_pass']);
}
catch (Exception $e) {
    http_response_code(500);
    exit('Connection error: ' . htmlspecialchars($e->getMessage()));
}

if (!in_array($_SESSION['role'], ['agent', 'group_leader', 'admin'])) {
    http_response_code(403);
    echo "<a href='login.php'>Logga in</a> innan du kan vara här";
    exit('Forbidden');
}

// Add alien
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['add_alien'])) {
   try {
        $stmt = $pdo->prepare("CALL c24elipe.sp_app_create_alien(?, ?, ?, ?, ?, ?)");
        $stmt->execute([
            $_POST['PNR'], $_POST['Ras'], $_POST['Hemplanet'],
            $_POST['KändaNamn'], $_POST['ÄrRegistrerad'], $_POST['Farlighet']
        ]);
        echo "<p>Alien skapad!</p>";
    } catch (PDOException $e) {
        echo "<p>Fel: {$e->getMessage()}</p>";
    }

    header("Location: " . $_SERVER['PHP_SELF']);
    exit();
}

function getAlienRacesOptions($pdo) {
    $options = '';
    try {
        $stmt = $pdo->query("SELECT RasNamn FROM c24elipe.Ras;");
        $races = $stmt->fetchAll(PDO::FETCH_ASSOC);
        foreach ($races as $race) {
            $options .= '<option value="'.htmlspecialchars($race['RasNamn']).'">'.htmlspecialchars($race['RasNamn']).'</option>';
        }
    } catch (PDOException $e) {
        echo "<p>Fel vid hämtning av raser: {$e->getMessage()}</p>";
    }
    return $options;
}


$aliens = [];
try {
    $stmt = $pdo->query("SELECT * FROM c24elipe.wv_Alien_Ras_Full;");
    $aliens = $stmt->fetchAll(PDO::FETCH_ASSOC);
} catch (PDOException $e) {
    echo "<p>Fel vid hämtning av aliens: {$e->getMessage()}</p>";
}
?>
<h2>Aliens</h2>
<table border="1">
    <tr><th>PNR</th><th>Alias</th><th>Ras</th><th>Farlighet</th></tr>
    <?php foreach ($aliens as $a): ?>
        <?php
        if (isset($_GET['PNR']) && $_GET['PNR'] === $a['PNR']) {
            echo '<tr class="highlighted-row">';
        }
        else
        {
            echo '<tr>';
        }
        ?>

            <td><?=htmlspecialchars($a['PNR'])?></td>
            <td><?=htmlspecialchars($a['Alias'])?></td>
            <td><?=htmlspecialchars($a['RasNamn'])?></td>
            <td><?=htmlspecialchars($a['Farlighet'])?></td>
        </tr>
    <?php endforeach; ?>
</table>

<h3>Lägg till ny Alien</h3>
<form method="POST">
    <input name="PNR" placeholder="PNR" required>
    <select name="Ras" placeholder="Ras" value="Okänd">
        <?= getAlienRacesOptions($pdo) ?>
    </select>

    <input name="Hemplanet" placeholder="Hemplanet">
    <input name="KändaNamn" placeholder="Kända namn">
    <select name="ÄrRegistrerad"><option value="1">Ja</option><option value="0">Nej</option></select>
    <select name="Farlighet">
        <option value="Harmlös">Harmlös</option>
        <option value="Halvt harmlös">Halvt harmlös</option>
        <option value="Ofarlig">Ofarlig</option>
        <option value="Neutral" selected="selected">Neutral</option>
        <option value="Svagt farlig">Svagt farlig</option>
        <option value="Farlig">Farlig</option>
        <option value="Extremt farlig">Extremt farlig</option>
        <option value="Spring för livet">Spring för livet</option>
    </select>
    <button name="add_alien">Lägg till</button>
</form>

<style>
    .highlighted-row {
        background-color: yellow;
    }
</style>

<?php if (isset($_GET['PNR'])){ ?>
    <script>
        document.addEventListener("DOMContentLoaded", function() {
            let element = document.querySelector(".highlighted-row");
            if (element) {
                element.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        });
    </script>

<?php } ?>