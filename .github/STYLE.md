# Style

Conventions for prose in this repository, which cover code comments, XML documentation, log and
exception messages, the answers of the game's commands, MSBuild, YAML and script comments, shader
comments, and Markdown. They exist so that the codebase reads consistently regardless of who wrote a
given file.

## Punctuation

Em dashes and en dashes are not used. Neither `—` nor `–`, in any file.

The spaced hyphen ` - ` is not sentence punctuation. Replace it according to the relationship
between the clauses.

| instead of | write |
|---|---|
| `the world is borrowed - only on the main thread` | `the world is borrowed, only on the main thread` |
| `it is two thoughts - here is the second` | `it is two thoughts. Here is the second` |
| `not uploaded yet - only meshes are - so this fails` | `not uploaded yet (only meshes are), so this fails` |
| `called from elsewhere - a menu, a test` | `called from elsewhere, such as a menu or a test` |

A comma continues a clause, a full stop separates two statements, and parentheses enclose an
aside.

Hyphens within words (`backend-agnostic`), arithmetic operators (`count - 1`) and the names of
characters (`the minus key`) are unaffected.

A running sentence holds no colon. A colon stands at the end of a sentence that introduces a
bulleted list, a code block or a command set below it, and after a label in a table. Inside a
sentence it appends an elaboration or a list that a sentence of its own, or the same sentence
recast, says plainly, and once every paragraph takes that shape the reader stops reading the mark
as anything at all.

The test is the text after the mark. A bulleted list, a code block or a command set below the
sentence is correct. Text that continues the sentence is a joint, whatever it says, so
`a tag is cheap: it costs nothing to store` is wrong where
`a tag is cheap, because it costs nothing to store` is right, and a list appended after the mark is
folded into the sentence or given a sentence of its own.

| instead of | write |
|---|---|
| `the wheel never moves the panel: rolling over a list hands it to whatever holds it` | `the wheel never moves the panel, because rolling over a list hands it to whatever holds it` |
| `a click is the engine's to answer: it raycasts the scene` | `a click is the engine's to answer. It raycasts the scene` |
| `three files, because they are edited by different hands: the first is` | `three files, because they are edited by different hands. The first is` |
| `it offers three: a menu, a button, a key` | `it offers a menu, a button and a key` |
| `makes the program the README opens with: a window with a turning cube` | `makes the program the README opens with, a window with a turning cube` |
| `Exit codes: 0 ok, 2 bad arguments` | `The exit code is 0 for success and 2 for bad arguments` |
| `**What is not here.** Despawning is not recorded` | unchanged, because the mark labels rather than joins |
| `Usage:` on a line of its own over a command | unchanged, because a command follows |

## Section banners

A banner names its section and ends:

```csharp
// -- Hierarchy
```

Trailing runs of dashes are not used. They carry no information, and their length is not
reproducible across edits.

## Comments

A comment that restates the code is a maintenance liability, because it has to be kept true as
the code changes. Remove it.

```csharp
// Incorrect: restates the line below it.
// Adds the time plugin so Time is updated every frame.
app.AddPlugin(new TimePlugin());

// Correct: no comment.
app.AddPlugin(new TimePlugin());
```

A comment is warranted when the reader would otherwise ask why, and the answer is not visible in
the surrounding code:

- a constraint that is not apparent locally
- a decision, together with the alternative that was rejected
- a hazard
- a safety argument for an `unsafe` block or a Vulkan synchronization choice

Where a comment would be needed to explain what the code does, renaming is usually the better
correction.

## Documentation

A public type or member of the game has a `<summary>` where its name does not say what it does or
what its value means, because an editor shows it when the member is hovered or completed. The
build compiles the documentation, so a `cref` that names nothing fails the strict build, while a
member with no summary is allowed. The tests are not held to it.

A summary says what the member does or what the value means, in a sentence, and a `<remarks>`
section holds the reasoning or the trap. An enum member whose name seems to say it all has a
summary that says what the name does not, and a second constructor or overload has one that says
what its extra parameter decides, rather than repeating the first one's.

## Tone

Prose is factual and plain.

Headings and rule names are descriptive rather than metaphorical or aphoristic. Use "Comments",
not "Comments earn their place".

