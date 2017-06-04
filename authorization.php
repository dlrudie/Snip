<?php

if (isset($_GET["client"]) && $_GET["client"] == "SNIP")
{
    $expiration = 3600;
    $tokenFile = "token.json";

    if (file_exists($tokenFile))
    {
        $modTime = filemtime($tokenFile);
        $changed = time() - ($modTime - 1); // -1 to account for file starting at 0

        if ($changed >= $expiration)
        {
            ObtainToken($tokenFile);
        }
        else
        {
            echo file_get_contents($tokenFile);
        }
    }
    else
    {
        ObtainToken($tokenFile);
        echo file_get_contents($tokenFile);
    }
}

function ObtainToken($tokenFile)
{
    $clientId = ""; // Set your client ID here
    $clientSecret = ""; // Set your client secret here

    $postUrl = "https://accounts.spotify.com/api/token";

    $authorizationKey = base64_encode($clientId.":".$clientSecret);

    $header = "Authorization: Basic $authorizationKey";
    $argument = "grant_type=client_credentials";

    $curl = curl_init($postUrl);
    curl_setopt($curl, CURLOPT_POST, 1);
    curl_setopt($curl, CURLOPT_POSTFIELDS, $argument);
    curl_setopt($curl, CURLOPT_HTTPHEADER, array($header));
    curl_setopt($curl, CURLOPT_RETURNTRANSFER, 1);
    $response = curl_exec($curl);

    file_put_contents($tokenFile, $response);
}

?>
