# Item 147, the held light without a shadow

The light of a torch, a lantern or glowstone in the hand or dropped is a plain point light with no
shadow map, and the game is pinned to 3DEngine 5.2.333, which carries the engine's mend of the
specks of its item 97. Read in Debug builds with the three default packs, offscreen, on tuned's
power-saving profile with the processor's energy preference at power.

## The cost

Seed 1's cherry grove at 1920 by 1080 at hour 16 with a torch in hand, a profile over eight
seconds, two readings to a session, sessions taken in turns. Another program used the GPU during
some readings, which put the frame at 31 to 32 ms in either build, so those are left out.

| | Frame | GPU's shadows | CPU's shadows |
|---|---|---|---|
| The light casting shadows, 5.2.326 | 22.87 and 23.24 ms | 3.95 and 4.09 ms | 1.60 and 1.93 ms |
| A plain point light, 5.2.326 | 21.84 and 22.60 ms | 3.31 and 3.34 ms | 1.32 and 1.35 ms |
| A plain point light, 5.2.333 | 21.77 and 24.61 ms | 3.43 and 3.89 ms | 1.29 and 1.29 ms |

The held light's shadow map cost about 0.7 ms of the GPU and 0.3 to 0.5 ms of the CPU.

## The specks

The meadow by the plains village at midnight from 560.5, 66, -350.5 looking north and 20 down, a
torch in hand, at 1280 by 720 on the RTX 4070 Laptop GPU. The pixels under 6 of 255 whose 5 by 5
neighbors read over 40 number 17 with the light casting shadows, 13 of them a seam of its shadow
down the middle of the view below the eye, and 4 with a plain point light on 5.2.326 and on 5.2.333
alike, the edges of two stars and a cloud against the sky and a tree's crown against the sky. No
speck lies on a lit surface before or after the pin on this GPU, as the engine read. The grass's
blades shaded each other in dark patches under the light's shadow and are lit evenly without it.
