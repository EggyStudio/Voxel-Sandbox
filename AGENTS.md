# Voxel Sandbox

A voxel game in C# on .NET 10, set in an endless generated world whose blocks are placed and
broken, with its light, its water, its mobs and the items they drop. It is built on the 3DEngine
package from nuget.org at the version `VoxelSandbox/VoxelSandbox.csproj` names, and its first use is
to test the engine's global illumination with emissive blocks. The engine is used through its
package alone, its flat API of static calls drawing each frame, and what the game finds the engine
lacks is asked of the engine, never mended in a copy of its code here.

## Controlling the game with `./e3d`

`./e3d` talks to a running program over a local socket, through the engine's command line, out of
the engine checkout `ENGINE_CHECKOUT` names or the folder above this one. **Before asking anything
about the running game (what is in its world, what a key does, what it looks like), check
`./e3d status` and send the live session its commands instead of launching a process per question.**

```bash
./e3d status                          # is anything serving?
./e3d open VoxelSandbox/bin/Debug/net10.0/VoxelSandbox --hidden --transient
                                      # start it, rendering in a window never shown, saving nothing
                                      # (--offscreen renders with no display at all)
./e3d list                            # what it can be asked, the voxel.* commands among it
./e3d command input.key W 30          # input through the engine, not the desktop
./e3d shot /tmp/x.png                 # capture the next frame
./e3d stop
```

The workflow, the commands and the failure modes are in [.github/E3D.md](.github/E3D.md). Read it
before scripting a session.

## Documents

| File | Holds |
|---|---|
| [README.md](README.md) | What the game does, its controls and commands, how it is drawn and what it costs |
| [.github/E3D.md](.github/E3D.md) | Controlling the running game through `./e3d`, with the workflow, every command and the failure modes |
| [.github/REVIEW.md](.github/REVIEW.md) | Direction from the reviewing session, which comes before any other order of work |
| [.github/REVIEW-ARCHIVE.md](.github/REVIEW-ARCHIVE.md) | What REVIEW.md no longer carries, the items done and the verdicts settled, each under its number |
| [.github/NORM.md](.github/NORM.md) | The numbered rules the game keeps, each with what checks it |
| [.github/STYLE.md](.github/STYLE.md) | Rules for every comment, message and Markdown file |
| [.github/COMMITS.md](.github/COMMITS.md) | How and when work is committed |
| [DEVLOG.md](DEVLOG.md) | What landed by day, with the pictures taken on the way, which `.github/devlog` keeps |
| The engine's [.github/ASKS.md](https://github.com/EggyStudio/3DEngine/blob/main/.github/ASKS.md) | What the games built on the engine ask of it, this game's entries under its own heading |

## Building and testing

```bash
build/fetch-slang.sh          # slangc, once per checkout
dotnet build VoxelSandbox.slnx
dotnet test VoxelSandbox.Tests
dotnet run --project VoxelSandbox
build/play.sh                 # a scripted run of the game through ./e3d, as the build workflow's
```

The tests hold the game's logic, its light, meshes, bodies, mobs, items, saves and generation, each
in a small world of its own, and need no window and no GPU. The build workflow runs them on Linux,
Windows and macOS on every push and, on lavapipe, the scripted run of the game in `build/play.sh`.
The pack workflow, run by hand, compiles every shader (`build/shaders.sh`), publishes the game as
native code for each system (`build/publish.sh`) and, with its `publish` input set, makes a GitHub
release of the zips.

## Where things are

| Path | Holds |
|---|---|
| `VoxelSandbox` | The game's program, where `Program.cs` opens the window and runs the frame, `VoxelGame.cs` holds the game's state and its frame, `GameCommands.cs` the `voxel.*` commands, and `Measurements.cs` the recording of how global illumination settles after the camera moves |
| `VoxelSandbox/Blocks` | The blocks, their shapes and the surfaces they are drawn with |
| `VoxelSandbox/Entities` | Mobs, passive and hostile, their bodies, paths, models, arrows and explosions |
| `VoxelSandbox/Generation` | The overworld and the flat world, climate and biomes, noise, ores, caves, ravines, trees, structures, villages and herds |
| `VoxelSandbox/Items` | Items, the inventory, recipes, loot, the stacks on the ground, furnaces and smelting |
| `VoxelSandbox/Player` | The first-person controller, the player's health and hunger, and the ray to the block looked at |
| `VoxelSandbox/Rendering` | The section mesher, the chunk renderer, the visibility graph, the sky, the clouds and the light's settings |
| `VoxelSandbox/Sound` | The game's own sounds, the steps by the ground and the sounds of a place, made as it starts and played through the engine's audio |
| `VoxelSandbox/Ui` | The HUD, the hotbar, the inventory window, item icons and the settings window, drawn with ImGui |
| `VoxelSandbox/World` | Sections, columns, the world, streaming, light levels, water and the save |
| `VoxelSandbox/resources` | The terrain shader, `shaders/terrain.slang`, copied beside the program |
| `VoxelSandbox.Tests` | xUnit tests of the game's logic, with `Laid.cs` laying out the small worlds they run in |
| `build` | The Slang compiler's fetch scripts, the version, and the scripts the workflows run to play, compile the shaders for and publish the game |

## Conventions

- Comments explain **why**, in prose. Match the surrounding density rather than trimming them.
- A public member whose name does not say what it does carries an XML summary, with a `<remarks>`
  section where there is reasoning or a trap to record.
- The game does not reflect at runtime where a generator can emit the code instead, so that a
  native build, which keeps only what the compiler sees used, runs as the Debug build does.
- There is no editor application. Tools are ImGui windows the game draws in its own frame, as its
  settings window is.
- A `[Command]` method is a console command, an `e3d command` verb and a line in `e3d list` at
  once. Adding one is writing one.
- The engine is the package `VoxelSandbox/VoxelSandbox.csproj` names, from nuget.org. Where the
  game finds the engine falls short, a cost it cannot afford or a thing it cannot do, an entry goes
  in the engine's `.github/ASKS.md` under this repository's name, with what was measured and how
  (`./e3d command profile`), written in the engine checkout, and nothing else of the engine's is
  edited from here.
- Prose in this repository follows `.github/STYLE.md`, which governs comments, XML documentation,
  messages and Markdown. Read it before writing any of them.
- Each finished batch of work is committed on `main` locally and never pushed from here, since the
  reviewing session pushes `main` once the previous build run has ended, with a message whose
  subject is three invisible marks and whose description is one plain sentence. Before each commit,
  `.github/STYLE.md` is read and applied to what is staged, the message included.
  `.github/COMMITS.md` has the exact form, that pass, and how to split a batch that shares a file
  with another.
- `.github/REVIEW.md` is read before a batch is started and before each commit. A second session
  writes it after reading the code and the history, and its Now list comes before any other order
  of work. Only its Replies section is edited here, for an item that is disputed, blocked or done,
  and the file is committed with whichever batch comes next.
- `.github/NORM.md` holds the rules the game keeps, each with a number, a reason and what checks
  it, and is written by the session that writes REVIEW.md. A rule read as wrong, or a fault of a
  kind no rule names, is said with a line under Replies in REVIEW.md beginning `Rule:`. A row that
  the table under Where things are lacks is added here when N 1.5 asks for it, and nothing else in
  this file changes without the owner's word (N 7.4).
- The game this repository held before, a voxel sandbox on Direct3D 12, is on the `legacy`
  branch. It is read for reference and not merged back.
