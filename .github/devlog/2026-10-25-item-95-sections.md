# Item 95, the sections as entities

The frame at item 93's views, seed 1's cherry grove and plains village at hour 16, the three default
packs, render distance 12, High with the screen's probes, unlimited frames, offscreen on the RTX 4070
Laptop GPU, each a profile over 300 frames in a Release build, read twice for each build in the order
5.2.291, 5.2.313, entities, entities, 5.2.313, 5.2.291. Another program held 10 to 50 percent of the
GPU through the runs, so the ranges are wide and the frames longer than a quiet GPU's.

The builds are the game on 5.2.291 drawing every section through `DrawMesh`, the game on 5.2.313
drawing the same way, and the game on 5.2.313 with each section's solid faces, its faces a pack cuts
out that cast a whole shadow and its lava as entities. `draw.sections` is the game's main thread
drawing the sections each frame. Times are in milliseconds.

| Build | View | Frame | Scene pass | Shadows | Global illumination | `draw.sections` |
|---|---|---|---|---|---|---|
| 5.2.291 | grove at 1920 by 1080 | 41.5 to 46.6 | 19.8 to 23.6 | 5.8 to 8.9 | 4.5 to 6.4 | 1.81 to 5.96 |
| 5.2.291 | grove at 1280 by 720 | 29.7 to 31.0 | 14.4 to 15.7 | 7.3 to 7.7 | 4.3 to 5.0 | 1.59 to 1.85 |
| 5.2.291 | village at 1920 by 1080 | 33.5 to 34.8 | 19.8 to 19.9 | 6.9 to 7.1 | 4.1 to 4.4 | 1.48 to 1.68 |
| 5.2.291 | village at 1280 by 720 | 27.5 to 29.5 | 14.7 to 15.2 | 7.1 to 7.2 | 3.8 to 4.5 | 1.27 to 1.59 |
| 5.2.313 | grove at 1920 by 1080 | 35.1 to 45.7 | 19.8 to 21.2 | 6.2 to 6.4 | 4.5 to 4.8 | 3.64 to 5.66 |
| 5.2.313 | grove at 1280 by 720 | 29.1 to 32.8 | 13.0 to 14.1 | 5.2 to 6.3 | 3.6 to 4.5 | 1.45 to 3.46 |
| 5.2.313 | village at 1920 by 1080 | 27.9 to 33.4 | 17.3 to 19.4 | 4.7 to 7.2 | 3.2 to 4.4 | 1.42 to 1.50 |
| 5.2.313 | village at 1280 by 720 | 20.3 to 29.2 | 11.5 to 13.0 | 4.7 to 6.5 | 2.7 to 3.9 | 1.22 to 3.43 |
| Entities | grove at 1920 by 1080 | 23.2 to 24.0 | 9.8 to 10.3 | 5.5 to 5.6 | 4.7 to 4.8 | 0.68 to 1.50 |
| Entities | grove at 1280 by 720 | 19.6 to 20.0 | 6.5 to 6.6 | 5.2 to 5.7 | 3.9 to 4.0 | 0.78 to 0.93 |
| Entities | village at 1920 by 1080 | 23.5 to 27.1 | 11.6 to 12.0 | 5.0 to 5.8 | 3.6 to 3.9 | 0.65 to 1.77 |
| Entities | village at 1280 by 720 | 19.2 to 20.0 | 7.1 to 7.5 | 4.8 to 5.5 | 3.1 to 3.6 | 0.71 to 1.54 |

The entities draw the same frame. At the grove, the village, the lava lake and the ravine the
sections, draws and triangles `voxel.state` counts are the same in both 5.2.313 builds, 1,673 draws
of 1.15 million triangles at the grove, and the captures differ by an RMSE of 0.0002 to 0.0011 with
at most five pixels more than 3 percent apart. The engine draws 1,059 entities at the grove and 976
at the village, and its `MeshEntityDraws` takes 0.25 to 0.33 ms a frame. With them the scene pass
takes about half its time on the GPU and the passes' recording on the CPU less than half, the
shadows' 4.5 to 6.1 ms against 8.6 to 11.9 at the grove. 5.2.313 alone takes 1 to 2 ms off the
shadows and up to 1.5 ms off global illumination against 5.2.291 within the noise of the load.
