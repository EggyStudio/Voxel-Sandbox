#!/usr/bin/env python3
"""Pairs each of the game's mobs with Minecraft's outline of it, standing and mid-stride.

    build/mob-pairs.py [kind ...]

Each kind stands alone on the flat world at noon, held still, facing south, and is captured from 20
blocks to the south and to the east through a field of view of 9 degrees, then again from the east
posed at the widest of a stride at its walking speed. Minecraft's model of it, read from the client
on this machine by build/MobLayers.java, is drawn through the same camera, standing and at the same
stride by Minecraft's own walk, and its outline is laid over the game's capture in red, with the
outline filled beneath it. Each kind's three pairs are written to .github/assets/mobs/<kind>.webp,
and the nine standing in a row in daylight to .github/assets/mobs/daylight.webp, so a model's or
its light's change is shown again by running this. Where build/pictures.sh has fetched the default
packs into captures/packs, the row is taken again wearing their pictures, to
.github/assets/mobs/daylight-pack.webp.

The client is the version MINECRAFT_VERSION names, 26.3 unless set, in the launcher folder
MINECRAFT_DIR names, or else the first of ~/.minecraft, ~/.minecraftx and the folders the launcher
keeps on Windows and macOS that holds it. Where none does, this says so and makes nothing. Java is
JAVA where set, else the runtime the launcher keeps for that version, else java on the path. The
game is the Debug build that `dotnet build VoxelSandbox.slnx` makes, ./e3d runs out of the engine
checkout ENGINE_CHECKOUT names or the folder above this one, and the drawing needs Pillow.
Nothing of the client is kept, only the pictures drawn from its numbers.
"""
import glob
import json
import math
import os
import subprocess
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = Path(__file__).resolve().parent.parent
PROGRAM = ROOT / "VoxelSandbox/bin/Debug/net10.0/VoxelSandbox"
CAPTURES = ROOT / "captures/mob-pairs"
# The default packs build/pictures.sh fetches and checks, in the order packs.json names them, the
# first over the rest, those it has fetched.
PACKS = [p for p in (ROOT / "captures/packs" / pack["file"] for pack in json.loads((ROOT / "VoxelSandbox/resources/packs.json").read_text())["packs"]) if p.is_file()]
OUT = ROOT / ".github/assets/mobs"

# The camera every capture and outline is seen through, the mob's feet at MOB.
W, H, FOV = 1280, 720, 9.0
F = (H / 2) / math.tan(math.radians(FOV / 2))
MOB = (0.5, 4.0, -2.5)
EYE_Y = 5.2
CAMERAS = {"front": (0.5, EYE_Y, 17.5), "side": (20.5, EYE_Y, -2.5)}
CROP = (290, 100, 990, 700)

# Each kind's walking speed in the game, in blocks a second, MobKind's Speed, and the layers of
# Minecraft's ModelLayers its model is drawn from, a sheep's wool over its body.
SPEED = {"villager": 1.5, "pig": 1.25, "cow": 1.1, "sheep": 1.15, "chicken": 1.0, "zombie": 1.0,
         "skeleton": 1.2, "creeper": 1.0, "spider": 1.5}
LAYERS = {"villager": ["VILLAGER"], "pig": ["PIG"], "cow": ["COW"], "sheep": ["SHEEP_WOOL", "SHEEP"],
          "chicken": ["CHICKEN"], "zombie": ["ZOMBIE"], "skeleton": ["SKELETON"], "creeper": ["CREEPER"],
          "spider": ["SPIDER"]}

# The game's player looks through a narrow field of view, and the one loaded mob is posed at the
# widest of a stride at its kind's speed as Gait.Step would bring it there, its phase at 0.
NARROW = 'VoxelSandbox.GameCommands.Game!.Player.FieldOfView = 9f; "narrowed"'
WIDE = 'VoxelSandbox.GameCommands.Game!.Player.FieldOfView = 70f; "widened"'
POSE = """var mob = VoxelSandbox.GameCommands.Game!.Mobs.All[0];
var type = mob.GetType();
type.GetProperty("Stride")!.SetValue(mob, MathF.Min(1, 4 * mob.Kind.Speed / 20));
type.GetProperty("StridePhase")!.SetValue(mob, 0f);
$"{mob.Kind.Name} posed at a stride of {mob.Stride:0.000}"
"""


