# Item 29, the sections meshed on the workers

The main thread's meshing (`update.mesh`, eased over about a second) and the frame, before with every
section meshed on the main thread within a budget of 4 ms a frame and after with the sections meshed
on worker threads, half the processor's cores to 16 at once and 64 copied at most, the main thread
copying each section's blocks and taking its faces in. Other programs held the machine's load at 6
to 12 through the readings, so the ranges are wide.

Three ten-second flights east at sprint from 0, 4,000 and -4,000 on seed 1 at render distance 12,
Release, offscreen at 1280 by 720 on the RTX 4070 Laptop GPU, one after the other.

| | `update.mesh` | Frame |
|---|---|---|
| Before | 0.26, 3.28, 3.71 ms | 25.4, 37.7, 45.1 ms |
| After | 0.32, 0.23, 0.46 ms | 25.9, 31.5, 30.0 ms |

New land settling after a teleport to 4,000, -4,000 and 8,000, read every 10 frames until nothing is
left to generate or mesh, the means over those readings.

| | Settled in | `update.mesh` | Frame |
|---|---|---|---|
| Release, before | 6, 5, 5 s | 6, 4, 4 ms | 62, 30, 32 ms |
| Release, after | 5, 3, 3 s | 3, 3, 2 ms | 71, 35, 36 ms |
| Debug on lavapipe on four cores at 4 columns, before | 59, 74 s | 12, 10 ms | 243, 248 ms |
| Debug on lavapipe on four cores at 4 columns, after | 64, 73 s | 4, 4 ms | 246, 249 ms |

A first version lent a mesher to each section and took one round a frame, eight at once, and
settled in 9 to 13 seconds where the main thread settled in 2 to 3, since each section of air above
the land waited a frame for a worker. Air is let go at once, and the workers build what is copied
as fast as they can, so the land settles as fast as before.
