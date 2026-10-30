# Item 141, the far cascade on and the shadows at 128 blocks

The far cascade on by default and the sun's shadows reaching 128 blocks, read in a Debug build with
the three default packs, offscreen at 1280 by 720, flying at a height of 90 over seed 1's spawn at
noon looking east, on tuned's power-saving profile with the processor's energy preference at power.
Each reading waited for a GPU no other program used and took a profile over ten seconds. An options
file of an older version that holds the far cascade off or the shadows at 64 blocks takes the new
defaults, the file's version 3.

## At 12 columns, one setting at a time

| | Frame | GPU's passes | CPU's passes | GPU's shadows | CPU's shadows | Cascades' draws |
|---|---|---|---|---|---|---|
| Shadows 64, far cascade off | 16.68 | 11.88 | 12.84 | 0.81 | 2.01 | 389 |
| Shadows 128, far cascade off | 17.13 | 13.79 | 13.91 | 2.68 | 3.05 | 1,294 |
| Shadows 128, far cascade on | 16.98 to 17.12 | 15.65 to 15.78 | 13.88 to 14.10 | 2.57 to 2.61 | 3.02 to 3.07 | 1,276 to 1,311 |

The shadows at 128 blocks draw three times the cascades' sections, 1.8 ms more of the GPU and 1 more
of the CPU, and the far cascade 1.9 ms more of the GPU, 1.1 of it the scene pass and 0.75 global
illumination.

## By distance at the defaults

The quiet readings of nine, 8, 12 and 16 columns up, down and up.

| Columns | Frame | GPU's passes | CPU's passes | GPU's shadows | Draws | Memory as first reached |
|---|---|---|---|---|---|---|
| 8 | 16.67 and 16.67 | 11.03 and 12.94 | 7.85 and 7.99 | 2.12 and 2.54 | 1,883 and 1,913 | 1,117 MB |
| 12 | 16.80, 16.96 and 17.42 | 15.53 to 15.60 | 13.25 to 14.01 | 2.50 to 2.54 | 3,129 to 3,179 | 1,455 MB |
| 16 | 26.51 and 26.78 | 20.97 and 21.20 | 22.21 and 22.25 | 2.56 and 2.57 | 4,734 and 4,758 | 2,266 MB |

Two readings with another program on the GPU, 8 columns at 23.88 ms and 16 at 57.05, are left out.

## The pictures

The plains village at hour 16, its far oak's shadow faint from the contact shadows at 64 blocks and
dark from the shadow map at 128, the frames otherwise alike. Seed 1's deep cave at -59.5, -4, -91.5
at noon at High is black with the far cascade off and on, as item 27 read it. At Low its far wall
takes the sky's light evenly with the far cascade off and only its blocks' own share with it on.