def fail(message):
    print(f"mob-pairs: {message}", file=sys.stderr)
    sys.exit(1)


def client():
    """The launcher folder, the version's description and its jar, or a plain refusal where the machine has none."""
    version = os.environ.get("MINECRAFT_VERSION", "26.3")
    home = Path.home()
    folders = [Path(os.environ["MINECRAFT_DIR"])] if "MINECRAFT_DIR" in os.environ else [
        home / ".minecraft", home / ".minecraftx", Path(os.environ.get("APPDATA", home)) / ".minecraft",
        home / "Library/Application Support/minecraft"]
    for folder in folders:
        jar = folder / "versions" / version / f"{version}.jar"
        if jar.is_file():
            return folder, json.loads((folder / "versions" / version / f"{version}.json").read_text()), jar
    fail(f"no Minecraft {version} client is on this machine, looked for versions/{version}/{version}.jar in "
         + ", ".join(str(f) for f in folders) + "; set MINECRAFT_DIR to a launcher's folder that has it, "
         "or MINECRAFT_VERSION to one it has, and nothing is made until then")


def java(folder, description):
    if "JAVA" in os.environ:
        return os.environ["JAVA"]
    component = description.get("javaVersion", {}).get("component", "")
    for pattern in (f"jre/{component}/bin/java", f"runtime/{component}/**/bin/java", f"runtime/{component}/**/bin/java.exe"):
        found = sorted(glob.glob(str(folder / pattern), recursive=True))
        if component and found:
            return found[0]
    return "java"


def numbers(folder, description, jar):
    """Minecraft's baked layers, as MobLayers.java prints them."""
    paths = [folder / "libraries" / library["downloads"]["artifact"]["path"]
             for library in description["libraries"] if "artifact" in library.get("downloads", {})]
    classpath = os.pathsep.join([str(p) for p in paths if p.is_file()] + [str(jar)])
    layers = sorted({layer for kind in LAYERS.values() for layer in kind})
    # From the captures' folder, where the client's logging writes its logs/latest.log.
    CAPTURES.mkdir(parents=True, exist_ok=True)
    run = subprocess.run([java(folder, description), "-cp", classpath, str(ROOT / "build/MobLayers.java"), *layers],
                         capture_output=True, text=True, cwd=CAPTURES)
    if run.returncode != 0:
        fail(f"MobLayers.java ended with {run.returncode}: {run.stderr.strip()[-600:]}")
    return run.stdout


def parse(text):
    layers, stack, name = {}, [], None
    for line in text.splitlines():
        if line.startswith("== "):
            name, stack = line[3:].strip(), []
            continue
        depth = (len(line) - len(line.lstrip(" "))) // 2
        words = line.split()
        if not words:
            continue
        if words[0] == "part":
            n = [float(w) for w in words[3:6] + words[7:10] + words[11:14]]
            part = {"name": words[1], "at": n[0:3], "turn": n[3:6], "scale": n[6:9], "cubes": [], "children": []}
            stack = stack[:depth]
            if stack:
                stack[-1]["children"].append(part)
            else:
                layers[name] = part
            stack.append(part)
        elif words[0] == "cube":
            v = [float(w) for w in words[2:5] + words[6:9] + words[10:13]]
            stack[depth - 1]["cubes"].append((v[0:3], v[3:6], v[6:9]))
    return layers


def e3d(*args, quiet=True):
    run = subprocess.run(["./e3d", *args, *(["--quiet"] if quiet else []), "--timeout", "600"], cwd=ROOT, capture_output=True, text=True)
    if run.returncode != 0:
        fail(f"./e3d {' '.join(args[:2])} ended with {run.returncode}: {run.stderr.strip()}")
    return run.stdout


