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
