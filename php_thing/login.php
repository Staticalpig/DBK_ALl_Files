<?php

require 'PHP/db_login.php';
session_start();

if (isset($_SESSION['role'])) {
    header("Location: index.php");
    exit;
}

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    try {
        $pdo = login_db();
    } catch (Exception $e) {
        $error = "Databasanslutning misslyckades: " . $e->getMessage();
        exit;
    }
    $username = $_POST['username'];
    $password = $_POST['password'];

    $stmt = $pdo->prepare("CALL sp_get_agent_login(?)");
    $stmt->execute([$username]);
    $row = $stmt->fetch(PDO::FETCH_ASSOC);

    $stmt->closeCursor(); // Stäng

    if ($row && password_verify($password, $row['Lösenord'])) {
        try {
            $pdo = login_db(); // RESET
        } catch (Exception $e) {
            $error = "Databasanslutning misslyckades: " . $e->getMessage();
            exit;
        }

        $loginInfo = $pdo->prepare("CALL c24elipe.sp_give_info_to_login(?);");
        if ($loginInfo === false) {
            $error = "Misslyckades att hämta användarinformation!";
            exit;
        }

        $loginInfo->execute([$username]);
        $u = $loginInfo->fetch(PDO::FETCH_ASSOC);

        $role = strtolower($u['Avdelning']); // admin / group_leader / agent etc

        switch ($role) {
            case 'admin':
                $_SESSION['db_user'] = 'c24elipe_admin';
                $_SESSION['db_pass'] = 'adminPassword123!';
                break;
            case 'group_leader':
                $_SESSION['db_user'] = 'c24elipe_groupleader';
                $_SESSION['db_pass'] = 'GroupPass123';
                break;
            default:
                $_SESSION['db_user'] = 'c24elipe_agent';
                $_SESSION['db_pass'] = 'AgentPass123';
        }

        $_SESSION['username'] = $username;
        $_SESSION['fornamn'] = $u['Fornamn'];
        $_SESSION['role'] = $role;

        $loginInfo->closeCursor();

        header("Location: index.php");
        exit;
    } else {
        $error = "Felaktigt användarnamn eller lösenord!";
    }
}
?>
<?php include_once 'static/html/header.php'; ?>
<body id="blue-background">
    <div id="site__center">
        <a href="login.php" class="logo-image">
            <img src="static/img/PUKO_LOGO_fixed.png" alt="PUCKO Logo">
        </a>
        <form method="POST" id="login-form" autocomplete="off">
            <h2>Login</h2>
            <div>
                <?php if (!empty($error)) echo "<p>$error</p>"; ?>
            </div>
            <label for="username">Username</label>
            <input type="text" id="username" name="username" required autofocus placeholder="Username">
            <label for="password">Password</label>
            <input type="password" id="password" name="password" required placeholder="Password">
            <button type="submit">Login</button>
        </form>
    </div>
</body>