def capture(kinds):
    """The game's captures of each kind, front and side standing and side mid-stride."""
    if not PROGRAM.is_file() and not PROGRAM.with_suffix(".exe").is_file():
        fail(f"{PROGRAM.relative_to(ROOT)} is not built, which dotnet build VoxelSandbox.slnx does")
    CAPTURES.mkdir(parents=True, exist_ok=True)
    opened = json.loads(e3d("open", str(PROGRAM.relative_to(ROOT)), "--offscreen", "--transient", "--flat", "--json", quiet=False))
    session = ["--session", str(opened["data"]["pid"])]
    try:
        for command in (["voxel.time", "12"], ["voxel.cycle", "off"], ["voxel.hud", "off"], ["voxel.clouds", "off"],
                        ["voxel.fly", "on"], ["voxel.tp", "0.5", "4", "0.5"], ["voxel.look", "0", "0"], ["frames.wait", "240"]):
            e3d("command", *command, *session)
        e3d("eval", NARROW, *session)
        e3d("command", "frames.wait", "240", *session)
        for kind in kinds:
            # Spawned three blocks ahead of the player looking north, the mob faces it, south.
            for command in (["voxel.still", "off"], ["voxel.kill", "all"], ["voxel.tp", "0.5", "4", "0.5"], ["voxel.look", "0", "0"],
                            ["frames.wait", "2"], ["voxel.spawn", kind, "0"], ["voxel.still", "on"], ["frames.wait", "2"]):
                e3d("command", *command, *session)
            # The eye 1.2 blocks above the feet at 4, at 5.2, puts the player's feet at 3.58.
            for view, (x, _, z), heading in (("front", CAMERAS["front"], "0"), ("side", CAMERAS["side"], "270")):
                e3d("command", "voxel.tp", str(x), "3.58", str(z), *session)
                e3d("command", "voxel.look", heading, "0", *session)
                e3d("command", "frames.wait", "40", *session)
                e3d("shot", str(CAPTURES / f"{kind}-{view}.png"), *session)
            e3d("eval", POSE, *session)
            e3d("command", "frames.wait", "3", *session)
            e3d("shot", str(CAPTURES / f"{kind}-walk.png"), *session)
        daylight(session)
        if PACKS:
            e3d("command", "voxel.packs", "; ".join(str(p) for p in PACKS), *session)
            daylight(session, "daylight-pack")
    finally:
        subprocess.run(["./e3d", "stop", *session, "--quiet"], cwd=ROOT, capture_output=True)


def daylight(session, name="daylight"):
    """The nine in a row facing south, two and a half blocks apart, seen at noon from the south and a
    little above through the game's own field of view, held still from before the first is placed
    so none turns or walks off home."""
    e3d("eval", WIDE, *session)
    for command in (["voxel.still", "off"], ["voxel.kill", "all"], ["voxel.still", "on"]):
        e3d("command", *command, *session)
    for i, kind in enumerate(LAYERS):
        for command in (["voxel.tp", str(0.5 + 2.5 * i), "4", "0.5"], ["voxel.look", "0", "0"], ["frames.wait", "2"], ["voxel.spawn", kind, "0"]):
            e3d("command", *command, *session)
    for command in (["voxel.tp", "10.5", "4", "7"], ["voxel.look", "0", "-10"], ["frames.wait", "60"]):
        e3d("command", *command, *session)
    e3d("shot", str(CAPTURES / f"{name}.png"), *session)


def mul(a, b):
    return [[sum(a[i][k] * b[k][j] for k in range(4)) for j in range(4)] for i in range(4)]


