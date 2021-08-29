# Phat-Pong-Game
Programming test for Phat Loot Studios

Best to run the game in 1280x720 display as the NetworkManagerHUD isn't very responsive.

The NetworkStartPosition components are used to load players to specified spawn locations. These spawn points are selected in a round-robin fashion thus will populate player 1 at the bottom and player 2 above that for the first time the 2 players connect.

If a non-host player disconnects and reconnects, they can spawn at the same location as the host but I didn't think it was necessary to handle this for the test.

Movement is client authoritative since it feels bad when latency affects controls. I didn't do any server validation checks for any client movement or boundary collision so this would be cheatable.

Only the host can restart the game.

Ball has some noticeable latency issues due to it being controlled server side. To reduce it I'd likely let the client control the initial location of the ball (whilst player is in possession of the ball) and once launched, let the server manage it. This would hide the most obvious displays of lag.

I left cross-player collision on since it felt more interesting (balls can collide with other balls and other players paddles).
