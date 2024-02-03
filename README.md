# Custom Ships

## About

Build and customize ships out of a variety of different ship pieces.


## Features

- Ships of varying sizes and shapes can be constructed
- Hull pieces snap and scale to existing placement, allowing for smooth curves with few base pieces
- Decoration like colored shields can be attached to personalize ships
- Multiple sails can be placed to increase ship speed, with diminishing returns
- Crates and barrels can be placed for storage


### Screenshots

<img src="https://raw.githubusercontent.com/MSchmoecker/CustomShips/master/Docs/small_ship.png" width="49%" alt="Small  Ship"/> <img src="https://raw.githubusercontent.com/MSchmoecker/CustomShips/master/Docs/medium_ship.png" width="49%" alt="Medium Ship"/>

<img src="https://raw.githubusercontent.com/MSchmoecker/CustomShips/master/Docs/ship_construction.png" width="49%" alt="Ship Construction"/> <img src="https://raw.githubusercontent.com/MSchmoecker/CustomShips/master/Docs/ship_pieces.png" width="49%" alt="Ship Pieces"/>


## Caveats

- snappoints are encouraged to be used, otherwise some behaviour doesn't work properly
- only a ship with one rudder can be steered, as it determinants the ships forward direction
- placing a rudder can sometimes turn the ship into a different direction


## Planned Features

The mod has gone through different implementation iterations already and is now at a good state to be released.
The most important pieces have been added to build a ship, the next big update will likely bring more decoration and a second deck to build on.


## Compatibility

The ships behave mostly like regular vanilla ship and only update their stats at runtime.
The pieces are specifically made for the ship and don't allow vanilla or modded pieces, decreasing the possibility of unwanted interactions.
Therefore most other ship related mods should be compatible too, please let me know if there is a conflict somewhere.

PlanBuild is not fully compatible, ships build with it have no behavior or physics.


## Manual Installation

This mod requires [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) and [Jötunn](https://valheim.thunderstore.io/package/ValheimModding/Jotunn/).
Extract all files to `BepInEx/plugins/CustomShips`

The mod must be installed on the server and all client.
It is ensured that all players have the mod installed with the same mod version, otherwise no connection is possible.


## Links

- [Thunderstore](https://valheim.thunderstore.io/package/MSchmoecker/CustomShips/)
- [Nexus](https://www.nexusmods.com/valheim/mods/2653)
- [Github](https://github.com/MSchmoecker/CustomShips)
- Discord: Margmas. Feel free to DM or ping me about feedback or questions, for example in the [Jötunn discord](https://discord.gg/DdUt6g7gyA)
