SNIP V3.1.0
=========
Copyright 2012, 2013, 2014 David Rudie <d.rudie@gmail.com>
Project Page: https://github.com/dlrudie/Snip

ABOUT
=====
This is a small application that sits in the system tray and updates a text
file with the currently playing audio track.

It supports Spotify, iTunes, and Winamp.

If you choose to use iTunes then Snip will automatically launch iTunes.  This
is the behavior of the COM API and there's nothing I can do about it.  If you
only use Spotify then you don't have to worry about it.

To switch between players just right-click on the icon in the system tray.

Snip will write a generic format output to a file called Snip.txt within the
same folder as Snip.exe.  If you choose to save information to separate files
then the files will be called Snip_Arist.txt, Snip_Track.txt, Snip_Album.txt,
and if you choose to save track history then it will be saved to a file
called Snip_History.txt.

WINAMP
======
For Winamp support to work properly you must change some options.

Make sure that when you install Winamp you enable "Global Hotkey Support"
under "User Interface Extensions" or hotkeys will not work.

Inside Winamp open up the options window.

* Under General Preferences make sure "Show the playlist number in the Windows
    taskbar" is disabled.

* Under General Preferences->Global Hotkeys make sure "Enable default
    multimedia key support" is enabled.

* Under General Preferences->Titles make sure "Use advanced title formatting
    when possible" is enabled.

Then you need to edit the formatting to look like this:
%title% – %artist%

Note
----
The hyphen used is an En Dash character!  Snip uses this character internally
to split the text apart so make sure you use this En Dash character or Snip
will not read it correctly!

HOTKEYS
=======
* Next Track: Ctrl, Alt, ]
* Previous Track: Ctrl, Alt, [
* Volume Up: Ctrl, Alt, +
* Volume Down: Ctrl, Alt, -
* Mute Track: Ctrl, Alt, M
* Play/Pause Track: Ctrl, Alt, Enter
* Stop Track: Ctrl, Alt, Backspace

Note
----
Not all hotkeys work between Spotify, iTunes, and Winamp.

CHANGES
=======

v3.1.0 (2014-xxx-xx):
* Renamed Cache Spotify Album Artwork to Keep Spotify Album Artwork.
* Added French translation.  Thanks, MindFlaw!
* Added Dutch translation.  Thanks, Azeirah!
* Resized the Output Format form to be wider to accomodate languages that
    might require more room to display text.

v3.0.1 (2013-Dec-30):
* Manifest file modified to require Administrator rights.
* Added a link to the project on GitHub to the README.txt file.
* Updated the strings file with detailed descriptions to hopefully help
    translators understand what everything is for.

v3.0.0 (2013-Dec-22):
* Added the ability to select album artwork resolution for Spotify.  The
    default size is Tiny since you really shouldn't need much larger for
    displaying the image on your stream.  However, the choice is there.
    Tiny = 60x60
    Small = 120x120
    Medium = 300x300
    Large = 640x640
* Strip " - Explicit Album Version" from song titles as it messes up the
    search results.
* Rather than take the first result provided from the search with Spotify it
    will now loop through the results until it finds a title that matches
    what's playing.  Results are still not 100% because the song title shown
    inside Spotify isn't always the result you get with their search.
* StyleCop dropped in favor of Code Analysis built in to MSVC.
* Unused code removed.
* Memory leaks fixed.
* Globalization support added to make it easy to translate and use in
    languages other than English.  (Feel free to e-mail me about this).
* Removed warning about the application already running.  It will just not do
    anything now.
* Updated SimpleJson to the latest version.
* Removed user csproj file from repository.

v2.8.0 (2013-Dec-21):
* Snip will download higher resolution album artwork using a more efficient
    method.  This patch was provided by Kailan Blanks.  Thank you.
* Added an option to cache Spotify album artwork locally.  If this option is
    enabled then it will download and save the artwork locally to a separate
    directory, and then each time that track is played it will use the saved
    artwork instead of redownloading it.
* Due to the contribution I added an AUTHORS.txt file.
* Split structs and classes into regions.
* I forced the download timeout to 5 seconds so that there's not too much of a
    wait between the time the album artwork starts downloading and the text
    file is updated.

v2.7.0 (Unreleased):
* Moved program information to its own class.
* Moved unsafe native methods to their own class.
* Made StyleCop mostly happy.
* Moved key binds to their own class.
* Moved key states to their own class.
* Moved media player fields to their own class.
* Snip.txt empties when no track is playing or other generic information is
    shown.

v2.6.0 (2013-Apr-20):
* Added SimpleJson as a way to parse json easily.
* Changed project from .Net Framework 4 Client Profile to .Net Framework 4.
* Added a reference to System.Web so HttpUtility.UrlEncode() could be used.
* Changed the way album artwork and track information are discovered with
    Spotify.  It no longer tries to get the exact track ID within Spotify's
    memory.  It will now query the Spotify API on the web using the artist
    name and track title.  If a match is found it will pull the track ID from
    there and then query the regular Spotify HTML page for artwork.
* The change above allowed me to remove a lot of unnecessary code, including
    ReadProcessMemory(), GetWindowThreadProcessId(), and FindInMemory().
* Removed some unused WindowMessage commands.

v2.5.0 (2013-Apr-20):
* Changed the readonly values to consts and renamed them.
* Embedded the blank image as a byte array instead of having a separate
    Snip_Blank.jpg.
* Created a SaveBlankImage() method that saves the blank image in case of any
    error.
* Changed the way I get Spotify album artwork.  It now uses the regular
    Spotify HTML page instead of the embed page.  This provides a lower
    resolution image than previously but it works.
* Added a ImageFromByteArray() method that converts a byte array to an Image.

v2.0.1 (2013-Jan-17):
* Fixed streams in Winamp causing Snip to crash.

v2.0 (2013-Jan-17):
* Prior change in v1.6.5 that I forgot to mention:
    Snip now defaults to have most options disabled by default.
* Winamp support added.
* Added the ability to save album information (currently only iTunes supports
    this).
* Volume up/down hotkeys now work for iTunes.
* The separator now has spaces and the code no longer inserts its own spaces.

v1.6.5 (2013-Jan-4):
* Fixed a bug preventing output format working with iTunes.
* Added code to make sure only one instance of Snip can run at once.

v1.6 (2013-Jan-1):
* Added the ability to set a custom output format.
    If you save the track and artist information to separate files then make
    sure the separator that you use does NOT appear in any track or artist
    title.  If it does then the text will not get separated correctly.
* Expanded the memory range that Snip searches for the track information.

v1.5 (2012-Nov-11):
* Snip_Artist.txt and Snip_Track.txt will now be written.  These can be used
    to display the artist or track name in different styles.
* Added support for saving image artwork.  Spotify's support is rather hacky
    so it may not always work, or it may cause Spotify to randomly stop
    playing music.

v1.1.1 (2012-Sep-14):
* Fixed Snip_History.txt to use Environment.NewLine instead of \n.

v1.1 (2012-Sep-14):
* Added support for saving track history.
    This will save the history to a new text file called Snip_History.txt.

v1.0 (2012-Aug-12):
* Initial Release
