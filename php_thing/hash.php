<?php


if (!empty($_GET['password'])) {
    $password = $_GET['password'];

    $hashed_password = password_hash($password, PASSWORD_DEFAULT);

    echo "Lösenord: " . htmlspecialchars($password) . "<br>";
    echo "Hashat lösenord: " . htmlspecialchars($hashed_password) . "<br>";

    if (password_verify($password, $hashed_password)) {
        echo "Lösenordet matchar hashen!";
    } else {
        echo "Lösenordet matchar inte hashen!";
    }
}

?>

<!doctype html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport"
          content="width=device-width, user-scalable=no, initial-scale=1.0, maximum-scale=1.0, minimum-scale=1.0">
    <meta http-equiv="X-UA-Compatible" content="ie=edge">
    <title>Document</title>
</head>
<body>
<form method="GET">
    <label for="password">PASSOWRD:</label>
    <input type="text" name="password" id="password">
    <input type="submit" value="Submit">
</form>
</body>
</html>
