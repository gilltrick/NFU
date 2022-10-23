# NFU
Networking for Unity (Version 0.2)

How does it work?

1) Download files 
2) Start Server
3) Import Package in 3D Project in Unity
4) Press Play<br><br>
  $ git clone https://www.github.com/gilltrick/nfu<br>
  $ cd nfu/SERVER<br>
  $ dotnet run<br>

<b>Attention:</b><br>
The Scripts inside the UNITYSCRIPTS folder are not working for the base project
They are compatible with the server inside the GAMESERVER folder.

Those files are used in a current project and will be updatet when i make a update for my game.

<b>Changes:</b><br>
Now the rotation and PlayerAction get transmitted as well.
Rotation is like position. PlayerAction is new and can be used to transmitt custom data for animation, tradeing, shooting what ever you want:
