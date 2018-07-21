#region File Information
/*
 * Copyright (C) 2016-2018 David Rudie
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02111, USA.
 */
#endregion

namespace Winter
{
    using System;
    using System.Net;
    using System.Reflection;
    using SimpleJson;

    public static class CheckVersion
    {
        private static string json = string.Empty;

        public static bool IsNewVersionAvailable()
        {
            DownloadJson();

            if (!string.IsNullOrEmpty(json))
            {
                dynamic jsonSummary = SimpleJson.DeserializeObject(json);

                if (jsonSummary != null)
                {
                    var latestVersionOnGithub = jsonSummary.tag_name.ToString().Replace("v", string.Empty);

                    Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                    Version latestVersion = null;

                    Version.TryParse(latestVersionOnGithub, out latestVersion);

                    int comparison = latestVersion.CompareTo(currentVersion);

                    // if comparison = -1 then latestVersion is older than currentVersion
                    // if comparison =  0 then latestVersion is the same as currentVersion
                    // if comparison =  1 then latestVersion is newer than currentVersion
                    if (comparison > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void DownloadJson()
        {
            using (WebClient webClient = new WebClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                try
                {
                    webClient.Encoding = System.Text.Encoding.UTF8;
                    webClient.Headers.Add("Accept: application/vnd.github.v3+json");
                    webClient.Headers.Add("User-Agent: Snip");

                    var downloadedJson = webClient.DownloadString("https://api.github.com/repos/dlrudie/snip/releases/latest");

                    if (!string.IsNullOrEmpty(downloadedJson))
                    {
                        json = downloadedJson;
                    }
                }
                catch (WebException)
                {
                    // Fail silently.
                    json = string.Empty;
                }
            }
        }
    }
}