def translate(x, y, z): return [[1, 0, 0, x], [0, 1, 0, y], [0, 0, 1, z], [0, 0, 0, 1]]
def scale(x, y, z): return [[x, 0, 0, 0], [0, y, 0, 0], [0, 0, z, 0], [0, 0, 0, 1]]
def rx(a): c, s = math.cos(a), math.sin(a); return [[1, 0, 0, 0], [0, c, -s, 0], [0, s, c, 0], [0, 0, 0, 1]]
def ry(a): c, s = math.cos(a), math.sin(a); return [[c, 0, s, 0], [0, 1, 0, 0], [-s, 0, c, 0], [0, 0, 0, 1]]
def rz(a): c, s = math.cos(a), math.sin(a); return [[c, -s, 0, 0], [s, c, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1]]
def apply(m, p): return [m[i][0] * p[0] + m[i][1] * p[1] + m[i][2] * p[2] + m[i][3] for i in range(3)]


def walk(kind):
    """The turns Minecraft's setupAnim gives each part at the widest of a stride at the game's
    speed of the kind: its walk speed is four times the blocks moved a tick, up to 1, and its
    position is where cos(position × 0.6662) is 1."""
    speed = min(1.0, 4 * SPEED[kind] / 20)
    if kind == "spider":
        turns = {}
        for pair, offset in (("hind_leg", 0), ("middle_hind_leg", math.pi), ("middle_front_leg", math.pi / 2), ("front_leg", 3 * math.pi / 2)):
            turn, lift = -math.cos(offset) * 0.4 * speed, abs(math.sin(offset) * 0.4) * speed
            turns["right_" + pair], turns["left_" + pair] = (0, turn, lift), (0, -turn, -lift)
        return turns
    legs = 1.4 * speed * (0.5 if kind == "villager" else 1)
    turns = {"right_hind_leg": legs, "left_hind_leg": -legs, "right_front_leg": -legs, "left_front_leg": legs,
             "right_leg": legs, "left_leg": -legs}
    if kind == "skeleton":
        turns.update({"right_arm": -speed, "left_arm": speed})
    return {name: (turn, 0, 0) for name, turn in turns.items()}


def corners(layer, kind, walking):
    """Each cuboid's eight corners in the world, the mob facing south at MOB as the game spawns it."""
    # Minecraft's render of a mob facing south, turned 180 degrees about y, its model scaled by -1
    # in x and y and moved down 1.501, each part placed in pixels over 16.
    entity = mul(translate(*MOB), mul(ry(math.pi), mul(scale(-1, -1, 1), translate(0, -1.501, 0))))
    turns = walk(kind) if walking else {}
    boxes = []

    def visit(part, parent):
        # The hat layers, which the villager's, the zombie's and the skeleton's own textures leave
        # clear, are drawn as nothing.
        if part["name"] in ("hat", "hat_rim"):
            return
        ax, ay, az = part["turn"]
        # A zombie holds its arms out a tenth of a right angle under level when not attacking, as
        # AnimationUtils.animateZombieArms does with π over 2.25.
        if kind == "zombie" and part["name"] in ("right_arm", "left_arm"):
            ax = -math.pi / 2.25
        dx, dy, dz = turns.get(part["name"], (0, 0, 0))
        x, y, z = part["at"]
        m = mul(parent, mul(translate(x / 16, y / 16, z / 16), mul(rz(az + dz), mul(ry(ay + dy), mul(rx(ax + dx), scale(*part["scale"]))))))
        for origin, size, grow in part["cubes"]:
            lo = [(origin[i] - grow[i]) / 16 for i in range(3)]
            hi = [(origin[i] + size[i] + grow[i]) / 16 for i in range(3)]
            boxes.append([apply(m, (cx, cy, cz)) for cx in (lo[0], hi[0]) for cy in (lo[1], hi[1]) for cz in (lo[2], hi[2])])
        for child in part["children"]:
            visit(child, m)

    visit(layer, entity)
    return boxes


def project(p, view):
    ex, ey, ez = CAMERAS[view]
    if view == "front":
        d = ez - p[2]
        return W / 2 + F * (p[0] - ex) / d, H / 2 - F * (p[1] - ey) / d
    d = ex - p[0]
    return W / 2 + F * (ez - p[2]) / d, H / 2 - F * (p[1] - ey) / d


