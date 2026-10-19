# Next Station Game Code

This repository contains the code for the Unity Game "Next Station". You can play the game on [itch.io](https://leszczyna.itch.io/next-station).

## Architecture Overview

The game is based around a concept of a [`GameModel`](Scripts/Core/Model/GameModel.cs). It stores all state related to the current run. The model, and thus all game state, is serializable. This makes game saves and undo/redo easy. 
The model is only intended to be mutated by a select few, most notably [`BasicGameMode`](Scripts/Core/GameModes/Basic/BasicGameMode.cs). `BasicGameMode` along with its subcomponents contains most of the game logic. 

You could think of the game as being turn based, as most logic only ever takes place when the player makes a move.

The visual layer is cleanly separated from the core. Game Logic knows nothing of the visuals, and visuals are only reading the model/supplementary data. See [`Visuals/`](Scripts/Visuals/) directory.

## How to read

I recommend you start with [`GameplayInput`](Scripts/GameplayInput.cs) which is where most of the user input is handled.
The `GameplayInput.HandleTilePlacement` method will be of most interest to you, and will naturally lead you to...

The [`Game`](Scripts/Game.cs) class. You will notice that is the authority on game's state, and a global access points to all important systems. `Game.PlaceTile` is the game logic's central point, which delegates the game logic to...

The [`BasicGameMode`](Scripts/Core/GameModes/Basic/BasicGameMode.cs)`.UpdateGameModel` method. It houses all of the game logic, by delegating to its subsystems.

## Credits

All game code has been written by me. External references were used; 3rd party code is marked with appropriated comments.

## AI usage

AI has only been used to author a small amount of the more boring unit tests (like equality).