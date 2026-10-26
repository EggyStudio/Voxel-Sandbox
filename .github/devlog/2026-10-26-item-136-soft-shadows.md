# Item 136, soft shadows

The sun's width through the engine's `SetShadowSoftness` on 3DEngine 5.2.322, read in one session in
Release with the three default packs on tuned's power-saving profile with the processor's energy
preference at power.

Seed 1's plains village from 550.5, 72, -356.5, looking at 45 degrees and 17 down, at noon and at
hour 16, the sun 0, 0.53, 1.5 and 3 degrees across, a capture of each at 1280 by 720. At noon the
oaks' shadows lie under them, short, and the four widths differ little. At hour 16 the shadows run
long, and at 0 their edges step along the pixels of the blocks they cross the whole way. At 0.53 the
far ends soften a little and the steps show along most of each edge. At 1.5 each shadow is sharp at
the trunk and soft toward its far end, the steps gone there, the crown's shape still read in it. At 3
the far half of each crown's shadow is a smear, as a cloud's shadow is.

Seed 1's cherry grove at 1920 by 1080 at hour 16, a profile over 300 frames for each width, in two
rounds. Where another program used the GPU during a reading, it is named.

| | Frame | Scene pass | Shadow maps |
|---|---|---|---|
| Low, 0 | 16.46 and 17.25 ms, the second with a test run at 14% | 6.73 and 7.28 ms | 4.69 and 4.95 ms |
| Low, 0.53 | 16.94 and 17.39 ms | 7.50 and 7.75 ms | 4.75 and 4.75 ms |
| Low, 1.5 | 16.87 and 16.80 ms | 7.51 and 7.41 ms | 4.62 and 4.68 ms |
| Low, 3 | 17.00 and 16.77 ms, the first with a test run at 12% | 7.54 and 7.47 ms | 4.68 and 4.59 ms |
| High, 0 | 19.69 and 19.81 ms | 7.27 and 7.29 ms | 4.65 and 4.69 ms |
| High, 0.53 | 20.78 and 20.14 ms | 7.94 and 7.85 ms | 4.85 and 4.61 ms |
| High, 1.5 | 20.40 and 20.38 ms | 7.91 and 7.90 ms | 4.69 and 4.63 ms |
| High, 3 | 21.30 and 20.66 ms, both with a test run at 12% | 8.29 and 7.94 ms | 4.91 and 4.72 ms |

Any width costs the scene pass, where each point looks over the shadow map for what shades it, about
0.6 ms at High against 0, and 3 up to 1 ms. The shadow maps cost the same at every width. The frame
at High grows by about 0.6 ms at 1.5, and at Low it moves less than the rounds differ.

1.5 is the default, the width nearest a warm low sun's long soft shadows at under a millisecond of
the frame, where 0.53 keeps most of the steps and 3 loses the crowns' shapes. Render Settings sets it
from 0 to 5 degrees, and the options file keeps it.
