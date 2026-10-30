# Item 142, the game as it is run by hand

The game run as `dotnet run` runs it, Debug, in a shown window on the development machine, with
the player's options file and packs copied into a scratch data folder, against offscreen captures of
the same views, on tuned's power-saving profile with the processor's energy preference at power.

## The device and the window

The log of a plain windowed `dotnet run` names the NVIDIA GeForce RTX 4070 Laptop GPU, a discrete
GPU, on SDL's x11 driver, a swapchain of `B8G8R8A8Unorm` and the present mode Immediate. No variable
of the environment picks a Vulkan driver. The window opened at 2560 by 1440 from its options file,
and the swapchain stayed 1280 by 720 from the loading screen's first frame at 2.35 s until the
first present after the load at 10.86 s, an entry of the engine's ASKS.md.

## The options file

The player's file, written on 2026-10-26, holds every setting. Against the defaults it keeps Soft
Shadows at 0 where the default is 1.5, which the player did not choose, the window at the monitor's
fifth mode, 2560 by 1440, and the shadows at 128 blocks with the far cascade on, the defaults of
item 141. The packs are the three default packs, as the captures' are.

## Window against capture

The plains village and the cherry grove at hour 16 and the spawn's open view at noon, the frame
settled after 600 frames, the window's frame read by `./e3d shot` against an offscreen run's of the
same view at the same size and options. The root mean square of their difference is 0.0043 to
0.0070 of the full range with the player's options at 2560 by 1440 and 0.0009 to 0.0020 with a fresh
file at 1280 by 720, the clouds' edges most of it. The window's path draws what the captures draw.

## Settling

At the village at 2560 by 1440 with the player's options, the camera turned away and back, the
frame differs from the settled one by 0.0118 five frames after the turn and by 0.0003 from 22 frames
on, the clouds and a few far patches. The frame takes 46.1 ms in Debug and 46.4 in Release, the GPU
bounding both, so the frame and its settling are the same in either.

## What differs from the pictures

The pictures are taken at 1280 by 720 with the time held, the mobs still and the frame settled. The
player's window draws four times the pixels at about 22 frames a second, its Soft Shadows at 0 left
the shadows' edges stepping where the pictures' soften, and the specks of the engine's item 97 wait
on its next package. The file's version line lets a changed default stand over a value the
player never chose.
