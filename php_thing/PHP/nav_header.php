<?php
if (session_status() === PHP_SESSION_NONE) {
    session_start();
}
$username = $_SESSION['username'] ?? '';
$role = $_SESSION['role'] ?? '';

if (empty($username) || empty($role)) {
    echo "Du är inte inloggad. Vänligen <a href='login.php'>logga in</a>.";
}
else {
?>

<nav>
    Inloggad som <b><?= htmlspecialchars($username) ?></b> (<?= htmlspecialchars($role) ?>) |
    <a href="index.php">Start</a> |
    <a href="aliens.php">Aliens</a> |
    <?php if ($role != 'agent'): ?>
        <a href="incidents.php">Incidenter</a> |
    <?php endif; ?>
    <a href="reports.php">Rapporter</a> |
    <?php if (in_array($role, ['group_leader', 'admin'])): ?>
        <a href="archive.php">Arkiv</a> |
    <?php endif; ?>
    <a href="logout.php">Logga ut</a>
</nav>

<?php
}
?>