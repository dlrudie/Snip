SNIP
====
Copyright 2012-2018 David Rudie <d.rudie@gmail.com>
Project Page: [Snip](https://github.com/dlrudie/Snip)

ABOUT
=====
This small application sits in the system tray and updates a text
file with the currently playing audio track.

It supports the following media players:
* [Spotify](https://www.spotify.com/)
* [iTunes](https://www.apple.com/itunes/)
* [Winamp](http://www.winamp.com/)
* [foobar2000](http://www.foobar2000.org/)
* [VLC](http://www.videolan.org/vlc/)
* [Google Play Music Desktop Player](https://www.googleplaymusicdesktopplayer.com/)
* [Quod Libet](https://quodlibet.readthedocs.io/)

If you choose to use iTunes, Snip will automatically launch it. (This
is the behavior of the COM API and there's nothing I can do about it.) If you
only use Spotify, you don't have to worry about it.

To switch between players just right-click on the icon in the system tray.

Snip will write a generic format output to a file called `Snip.txt` within the
same folder as `Snip.exe`.  If you choose to save information to separate files, 
the files will be called `Snip_Artist.txt`, `Snip_Track.txt`, `Snip_Album.txt`, 
and `Snip_History.txt` (track history).

SUPPORTED FEATURES FOR PLAYERS
------------------------------
* **Spotify:** Artist, Track, Album, Artwork, Output Formatting
* **iTunes:** Artist, Track, Album, Artwork, Output Formatting
* **Winamp:** Artist, Track
* **foobar2000:** Artist, Track
* **VLC:** Nothing (It uses whatever the titlebar says)
* **Google Play Music Desktop Player:** Artist, Track, Album, Artwork, Output
    Formatting
* **Quod Libet:** Nothing (It uses whatever the titlebar says)

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
`%title% – %artist%`

Note
----
The hyphen used is an en dash character!  Snip uses this character internally
to split the text apart so make sure you use this en dash character or Snip
will not read it correctly!

FOOBAR2000
==========
For foobar2000 support to work properly you must change some options.

Inside foobar2000 open up the preferences window.

Expand the Display options and select Default User Interface.  At the bottom
of the window you will need to change "Window title" to look like this:
`%title% – %artist%`

Note
----
The hyphen used is an en dash character!  Snip uses this character internally
to split the text apart so make sure you use this en dash character or Snip
will not read it correctly!

VLC
===
Snip looks for the window class and "VLC media player" window title to
determine if VLC is running.  If the title bar is anything different then it
will not be found.  If a track is currently playing when you start Snip you
just have to push stop and Snip will find VLC.

QUOD LIBET
==========
Snip looks for the window class "quodlibet" to determine if Quod Libet is
running.  Once the window is found it will just read the titlebar and output
the text in it.

GOOGLE PLAY MUSIC DESKTOP PLAYER
================================
Snip utilizes GPMDP's JSON API. Make sure "Enable JSON API" has a checkmark
next to it under Desktop Settings.

HOTKEYS
=======
* **Next Track:** Ctrl, Alt, ]
* **Previous Track:** Ctrl, Alt, [
* **Volume Up:** Ctrl, Alt, +
* **Volume Down:** Ctrl, Alt, -
* **Mute Track:** Ctrl, Alt, M
* **Pause Track:** Ctrl, Alt, P
* **Play/Pause Track:** Ctrl, Alt, Enter
* **Stop Track:** Ctrl, Alt, Backspace

Note
----
Not all hotkeys work between Spotify, iTunes, Winamp, foobar2000, VLC, and
Google Play Music Desktop Player.

DONATIONS
=========
Snip is free; however, I've received a lot of requests from people wishing
to donate. If you'd like to donate it's entirely up to you. You may donate
here: [PayPal](https://paypal.me/thedopefish)
