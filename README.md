# Pi3D Mini-project
Pikmin but Worse

## Overview of the Game
The game is a very bare bones version of the Pikmin game series. The objective of the game is to find and collect treasure, with the help of the pikmin, which follow you around. The player manages the pikmin by calling on them and throwing them at the treasures. Each pikmin has different abilities or weaknesses, which the player has to use/work around. The game is won when all treasures are collected, or lost when the needed pikmin are dead.  

### Main Hierarchy
- Camera - 3rd person camera which follows the player and cannot be rotated.
- Player - Player character controlled by WASD. Can interact with pikmin.
- Pikmin - NPC’s that have a variety of interactions with both the player, treasures etc. the game is centered around what you have them do.
- Treasures - Large objects that are hidden on the map. When enough pikmin are trying to carry one it will be moved towards the players base.
- Terrain - An enclosed map made using the terrain tools, with different challenges built into the terrain.
- UI - The UI keeps track of how many pikmin are on themap, how many are following the player and which pikmin is closest. Also shows how many treasures have been collected.
- Managers - Soundmanager, which starts the background music. Interactionmanager, keeping track of all the mouse and keyboard interactions (except WASD).

### Features in the game
- Explore the terrain and find treasure and pikmin.
- Get pikmin to follow the player using the mouse or by colliding with them.
- Throw the pikmin that are following the player and get them to carry treasure.
- Figure out their strengths/weaknesses.

## Project Parts

### Scripts
- Game Manager - Keeps track of all the UI and the progress in the game. 
- Game Scene manager - Is used in Start and End scenes to change scenes via buttons.
- Pikmin Behavior - Controls the states of each pikmin, using an enumerator, colliders and other ways of interacting with each pikmin. Uses NavMesh agent and rigid body to control the behavior of the pikmin.
- Player Interaction - Useus action inputs, primarily for checking mouse input. Displays a cursor on the ground and is used for almost all the interaction with pikmin. Such as calling them and throwing them.
- Player Movement - Controls the player movement using player controller and action input. Also controls the movement of the camera. 
- Sound Manager - Only plays the background music.
- Treasure - Has a dictionary of positions that pikmin take when carrying the treasure, and when all are taken, the treasure uses a Nav Mesh agent to move to the player base. 

### Graphics
- The terrain was made using Unity’s built in tearran tools.
- The UI was made by me and made to look like the general Pikmin UI.
- The pikmin were 3D modeled by me, and are made to look exactly like in the original

### Time Spent
| **Task**                                                                | **Time it Took (in hours)** |
|--------------------------------------------------------------------------------|------------------------------------|
|     Terrain                                                                    |     3                              |
|     Pikmin modeling                                                            |     4                              |
|     Pikmin following behavior                                                  |     4                              |
|     Camera behavior                                                            |     2                              |
|     Player movement                                                            |     1.5                            |
|     Cursor projection                                                          |     2                              |
|     Pick up pikmin                                                             |     (10 min)                       |
|     Throw pikmin                                                               |     4                              |
|     Call pikmin                                                                |     3                              |
|     Treasure Collection (lots of trial and error)                              |     7                              |
|     Pikmin differences (dying in water and jumping higher)                     |     4                              |
|     UI                                                                         |     2                              |
|     Switch pikmin                                                              |     3                              |
|     Dismiss pikmin                                                             |     1                              |
|     Player model (unfinished unfortunately)                                    |     7                              |
|     Lose/win conditions                                                        |     4                              |
|     Optimizing and reorganizing                                                |     6                              |
|     Writing Report                                                             |     5                              |
|     **All**                                                                    |     **62 (and 10 min)**            |


## References
- [3rd person camera and player controls](https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-updates-in-new-charactercontroller-pa-196526)
- [Terrain Samples](https://assetstore.unity.com/packages/3d/environments/landscapes/terrain-sample-asset-pack-145808)
- [Tutorial on how to make flowers in blender](https://www.youtube.com/watch?v=3AkGLcu6gUU)
- Chat GPT was used as guidance when lost.
- OST form Pikmin was used. 
