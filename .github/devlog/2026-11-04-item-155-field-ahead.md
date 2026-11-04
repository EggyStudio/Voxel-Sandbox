# The scene's field ahead of the eye

The game sets the scene's field's centre through the engine's `SetSceneFieldCenter` each frame,
ahead of the camera along its view by half the finest cascade's width, 64 cells of 0.25, eight
blocks, where the engine centred it on the camera and the finest cascade spent half its width behind
the player. Read on the engine's 6.0.91 at 1920 by 1080 as item 154 read it, at the cherry grove,
the deep cave and the open land from 90 over seed 1's spawn at noon looking east, the field around
the eye and ahead of it in the same run, two readings of 300 frames each.

| | Around the eye | Ahead |
|---|---|---|
| Grove, global illumination at Low | 3.48 to 3.83 ms, 0.173 | 3.57 to 3.60 ms, 0.171 |
| Grove, at Medium | 5.67 to 5.78, 0.166 | 5.60 to 5.64, 0.164 |
| Grove, at High | 6.56 to 6.83, 0.166 | 6.62 to 6.65, 0.164 |
| Open land over the spawn at High | 6.23 to 6.39, 0.780 | 6.08 and 6.09, 0.776 |
| The frame at the spawn at High at 12 columns | 16.11 to 17.58 | 16.60 and 16.86 |
| The GPU's part of it | 14.29 to 14.77 | 14.20 and 14.21 |
| Deep cave's mean linear light at High | 1.0 hundred-thousandths | 2.0 hundred-thousandths |

The grove ahead reads a little nearer its reference at every quality, its frames 5,531 pixels apart
by more than 8 of 255, the shade under the near crown and the ground below it, and the cave is
black in both shots where its reference's light is a millionth. Turning the eye through the full
circle over 240 frames, three degrees every two, the grove's frame at High reads 24.1 and 27.9 ms
ahead against 24.3 and 23.9 around the eye, the field's CPU 1.15 and 1.31 ms against 1.10 and 1.02
and the slowest frame 155 and 127 ms against 250 and 207.
