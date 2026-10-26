# Item 27, the far cascade

The engine's far cascade on 3DEngine 5.2.319, set through `SetSceneField` past the four counted
cascades to reach the render distance, 192 blocks at 12 columns, read in one session in Release with
the three default packs on tuned's power-saving profile with the processor's energy preference at
power.

Seed 1's deep cave at -59.5, -4, -91.5 at noon, the tops of the blocks 45 and 90 blocks ahead, each
a pixel's red, green and blue.

| Quality | Far cascade off | Far cascade on |
|---|---|---|
| Low | 22, 13, 7 and 22, 13, 7 | 17, 9, 3 and 18, 10, 4 |
| Medium | 0, 0, 0 and 22, 13, 7 | 0, 0, 0 and 22, 13, 6 |
| High | 0, 0, 0 and 0, 0, 0 | 0, 0, 0 and 0, 0, 0 |

Seed 1's cherry grove at 1920 by 1080 at hour 16, a profile over 300 frames, the readings with no
other program on the GPU.

| | Frame | Global illumination | Scene pass |
|---|---|---|---|
| Low, off | 17.2 ms | 3.06 ms | 6.92 ms |
| Low, on | 19.4 and 19.1 ms | 4.16 and 4.11 ms | 8.03 and 7.94 ms |
| High, off | 21.6 ms | 6.35 ms | 7.80 ms |
| High, on | 23.2 and 24.6 ms | 7.50 and 7.30 ms | 8.35 and 8.28 ms |

The far cascade darkens the cave's far walls at Low by about a quarter, its rays meeting the rock
where they met the sky, changes nothing seen at Medium and High, and costs 1.6 to 2 ms of the frame
at both, so it is off by default and Render Settings turns it on.
