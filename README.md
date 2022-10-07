SNIP
====
Copyright 2012-2022 David Rudie <d.rudie@gmail.com>\
Project Page: [Snip](https://github.com/dlrudie/Snip)

ABOUT
=====
This small application sits in the system tray and updates a text
file with the currently playing audio track.

It supports the following media players:
* [Spotify](https://www.spotify.com/)
* [iTunes](https://www.apple.com/itunes/)

If you choose to use iTunes, Snip will automatically launch it. (This
is the behavior of the COM API and there's nothing I can do about it.) If you
only use Spotify, you don't have to worry about it.

To switch between players just right-click on the icon in the system tray.

Snip will write a generic format output to a file called `Snip.txt` within the
same folder as `Snip.exe`.  If you choose to save information to separate files, 
the files will be called `Snip_Artist.txt`, `Snip_Track.txt`, `Snip_Album.txt`, 
and `Snip_History.txt` (track history).

VARIABLES
=========
There are a handful of variables you can use within the Output Settings.

These variables only affect the output of Snip.txt itself.

* $$t = Track Title
* $$ut = TRACK TITLE
* $$lt = track title
* $$a = Track Artist
* $$ua = TRACK ARTIST
* $$la = track artist
* $$l = Track Album
* $$ul = TRACK ALBUM
* $$ll = track album
* $$i = Spotify track ID
* $$n = New line

HOTKEYS
=======
* **Next Track:** Ctrl, Alt, ]
* **Previous Track:** Ctrl, Alt, [
* **Volume Up:** Ctrl, Alt, + (iTunes Only)
* **Volume Down:** Ctrl, Alt, - (iTunes Only)
* **Mute Track:** Ctrl, Alt, M (iTunes Only)
* **Pause Track:** Ctrl, Alt, P
* **Play/Pause Track:** Ctrl, Alt, Enter (Pause iTunes Only)
* **Stop Track:** Ctrl, Alt, Backspace (iTunes Only)

DONATIONS
=========
Snip is free and always will be. However, if you'd like to support Snip
donations are always greatly appreciated and all donations will go right
back into making Snip even better. Thank you!
* [PayPal](https://paypal.me/thedopefish)
* ETH: 0x9f65813778F8Cc0fc1579923b64115998961A127
* CRO: cro1950wnv6fv3xzsk5xlsud7peru9lhlcpwsuhhxp
* BTC: bc1q38htzqlz7w42am8846mvxdza9nl45z3np8p7uv
* Want to send a different coin? Let me know.
