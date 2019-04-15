# SimpleWhitelist
A super simple steamid-based whitelist resource that you can use to keep random people off of your FiveM server.

### Who is this for?
This is great for when you are running a small server for your friends or just need a server to test something on, and you don't want outsiders on it. 

There are other scripts/classes out there that basically do the same, but all of them require either some form of a database, fiddling with JSON files or having to restart the resource after making a change.

Also this is written in C#, I don't think I have seen one written in C#.

### How does it work?
This resource lets you add/remove people through RCON so you can add/remove on the fly, and saves it in plain text on your server.

### Commands

Allow someone on the server\
`whitelist.add [steamid]` 

Remove someone from the server\
`whitelist.remove [steamid]` 

Show Id's allowed on the server\
`whitelist.list`

Show a help message in the console\
`whitelist`


### Requirements
Requires FXServer 1145 or later due to using .net standard 2.0\
You also need an RCON connection or direct console access to your server.
