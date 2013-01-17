SNIP V2.0.1
=========
Copyright 2012, 2013 David Rudie (d.rudie@gmail.com)

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
