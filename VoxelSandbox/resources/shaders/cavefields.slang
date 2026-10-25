// The caves' noises read at the corners of a batch of columns' cells, a thread a reading, in fixed
// point with integers alone as SourceNoise and FixedNoise read them on the CPU, so each value is the
// CPU's to the bit. A column's readings lie corner by corner, each corner's samplers side by side, in
// the order Caves.Samplers gives them, and the CPU places the column's blocks from them.
//
// The engine enables no 64-bit integers in shaders, so a product of two numbers of 16 fractional
// bits is taken in four products of their halves, which come to the floor of the whole product as
// the CPU's 64-bit one does, overflow wrapping the same in both.

// How many columns the batch holds, how many samplers the caves read, and how many corners a column has.
uniform uint columns;
uniform uint samplers;
uniform uint corners;
// Each column's corner, x and z, in blocks, a uniform of the dispatch's own so several batches may
// be in flight at once.
uniform int4 origins[8];
// Each sampler's first level and its count of levels.
RWStructuredBuffer<uint> ranges;
// Each level's permutation, its offsets along x, y and z and its steps along them in 24 fractional
// bits, and its weight in 16, eight to a level.
RWStructuredBuffer<uint> levels;
// The permutations, 256 bytes each, four bytes to a word.
RWStructuredBuffer<uint> permutations;
RWStructuredBuffer<int> values;

static const int One = 65536;
// Each corner of a column's cells, four blocks apart across and eight up from the world's floor.
static const int Across = 5;
static const int CellWidth = 4;
static const int CellHeight = 8;
static const int Bottom = -64;

static const int Gradients[48] = {
    1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1, 0, 1, 0, 1, -1, 0, 1, 1, 0, -1, -1, 0, -1,
    0, 1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1, 1, 1, 0, 0, -1, 1, -1, 1, 0, 0, -1, -1,
};

// The floor of the product of two numbers of 16 fractional bits, by their halves.
int mul(int a, int b)
{
    int ah = a >> 16;
    int bh = b >> 16;
    uint al = uint(a) & 0xFFFFu;
    uint bl = uint(b) & 0xFFFFu;
    return ah * bh * One + ah * int(bl) + int(al) * bh + int((al * bl) >> 16);
}

int smooth(int t)
{
    return mul(mul(mul(t, t), t), mul(t, 6 * t - 15 * One) + 10 * One);
}

int lerp16(int t, int a, int b)
{
    return a + mul(t, b - a);
}

int p(uint permutation, int i)
{
    uint at = uint(i & 255);
    return int((permutations[permutation * 64u + (at >> 2)] >> ((at & 3u) * 8u)) & 255u);
}

int dot3(int hash, int x, int y, int z)
{
    int g = (hash & 15) * 3;
    return Gradients[g] * x + Gradients[g + 1] * y + Gradients[g + 2] * z;
}

// Ken Perlin's improved noise at a point of 24 fractional bits whose whole bits wrap at 256.
int improved(uint permutation, uint x, uint y, uint z)
{
    int i = int(x >> 24), j = int(y >> 24), k = int(z >> 24);
    int fx = int((x >> 8) & 0xFFFFu), fy = int((y >> 8) & 0xFFFFu), fz = int((z >> 8) & 0xFFFFu);
    int a = p(permutation, i), b = p(permutation, i + 1);
    int aa = p(permutation, a + j), ab = p(permutation, a + j + 1), ba = p(permutation, b + j), bb = p(permutation, b + j + 1);
    int u = smooth(fx), v = smooth(fy), w = smooth(fz);
    return lerp16(w,
        lerp16(v, lerp16(u, dot3(p(permutation, aa + k), fx, fy, fz), dot3(p(permutation, ba + k), fx - One, fy, fz)),
                  lerp16(u, dot3(p(permutation, ab + k), fx, fy - One, fz), dot3(p(permutation, bb + k), fx - One, fy - One, fz))),
        lerp16(v, lerp16(u, dot3(p(permutation, aa + k + 1), fx, fy, fz - One), dot3(p(permutation, ba + k + 1), fx - One, fy, fz - One)),
                  lerp16(u, dot3(p(permutation, ab + k + 1), fx, fy - One, fz - One), dot3(p(permutation, bb + k + 1), fx - One, fy - One, fz - One))));
}

[shader("compute")]
[numthreads(64, 1, 1)]
void computeMain(uint3 id : SV_DispatchThreadID)
{
    uint reading = id.x;
    if (reading >= columns * corners * samplers)
        return;
    uint sampler = reading % samplers;
    uint corner = reading / samplers % corners;
    uint column = reading / (samplers * corners);
    int x = origins[column].x + int(corner % uint(Across)) * CellWidth;
    int z = origins[column].y + int(corner / uint(Across) % uint(Across)) * CellWidth;
    int y = Bottom + int(corner / uint(Across * Across)) * CellHeight;
    uint first = ranges[sampler * 2u], count = ranges[sampler * 2u + 1u];
    int sum = 0;
    for (uint l = first; l < first + count; l++)
    {
        uint at = l * 8u;
        uint cx = levels[at + 1u] + uint(x) * levels[at + 4u];
        uint cy = levels[at + 2u] + uint(y) * levels[at + 5u];
        uint cz = levels[at + 3u] + uint(z) * levels[at + 6u];
        sum += mul(improved(levels[at], cx, cy, cz), int(levels[at + 7u]));
    }
    values[reading] = sum;
}