Marketing register is not used, and neither is the rhetorical triple.

> No registration call, no partial-class list, no startup boilerplate. Add the package, write
> behaviors, compile, run.

State what the software does instead.

> Behaviors are discovered at compile time, so a consuming project needs no registration code.

The words `simply`, `just`, `powerful` and `blazing` are not used, nor are exclamation marks.
`easy` is not used to describe working with the software. It is acceptable in a warning, as in
"easy to get subtly wrong", where it tells the reader something they need.

Antithesis is not used for emphasis. A claim paired with the negation of something nobody
proposed reads as a slogan, and the negated half carries nothing.

> Discovery is generated, not scanned.

State the claim on its own.

> Registration happens at assembly load.

The construction is warranted where a reader would otherwise assume the negated alternative, and
correcting that assumption is the point of the sentence, as in "`DrawCube` records a cube for the
frame, not a mesh that outlives it".

Limitations are stated directly. "Not implemented yet" is preferable to "coming soon".

### Saying what things are for

A sentence names its subject and says what it does. The cleft forms `X is what Y needs`,
`which is what Y wants`, `that is what Y is for` and `this is what makes it work` route a plain
statement through a relative clause and read as a mannerism once they recur, which they do, because
every explanation can be forced into that shape. Code and features do not want things either, so
`a swapchain wants a new size` is written as the requirement it is.

| instead of | write |
|---|---|
| `a depth buffer is what an outline is made of` | `an outline is drawn from the depth buffer` |
| `which is what the shadow pass needs` | `which the shadow pass needs` |
| `that is what the timestamp queries are for` | `the timestamp queries measure that` |
| `this is what lets a reload keep its values` | `so a reload keeps its values` |
| `a scene of many cubes wants instancing` | `a scene of many cubes benefits from instancing` |
| `what the caller wanted` | unchanged, because a person wants things |

The same applies to `what X does is Y`, `the point is that`, and `the reason is that`, which delay
the statement they introduce.

## Register

Prose uses the technical and generic term for a thing, the engine's own where it has one, and no
figure of speech or casual paraphrase in its place. A paraphrase reads as an explanation the first
time and as a mannerism after, and a reader who knows the term translates it back each time.

A likeness is not a description and a figure of speech is not a term, so a thing is said by what it
is and does. Another product is named only where a rule or a number is taken from it, as the
player's sizes and speeds and the light levels are taken from Minecraft. A description holds no
plan, so no line says what a thing is the start of or grows toward.

| instead of | write |
|---|---|
| `the light that bounces`, `the bounce` | `global illumination`, or `radiance cascades` where the technique is meant and `indirect light` where its effect is |
| `a block that gives off light`, `a glowing block` | `an emissive block` |
| `drives the running game`, `driving a session` | `controls the running game through the command line`, `scripting a session` |
| `a drive of the game` | `a scripted run of the game` |
| `A voxel game in the manner of Minecraft, and the ground floor of a larger one` | `A voxel game set in an endless generated world whose blocks are placed and broken, with its light, its water, its mobs and the items they drop` |

The idioms in the manner of, the ground floor of, out of the box, under the hood, at a glance, a
handful of, a great deal of, in the wild, the plumbing and a long way are replaced in the same way,
each by the term or the number it stands for. A word in its literal sense stays, as by hand does for
work done without a tool or a program.

A name of the engine's API keeps its spelling, such as `DrawBounceWindow` and
`ModelMaterial.BouncesLight`, and so does a name from Minecraft, such as glowstone. An identifier of
the game keeps its name unless the thing it names is new and renaming it is cheap, and a new name
follows the rule, as `build/play.sh` does.

## Self-reference

Prose states what is true. It does not describe its own history, and it does not narrate the work
that produced it.

| instead of | write |
|---|---|
| `Scene loading is not blocked, contrary to what this file said before` | `Scene loading works` |
| `The question the old entry raised is settled: parts are addressed individually` | `Parts are addressed individually` |
| `Recorded here so the approach is settled when it comes up` | delete the sentence |
| `Cameras take their common parameters now` | `Cameras take their common parameters` |
| `The bug this was written to explain is fixed now` | delete the sentence |

