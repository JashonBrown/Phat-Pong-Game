# Phat-Pong-Game
Programming test for Phat Loot Studios

Best to run the game in 1920x1080 display as the NetworkManagerHUD isn't very responsive.

The NetworkStartPosition components are used to load players to specified spawn locations. These spawn points are selected in a round-robin fashion thus will populate player 1 at the bottom and player 2 above that for the first time the 2 players connect.

If a non-host player disconnects and reconnects, they can spawn at the same location as the host but I didn't think it was necessary to handle this for the test.

Movement is client authoritative since it feels bad when latency affects controls. I didn't do any server validation checks for any client movement or boundary collision so this would be cheatable.

TODO: Only the host can restart the game
