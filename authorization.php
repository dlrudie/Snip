<?php
    if (isset($_GET["client"]) && $_GET["client"] == "SNIP")
    {
        $clientId = "";
        $clientSecret = "";

        $postUrl = "https://accounts.spotify.com/api/token";

        $authorizationKey = base64_encode($clientId.":".$clientSecret);

        $header = "Authorization: Basic $authorizationKey";
        $argument = "grant_type=client_credentials";

        $curl = curl_init($postUrl);
        curl_setopt($curl, CURLOPT_POST, 1);
        curl_setopt($curl, CURLOPT_POSTFIELDS, $argument);
        curl_setopt($curl, CURLOPT_HTTPHEADER, array($header));
        curl_exec($curl);
    }
?>
