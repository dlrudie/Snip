SNIP
====
Copyright 2012-2014 David Rudie <d.rudie@gmail.com>
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

FOOBAR2000
======
For foobar2000 support to work properly you must change some options.

Inside foobar2000 open up the preferences window.

Expand the Display options and select Default User Interface.  At the bottom
of the window you will need to change "Window title" to look like this:
%title% – %artist%

Note
----
The hyphen used is an En Dash character!  Snip uses this character internally
to split the text apart so make sure you use this En Dash character or Snip
will not read it correctly!

HOTKEYS
=======
* **Next Track:** Ctrl, Alt, ]
* **Previous Track:** Ctrl, Alt, [
* **Volume Up:** Ctrl, Alt, +
* **Volume Down:** Ctrl, Alt, -
* **Mute Track:** Ctrl, Alt, M
* **Play/Pause Track:** Ctrl, Alt, Enter
* **Stop Track:** Ctrl, Alt, Backspace

Note
----
Not all hotkeys work between Spotify, iTunes, Winamp, and foobar2000.
