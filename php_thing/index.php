<?php
session_start();
include 'PHP/nav_header.php';
?>
<h1>INDEX :)</h1>
<?php
if (!empty($_SESSION['username'])) {
    echo "<p>Välkommen, " . htmlspecialchars($_SESSION["username"]) . " : ".htmlspecialchars($_SESSION['fornamn'])."!</p>";
    echo '<p><a href="logout.php">Logga ut</a></p>';
} else {
    echo '<p><a href="login.php">Logga in</a></p>';
}
?>