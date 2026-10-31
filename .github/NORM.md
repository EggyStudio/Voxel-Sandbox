# Norm

The rules this game keeps, each with a number, a reason and what checks it, written by the session
that writes [REVIEW.md](REVIEW.md). The rules are the engines' of the same number, from the norm
BevyCSharp and 3DEngine share, taken where they fit a game built on the package; a rule the engines
have and this file does not is not asked of the game.

## Scope

The norm covers how the game is laid out and built: its project, its tests, its documents, its build
scripts and its workflows, and how work on it is done. What the game does is its own design.

## Terms

| Term | Meaning |
|---|---|
| the engine | 3DEngine, used through its package alone |
| the package | The engine as published on nuget.org, at the version the project names |
| the engine checkout | A checkout of the engine's repository, which `./e3d` runs out of |
| the build workflow | What GitHub runs on a push, `.github/workflows/build.yml` |
| the pack run | A run of the pack workflow, by hand, which publishes the game native for each system |
| a batch | One finished piece of work, which is one commit |
| the working session | The session that writes the game |
| the reviewing session | The session that writes this file and REVIEW.md |

## How a rule is kept

A rule is kept by a test, by a script the workflow runs, or by review. A rule kept by review is
read by the reviewing session at each commit, and what it finds is a verdict in REVIEW.md.

## 1 Layout

**N 1.5 Every top folder of the repository, the hidden ones apart, has a row in AGENTS.md's table of
areas.** A reader finds a folder's purpose in one place. By review.

## 3 Tests

**N 3.1 A fault that is mended has a test that fails without the mend.** A fault with no test comes
back. By review.

**N 3.2 A test that cannot run on a machine is reported as skipped, with its reason.** A test that
passes because it did nothing hides what it was for. By review.

**N 3.5 A tolerance or a wait is changed only with the measurement that calls for it, in the
commit's message.** A number loosened without a measurement loosens forever. By review.

**N 3.6 A number in a document names the command that measured it.** A number nobody can measure
again is a claim. By review.

## 4 Documents

**N 4.1 Prose follows STYLE.md, in comments, messages, documentation and Markdown.** One voice. By
review.

**N 4.3 Every link in the README and the documents leads somewhere, and a link into this repository
is checked with no request made.** By review.

**N 4.7 A document or a comment a player or a contributor reads names no one who decided, no owner
and no session.** Documents speak for the game. By review.

**N 4.8 A picture a session makes while it works, a reading, a pair before and after a mend or a
capture sent to the owner, is saved in the repository in the commit of the work it belongs to.** The
pictures are the devlog's substance, and one left in a session's scratch space is gone with the
session, as a day's radiance cascade captures were. Each is a WebP under `.github/devlog/`, named by
the date and the item, as `2026-10-24-item-51-emitters.webp`, and its caption in DEVLOG.md, which is
generated from them, is the line the folder's `captions.md` gives it, the file's name then a bar
then one sentence, or the commit's sentence where the file has none; a picture of no item takes the
day and a name. An item that changes what is seen ends with at least one such picture, a pair before
and after where it mends, and an item built in phases one a phase, so the devlog shows the work as
it goes and not at a day's end alone; DEVLOG.md is regenerated as each item ends, and the commit
after carries it as it carries the review's files. A reading worth the devlog's reader with no
picture to carry it, a table of numbers or a few sentences, is saved beside the pictures as a
Markdown file named the same way, as `2026-10-25-item-122-resolution.md`, and DEVLOG.md carries it
on its day under the pictures. By review, the reviewer reading each commit for the pictures its
report names and regenerating DEVLOG.md as it archives an item.

**N 4.9 A picture shows what the words claim, and carries nothing for form's sake.** A plain
capture, a pair before and after or a collage of views on a white ground, as the sessions made them
before 2026-10-31, is the form by default, and the caption says which is which. Where a picture
reads something, the session adds what that reading needs and no more, as 3DEngine's canopy and
grove sheets of item 136 did. A title and a label a panel go where several panels could be confused;
a box is drawn at a place only where a number read there is given in the caption or the note, never
a box with no reading; a difference panel against a reference, or a plot, as the light hall's floor
against the turn in BevyCSharp's item 82, goes where it says more than the frames do; and every
panel keeps the same view and exposure. The ground stays white, with labels in dark text. The owner,
shown the kinds on 2026-10-31, keeps the plain form as it was and asks that a sheet's parts come
from its reading and not from a list. By review.