A reader has no access to the previous version of a file, so a correction phrased against it says
nothing. Rewrite the passage as a plain statement and let the diff carry the change.

The words `now`, `already` and `no longer` are the usual signals. Each is fine where it
distinguishes two states the reader can see, as in "the handle reports `Loading` until the file
has been read", and wrong where it only means "since the last edit".

The same applies to `TODO.md`. An item is a description of outstanding work, so completed work is
removed from it or the entry is rewritten around what remains. It is not annotated as done, struck
through, or kept for the record.

A list of gaps is a claim about the software in the same way the rest of the prose is, so closing
a gap means rewriting the entry around what is left of it. `TODO.md` and the README's status
section describe the same software and are kept consistent with each other.

## References

Prose refers only to what a reader of the repository can open. An ignored directory, a path that
exists on one machine, a private branch or an internal ticket tells them nothing. Where something
of that kind informed a decision, state the decision and the reasoning, which is the part worth
keeping.

Public sources are citable, such as a package on nuget.org, a type in a dependency, an upstream
issue, or Minecraft's own behavior as its wiki describes it. So is anything checked in, by
repository-relative path, and the engine's repository, by its address on GitHub.

## Spelling

American spelling is used everywhere, in prose as well as in code, as in `behavior`, `color`,
`center`, `gray`, `meter`, `neighbor`, `initialize`, `serialize`, `canceled`, `modeled` and
`toward`. The public API is spelled that way, and so are .NET, Vulkan, SDL and raylib, and a word
spelled one way in a type name and another in the sentence describing it reads as two words.

A name that comes from outside is kept as it is, such as .NET's `StringMarshalling`, a file name in
a third-party icon set, or the text of a license.

## Scope

- `//` and `///` in C#, and `//` in Slang shaders
- exception messages, log output, the answers of the game's commands and `GITHUB_STEP_SUMMARY`
  content
- MSBuild `<!-- -->` comments
- YAML comments under `.github/workflows`, and comments in the scripts under `build/`
- `README.md`, `AGENTS.md`, every Markdown file under `.github/`, and this file

## Checks

```bash
# Em and en dashes.
grep -rn "[—–]" --include=*.cs --include=*.slang --include=*.md --include=*.yml --include=*.sh .

# Spaced hyphens in comments and strings.
grep -rn "^\s*\(///\|//\) .* - \|\"[^\"]* - [^\"]*\"" --include=*.cs --include=*.slang .

# Colons inside a running sentence, in comments and scripts and in the prose of the Markdown files
# outside their code. A label is correct, so the hits are read rather than counted.
grep -rn "^\s*\(///\|//\|#\) .*[a-z)\`]: " --include=*.cs --include=*.slang --include=*.sh --include=*.yml .
awk 'FNR == 1 { code = 0 } /^ *```/ { code = !code; next } !code && /[a-z)`]: /' README.md AGENTS.md .github/STYLE.md .github/COMMITS.md .github/E3D.md

# The casual paraphrases the register replaces. Names of the engine's API and Minecraft's glowstone
# are left, so each hit is read.
git grep -n -i -E "light that bounces|the bounce|bounc(e|es|ing) (no )?light|give[sn]? off|gave off|light-giving|glowing|\bdriv(e|en|es|ing)\b" -- ':!.github/REVIEW.md'

# The likenesses and idioms the register replaces. A word in its literal sense is left, so each hit
# is read.
git grep -n -i -E "in the manner of|ground floor|out of the box|under the hood|at a glance|a handful of|a great deal of|in the wild|plumbing|a long way" -- ':!.github/REVIEW.md'

# Padded section banners.
grep -rn "// -- .*--" --include=*.cs --include=*.slang .

# Cleft sentences and things that want. People want things, so each hit is read.
grep -rniE "\b(is|are|was|were) what\b|which is what|that is what|this is what|\bwants?\b" --include=*.cs --include=*.slang --include=*.md .

# Prose about earlier revisions.
grep -rniE "contrary to what|this (file|document|entry) (said|used to)|the (old|previous) (entry|version)|at the time .* was written" --include=*.md .
```

Arithmetic such as `counts[i - 1]` is the expected false positive in the second command.