def hull(points):
    points = sorted(set(points))
    if len(points) < 3:
        return points

    def cross(o, a, b):
        return (a[0] - o[0]) * (b[1] - o[1]) - (a[1] - o[1]) * (b[0] - o[0])

    lower, upper = [], []
    for p in points:
        while len(lower) >= 2 and cross(lower[-2], lower[-1], p) <= 0:
            lower.pop()
        lower.append(p)
    for p in reversed(points):
        while len(upper) >= 2 and cross(upper[-2], upper[-1], p) <= 0:
            upper.pop()
        upper.append(p)
    return lower[:-1] + upper[:-1]


def silhouette(layers, kind, view, walking):
    mask = Image.new("L", (W, H), 0)
    draw = ImageDraw.Draw(mask)
    for name in LAYERS[kind]:
        for box in corners(layers[name], kind, walking):
            draw.polygon(hull([project(c, view) for c in box]), fill=255)
    return mask


def pairs(layers, kind, version):
    font = ImageFont.load_default(size=22)
    w, h = CROP[2] - CROP[0], CROP[3] - CROP[1]
    sheet = Image.new("RGB", (w * 3 + 40, h * 2 + 70), (250, 250, 248))
    draw = ImageDraw.Draw(sheet)
    for i, (view, shot, walking, title) in enumerate((("front", "front", False, "standing, front"), ("side", "side", False, "standing, side"),
                                                      ("side", "walk", True, "mid-stride, side"))):
        mask = silhouette(layers, kind, view, walking)
        game = Image.open(CAPTURES / f"{kind}-{shot}.png").convert("RGB")
        edge = mask.filter(ImageFilter.FIND_EDGES).point(lambda v: 255 if v > 0 else 0).filter(ImageFilter.MaxFilter(3))
        game.paste((230, 30, 30), mask=edge)
        plain = Image.new("RGB", (W, H), (236, 236, 232))
        plain.paste((70, 70, 78), mask=mask)
        x = i * (w + 20)
        sheet.paste(game.crop(CROP), (x, 40))
        sheet.paste(plain.crop(CROP), (x, 40 + h + 30))
        draw.text((x + 8, 8), f"{kind}, {title}: the game, Minecraft's outline in red", fill=(30, 30, 30), font=font)
        draw.text((x + 8, 40 + h + 2), f"Minecraft {version}'s model through the same camera", fill=(30, 30, 30), font=font)
    OUT.mkdir(parents=True, exist_ok=True)
    sheet.resize((1600, round(sheet.height * 1600 / sheet.width)), Image.LANCZOS).save(OUT / f"{kind}.webp", quality=88)
    print(f"{kind}: {(OUT / f'{kind}.webp').relative_to(ROOT)}")


def main():
    kinds = sys.argv[1:] or list(LAYERS)
    unknown = [k for k in kinds if k not in LAYERS]
    if unknown:
        fail(f"no mob is called {', '.join(unknown)}, which is {', '.join(LAYERS)}")
    folder, description, jar = client()
    layers = parse(numbers(folder, description, jar))
    capture(kinds)
    for kind in kinds:
        pairs(layers, kind, description.get("id", os.environ.get("MINECRAFT_VERSION", "26.3")))
    Image.open(CAPTURES / "daylight.png").convert("RGB").save(OUT / "daylight.webp", quality=88)
    print(f"the nine: {(OUT / 'daylight.webp').relative_to(ROOT)}")
    if PACKS:
        Image.open(CAPTURES / "daylight-pack.png").convert("RGB").save(OUT / "daylight-pack.webp", quality=88)
        print(f"the nine with the packs: {(OUT / 'daylight-pack.webp').relative_to(ROOT)}")
    else:
        print("the nine with the packs: none, since captures/packs holds none of the default packs, which build/pictures.sh fetches")


main()
