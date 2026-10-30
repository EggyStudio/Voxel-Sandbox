# Item 150, the play job's time

The build workflow's play job, `build/play.sh` on Mesa's lavapipe on a hosted runner, read from the
logs of the run of `b25e668` on 2026-10-26, 77 minutes, and of `b5626c6` on 2026-10-30, 84 minutes,
each check's time as the walk writes it. The checks took 4,568 s and 4,983 s in all, 415 s more,
most checks growing by 5 to 20 percent and three by none, the far cascade and the shadows to 128 blocks of
items 141 and 143 giving lavapipe more to draw each frame.

| Check | 2026-10-26 | 2026-10-30 | Growth |
|---|---|---|---|
| the rain's splashes | 734 s | 795 s | 61 s |
| the held torch | 293 s | 306 s | 13 s |
| the pack's pictures | 222 s | 242 s | 20 s |
| the seed's field | 196 s | 217 s | 21 s |
| the swim to the bank | 175 s | 192 s | 17 s |
| the jungle's floor | 177 s | 188 s | 11 s |
| the options | 157 s | 173 s | 16 s |
| the fire's top | 155 s | 167 s | 12 s |
| the block's bits | 141 s | 152 s | 11 s |
| the water's depth | 128 s | 142 s | 14 s |
| the open | 132 s | 138 s | 6 s |
| the pool's reflection | 111 s | 122 s | 11 s |
| the item's name | 95 s | 115 s | 20 s |
| the jungle | 101 s | 108 s | 7 s |

The other 34 checks took 0 to 95 s each and grew by at most 14 s.

The rain's splashes took the longest, 795 s, of it about 530 s for 300 frames waited with Rain
Splashes at none before its count was read, at 1.8 s a frame. The count eases toward none by about
a twentieth a frame, the game's step held to 0.05 s, so it falls under one drop in about 80 frames,
and the walk reads it every 20 frames until it does, up to 300. The seed's field held Control for
120 frames where V follows within a few once `input.state` reads Control down, which held its check
about 90 frames longer than it reads anything on lavapipe. It holds about a second's frames by the
median frame `voxel.frames` reads, 30 at least, since a fixed 30 ended before V came on the GPU. No check reads only what a cheaper
one reads, so none is dropped. The job's limit is 150 minutes, where 90 left 6 minutes to spare.

On the development machine's lavapipe the walk took 1,840 s, the rain's splashes 154 s and the
seed's field 40 s, the seed read as 12345.