**N 4.10 Every picture a session makes is kept in the repository's dump folder, apart from the
devlog's chosen few.** N 4.8 keeps the pictures chosen for the devlog; the rest, every shot,
comparison and sheet a session makes while it works, went to its scratch space and was gone with the
session. Each goes as it is made, as captured, into `.github/dump/<date>/` under the repository,
named by the time, the item and a word, as `09-41-item-83-meadow-before.png`, with a line in that
day's `captions.md`, the file's name then a bar then one sentence, written as the shot is taken. The
folder is in `.gitignore` and never committed; the owner reads it and prunes it by day. A
recording's frames and any set of pictures past a few hundred megabytes go there as well, under a
folder of their own, and never into a session's scratch space, which is RAM-backed and held 8.8 GB
of one recording's frames on 2026-10-31 until the machine ran short and another session broke; a
finished experiment's frames are deleted at once. A session takes its shots there directly rather
than copying them after, so nothing is lost when a session ends or the machine restarts. Every
picture a session reads or compares is also shown to the owner in the session as it is made, with
its sentence, since the owner follows the work by its pictures and a capture compared behind the
scenes and never shown is work the owner cannot see. By review, the reviewer reading a day's folder
against the items worked.

## 6 Build and package

**N 6.1 A build has no warnings, and a warning fails the workflow.** A warning left standing hides
the next one. `-warnaserror` in the build workflow.

**N 6.2 The tests run on every desktop system the game ships for, on every push.** Path separators,
file locking and file events differ between the three. The build workflow.

**N 6.6 A script that more than one system runs uses only what each system's tools read.** The
engine's `ScriptTests` is the model; by review until the game has a test of its own.

**N 6.8 Every job of a workflow has a time limit, with its reason in a comment.** A job without one
runs six hours on a hang. By review.

## 7 Working

**N 7.1 Commits are made on `main`, and the reviewing session pushes them once the previous build
run of the repository has ended.** The owner gave the pushes to the reviewing session on 2026-10-19,
so a coder never pushes and no run is cancelled midway by a push. By review.

**N 7.2 A commit is one finished batch, and its message has the form COMMITS.md gives.** By review.

**N 7.3 REVIEW.md and this file have one writer, the reviewing session, and the working session
writes Replies alone.** By review.

**N 7.4 AGENTS.md changes on the owner's word, apart from a row of its table of areas that N 1.5
asks for.** By review.

**N 7.5 A branch is named for its work, never `claude/` nor a random string, and exists only while a
second session writes to the repository.** A session with `main` to itself commits on `main`. A
second writer, a cloud session among them, works on a branch such as `modeling`; the local session
cherry-picks its commits onto `main` with the repository's author and without a trailer, and the
reviewing session deletes the branch once its commits are on `main`. The owner asked for it on
2026-10-23. By review.

**N 7.6 Every commit's author and committer are the owner's identity, on every branch.** A session
sets `user.name` and `user.email` to the owner's before its first commit, a cloud session among
them, and a commit with any other identity is pushed nowhere; one that was is rewritten before it
goes anywhere, and a branch that carried one is deleted with its workflow runs. The owner asked for
it on 2026-10-23, having seen the cloud sessions' branches name another author as their sole
contributor. By review, the reviewing session reading the author of every commit it pushes or has
cherry-picked.

**N 7.7 A session spends tokens on what changes the work.** The weekly usage ran out in three days
twice, in October 2026, with four sessions working around the clock. A report to the reviewing
session is a few lines, the hash, what changed, a departure if any, the numbers that matter and the
checks, the detail in the commit's words and the devlog note; no log, table or file is pasted into a
message, a path is given. Before a batch and before a commit a session reads REVIEW.md's head, the
items it works on and the Replies, not the whole file, and reads a file's part by a search rather
than the file. A proposal is made once an item in a few lines and answered once; a reading is taken
once and recorded once. The whole suite runs once a commit, filtered runs while working. The
reviewing session records a commit in a few lines, the detail in the archive's item, the note and
the commit, and its watch wakes once an hour, the runs' ends reported by waits. By review, the
reviewer reading each report's and each record's length.

**N 7.8 A session runs a program offscreen or headless, and opens a window only to read what a
window alone shows.** The owner's word of 2026-10-30. A test, a reading, a capture and a drive run
offscreen or headless as they do; a window is opened on the owner's machine for the window's own
questions, the swapchain and its format, the modes and sizes, a screenshot read against an offscreen
capture, and closed when read. By review.

## Conformance

| Rule | Voxel-Sandbox |
|---|---|
| N 1.5 | to check at the first commit |
| N 3.1 | to check at the first commit |
| N 3.2 | to check at the first commit |
| N 3.5 | to check at the first commit |
| N 3.6 | to check at the first commit |
| N 4.1 | to check at the first commit |
| N 4.3 | to check at the first commit |
| N 4.7 | to check at the first commit |
| N 4.8 | by review |
| N 4.9 | by review |
| N 4.10 | by review |
| N 6.1 | to check at the first push |
| N 6.2 | to check at the first push |
| N 6.6 | to check at the first commit |
| N 6.8 | to check at the first commit |
| N 7.1 | kept |
| N 7.2 | to check at the first commit |
| N 7.3 | kept |
| N 7.4 | kept |
| N 7.5 | kept |
| N 7.6 | kept |

## How the norm changes

The reviewing session adds a rule alone when the rule comes with its check and no existing code
breaks it. A rule that existing code breaks, or that touches what is the owner's (dependencies, the
engine's version, AGENTS.md, what is published), waits for the owner's word.
