# Benchmark (N.I.N.A. plugin) - support knowledge base

**This is written for a support agent, not for a user.** Quote it, do not paraphrase
it: the sentences here are checked against the source, a paraphrase is not.

Applies to **0.6.3.0**. To check what the user is running: the version is shown next
to **Benchmark** in N.I.N.A.'s plugin list.

**The plugin's own interface is in English only, even when N.I.N.A. itself is in
French or German.** So a French user will still quote English labels at you
("Run benchmark", "Clear all", "Share"). Every label, button and message in this
document is the exact string the plugin displays. If a user quotes a label you cannot
find in this document, say so and escalate rather than guess which control they mean.

**Numbers are formatted by the user's Windows locale, not by the plugin.** The same
score is shown as `84.2` on an English Windows and `84,2` on a French one, and sizes
likewise (`179.6 MB` or `179,6 MB`). Every figure quoted in this document uses the dot
form. Match on the digits, not on the separator, and never tell a user their number
"looks wrong" because of a comma.

**Never invent a figure, a path, a version or a compatibility claim.** This plugin
exists to publish measured numbers, and support has to hold the same line. If the
answer is not in this document, the correct answer is *"I don't know, I'm passing this
to the team."*

- Repository and issue tracker: https://github.com/caelo-works/nina.plugin.benchmark
- Plugin page: https://nina-plugins.caelo.works/en/plugins/benchmark
- Leaderboard and run pages: https://nina-benchmark-plugin.com

---

## The product card: what the Benchmark plugin is

Benchmark is a **N.I.N.A. plugin** that times the real N.I.N.A. and Accord
image-analysis routines of the post-capture pipeline (debayer, stretch remap, resize,
blur, edge detection, threshold, dilation, blob counter, star detection) over a set of
downloaded test frames, and turns the total into a single comparable **score**. The
run can then be submitted to a public leaderboard.

- **Version:** 0.6.3.0
- **Licence:** MPL-2.0, free and open source
- **Requires N.I.N.A. 3.2 or newer.** Windows x64 only (the plugin is a
  `net8.0-windows` WPF assembly, like N.I.N.A. itself).
- **Appears as:** the **Benchmark** page in N.I.N.A.'s **Plugins** tab, plus two
  dockables on the **Imaging** view (**Benchmark** and **Benchmark System**).
- **Test frames are not bundled.** They are downloaded once (about 190 MB) from
  https://nina-benchmark-plugin.com and cached locally.

**The score is `100000 / total milliseconds`, rounded to one decimal. Higher is
faster.** The total counts the primitives only. **StarDetection (full) is measured but
deliberately excluded from the total**, because it is a superset of the primitives and
would count them twice.

**What the plugin writes on disk.** Everything lives under
`%LOCALAPPDATA%\NINA\BenchmarkPlugin\`:

- `TestImages\` - the downloaded test frames.
- `testset.json` - the manifest of the downloaded set (names, sizes, sha256).
- `history.json` - the saved run history.
- `settings.json` - the Machine name field.

Deleting that folder resets the plugin completely: the frames must be downloaded
again and the history is lost.

---

## Installation: how to install the Benchmark plugin

### From N.I.N.A.'s plugin manager

The plugin is published in N.I.N.A.'s plugin manager. It appears under
**Plugins → Available** as **Benchmark**, author **CaeloWorks**. Install it, then
**restart N.I.N.A.**

### Manual install

Download `CaeloWorks.NINA.Benchmark.dll` from the releases page
(https://github.com/caelo-works/nina.plugin.benchmark/releases) and drop it into
`%LOCALAPPDATA%\NINA\Plugins\<N.I.N.A. version>\`, then restart N.I.N.A.

The plugin ships a **single DLL**, `CaeloWorks.NINA.Benchmark.dll`. There is nothing
else to install and no separate download of the test frames at this stage: the frames
are fetched later, from inside the plugin.

### "I installed it and I can't find it"

Almost always one of these, in this order:

- **N.I.N.A. was not restarted** after the install. This is the number one cause.
- They are looking on the **Imaging** view but have not added the panel. The plugin
  adds two dockables there: **Benchmark** (in the **Tools** group of the panel list)
  and **Benchmark System**. Both must be enabled from the Imaging view's panel list
  before they show up.
- They are looking for a sequencer instruction or a trigger. **There are none.** This
  plugin adds no sequencer item, no trigger, no condition and no device. It is a page
  plus two dockables, nothing else.

The always-available entry point is the **Benchmark** page in the **Plugins** tab. If
it is there, the plugin is correctly installed.

---

## The plugin surface: pages, dockables and buttons

The plugin has **one page** and **two dockables**, and they all drive the **same
shared engine**. A benchmark started from the Imaging dockable is the same run, with
the same history, as one started from the Plugins page. Users often think there are
two separate tools. There are not.

### The Benchmark page in the Plugins tab

Found at **Plugins → Benchmark**. It stacks three blocks, top to bottom:

1. **System information** - the same content as the Benchmark System dockable.
2. **Benchmark results** - the same content as the Benchmark dockable.
3. **Sharing (submit results online)** - the Machine name field. This block exists
   **only here**, not on the dockables.

### The "Benchmark" dockable (Benchmark results)

Title: **Benchmark**. It sits in the **Tools** group of the Imaging view's panels.
Its box is headed **Benchmark results**. It contains:

- **Run benchmark** - starts the run. It is only present once the test frames are
  downloaded and verified.
- **Cancel** - only visible while a run (or a download) is in progress.
- **Clear all** - tooltip *"Remove every saved benchmark run from the history"*.
  **It deletes the whole history immediately, with no confirmation dialog.** See the
  known-limits section.
- **Best score:** followed by the highest score in the kept history.
- A **progress bar** and a **status line** to its right.
- **Latest run (per function)** - the breakdown of the most recent run, ending with
  the line **"Total (primitives only = score basis): N ms"**.
- A **history table** with the columns: (buttons), **Date**, **Score**,
  **Total (ms)**, **Power plan**, **NINA**.

Each history row carries buttons: **Details** (expands the row to show
**System at test time** and **Per-function timing**), then either **Share** (if the
run was never submitted) or **View** (if it was, opening its public page in the
browser).

### The "Benchmark System" dockable (System information)

Title: **Benchmark System**. Its box is headed **System information**. It shows a
plain monospace block and a **Refresh** button. The block lists, in this order:
**CPU**, **Cores** (physical / logical), **Max clock**, **GPU**, **RAM**
(free / total), **Power plan**, **OS**, **.NET**, **N.I.N.A.**

The **Max clock** line is omitted when the value could not be read. The snapshot is
refreshed automatically at the start of every benchmark run, so the values stored with
a result reflect the machine at run time.

### The "Sharing (submit results online)" panel

On the **Plugins → Benchmark** page only. One field:

- **Machine name** - tooltip *"Optional label for this rig. Defaults to the active
  profile name if left empty."* Optional, saved as you type.

The panel then states, in grey, that the **Share** button submits the run to
`https://nina-benchmark-plugin.com/api/runs` **anonymously, with no sign-up**, and
that what is sent is the machine specs (CPU, GPU, RAM, OS, power plan) and the
per-function timings, **no images**.

---

## Downloading the test frames (the test set)

The benchmark frames are **not shipped with the plugin**. Until they are downloaded,
the Benchmark results box shows no Run button at all: it shows a download icon, a
message, and a single **Download test set** button. This surprises people who expect
to press Run straight away, and it is normal.

**The message shown when the frames are absent** is, word for word:

> The benchmark test frames aren't on this machine yet. They're downloaded once from
> the sharing site (~190 MB) and cached for every future run.

**While downloading**, the box shows **Downloading test set…**, a progress bar, a line
like `128.4 MB / 179.6 MB · 51.2 MB left` on the left, the throughput
(e.g. `12.4 MB/s`) on the right, and a **Cancel** button. On a French Windows the same
line reads `128,4 MB / 179,6 MB`: the separator follows the OS locale.

**What is actually downloaded:** four FITS frames, about **188 MB** in total, from
`https://nina-benchmark-plugin.com`. They are stored in
`%LOCALAPPDATA%\NINA\BenchmarkPlugin\TestImages\`. Each file's **sha256 is checked as
it lands**, and the whole set is **re-verified before every single run** (that is the
**"Verifying test set…"** status). A corrupted or incomplete set is rejected and the
user is sent back to the download prompt.

**"It said 190 MB but the bar only counts to 179.6 MB."** Both figures are right and
nothing is missing. The message announces the size in decimal megabytes (about
190 MB), while the progress bar counts in binary megabytes (188,326,080 bytes is
179.6 MB when divided by 1024). The download is complete. Reassure and move on.

**The download is resumable only by restarting it.** Cancelling shows *"Download
cancelled. Click to try again."* and the partial file is discarded. There is no
pause/resume.

**If the cached frames go bad**, the message becomes:

> The cached test frames are missing or don't match their checksums. Download them
> again to run the benchmark.

The fix is always the same: click **Download test set** again. It is safe, it
overwrites, and it costs only the download.

---

## The score and the per-function breakdown

**The score is `100000 / total milliseconds`, rounded to one decimal. Higher is
faster.** A machine twice as fast scores twice as high. The figure has no unit and no
absolute meaning: it exists to be compared with other runs of the **same test set**.

**Each function is timed the same way: one warm-up pass that is thrown away, then the
mean of 3 runs.** The number of runs is **fixed at 3 and is not user-configurable** in
0.6.3.0. There is no setting for it anywhere in the interface, so do not send users
looking for one.

The breakdown lists these functions, under exactly these names:

- `BayerFilter16bpp (debayer)`
- `ColorRemappingGeneral (stretch)`
- `FastGaussianBlur`
- `ResizeBicubic`
- `CannyEdgeDetector`
- `NoBlurCannyEdgeDetector`
- `Convolution (LoG 5x5)`
- `SISThreshold`
- `BinaryDilation3x3`
- `BlobCounter`
- `StarDetection (full)`

**`StarDetection (full)` is marked `(not in total, superset)` and is excluded from the
score.** It re-runs the whole detector, which internally repeats the primitives above;
counting it would count them twice. This is deliberate, it is not a bug, and it is the
single most common "is this broken?" question about the breakdown.

**`BayerFilter16bpp (debayer)` is only timed on the colour frame.** The test set holds
four frames and only one of them is an OSC/Bayer frame, so the debayer step runs on
that one alone. Its time is therefore much smaller than the others, and that is
correct.

**Why StarDetection (full) varies between machines with the same score:** it is run
with the **active N.I.N.A. profile's** image settings (auto-stretch factor, black
clipping, star sensitivity, noise reduction) and with **whichever star-detection
behaviour is selected in the profile** (a star-detection plugin such as Hocus Focus
replaces it). The primitives, and therefore **the score, are unaffected by profile
settings** and stay comparable. If a user asks why their StarDetection line differs
from a friend's, this is why, and their score is still valid.

---

## Sharing a run to the leaderboard

**Sharing is anonymous and needs no account.** On a history row, the **Share** button
submits that run to https://nina-benchmark-plugin.com. When it succeeds:

- the status line reads **"Shared. Link copied: <url>"**, and the run's URL is
  **copied to the clipboard automatically**;
- the row's **Share** button becomes **View**, which opens the run's public page in
  the default browser;
- the link survives a restart of N.I.N.A., because it is saved in the history.

**What is sent:** the machine specs (CPU, cores, clock, GPU, RAM, OS, .NET, power
plan, N.I.N.A. version), the per-function timings, the score, and the two labels
below. **No images are sent, ever.**

**How a run is labelled on the leaderboard, and this is public:**

- the **nickname** is the **Observer name configured in N.I.N.A.**, found at
  **Options → General → Astrometry** (truncated to 40 characters);
- the **machine** is the **Machine name** field from the Sharing panel, or, if that
  field is empty, **the name of the active N.I.N.A. profile** (truncated to 60
  characters).

A user who does not want their real name on a public leaderboard must change their
N.I.N.A. Observer name **before** sharing. **It is at Options → General → Astrometry,
in the Observer field.** (N.I.N.A.'s own menus are translated, so a French user sees
that path in French, unlike the plugin's panels which stay in English.) Point this out
if they seem to be sharing under a real name by accident.

**Submissions are signed.** The plugin fetches a single-use nonce from the site and
signs the run, so the server can tell that a submission came from a genuine plugin
build and was not tampered with or replayed. Users never see any of this unless it
fails, in which case they get one of the "Submit failed" messages listed in the
error-message section.

**A run can only be submitted once.** After that the button is **View**, not
**Share**. There is **no way to delete or edit a submitted run from inside the
plugin**: that requires a human. See the escalation section.

---

## Error messages, word for word

The user will paste the message. These are the exact strings, and where they appear.

### Messages in the status line during a run

- **"Test set missing or corrupted. Download it again."** The sha256 re-verification
  that runs before every benchmark failed. Click **Download test set** again.
- **"No test frames found. Download the test set first."** The frames folder is empty.
  Same fix.
- **"Cancelled"** The user pressed **Cancel**. Nothing is saved, nothing is broken.
- **"Error: <message>"** Any other failure during the run. Ask for the exact text and
  escalate: this one is not covered by a known cause.
- **"History cleared"** Confirms **Clear all** wiped the history. It is not
  recoverable.
- **"Done. Score 84.2 (1188 ms total)"** Normal end of a successful run. The decimal
  separator follows the user's Windows locale, so a French user reads
  *"Done. Score 84,2"*.

### Messages while downloading the test frames

- **"Download cancelled. Click to try again."** The user pressed **Cancel**. Harmless.
- **"Checksum mismatch on <file>. The download may be corrupted. Please retry."** The
  file arrived damaged (flaky connection, proxy, antivirus rewriting the stream). The
  partial file is deleted. Retrying is the fix and usually works.
- **"Download failed: <message>"** Network or site error. Have them check their
  connection and retry.
- **"The site reports no test frames available."** The site answered with an empty
  manifest. This is a **server-side** problem, not the user's. Escalate.

### "Submit failed": what the server refused

These all appear as **"Submit failed: Server returned <code>. <body>"**.

- **`401` with `Unrecognized or unsupported plugin version.`** The plugin build is too
  old for the site to accept. **Have them update the plugin to the current version and
  run the benchmark again.** An old run in the history cannot be rescued: it has to be
  re-run on the new build.
- **`401` with `Invalid or expired nonce.`** or **`Signature verification failed.`**
  The signed submission was refused. Have them simply **click Share again**: the
  plugin takes a fresh nonce each time and a one-off network hiccup explains most of
  these. If it repeats, escalate.
- **`422` with `Run was not produced against the current official test set.`** See the
  known-limits section: the run is older than the current official frames. **A fresh
  benchmark run fixes it.**
- **`422` with `Inconsistent timings (sum ≠ total).`** or **`Score does not match the
  total time.`** The server's consistency checks rejected the run. This should never
  happen on an untouched plugin. Escalate.
- **`429` with `Too many submissions, slow down.`** The site rate-limits submissions
  per IP. Wait a minute and share again.
- **`400` with `Invalid JSON`** or **`422` with `Invalid payload`** The site could not
  read what the plugin sent. This should never happen on an untouched plugin, and the
  usual cause is a hand-edited or corrupted `history.json`. Escalate.
- **`500` with `Database error`** A site-side failure. Escalate.

Two more, both raised before the run is even sent:

- **"Could not get a submission nonce (<code>). <body>"** The site could not be
  reached, or refused, when the plugin asked for its single-use token. Check the
  internet connection and retry.
- **"Server returned no nonce."** Site-side problem. Escalate.

---

## Known bugs and limits: read before answering

**There is no open bug in 0.6.3.0.** What follows are real limits of the current
build. They are not user mistakes: confirm them, do not send the user back to their
settings to hunt for an error they did not make.

### "Clear all" wipes the history instantly, with no confirmation

**Symptom:** *"I clicked Clear all and everything is gone, including the links to my
shared runs."*

**Cause:** **Clear all** deletes every saved run immediately. There is **no
confirmation dialog and no undo** in 0.6.3.0. It rewrites `history.json` on the spot.

**What survives:** the runs that were **already shared** are still online and still on
the leaderboard. Only the local history and the local copies of their links are lost.
If the user shared a run, its page still exists, and they can find it again on the
leaderboard at https://nina-benchmark-plugin.com/leaderboard. If they never shared it,
**it is gone**. Do not promise a recovery.

### Only the last 25 runs are kept

**Symptom:** *"My oldest benchmark runs disappeared on their own."*

**Cause:** the history keeps a maximum of **25 runs**. When a 26th is added, the
oldest one is dropped silently. There is no warning and no setting to raise the limit.
Runs that were shared remain online regardless; it is only the local list that is
trimmed.

### An old run in the history can no longer be shared

**Symptom:** *"Share fails on my old runs but works on the new one."* The message is
`422` with `Run was not produced against the current official test set.`

**Cause:** a submission is tied to **the version of the test frames the run was
measured on**. When the official test set is updated, the runs already sitting in the
history become unsubmittable. In particular, **runs recorded before plugin 0.6.0.0
carry no test-set version at all and can never be submitted.**

**The fix takes one click: run the benchmark again, then share the new run.** Nothing
is broken, and the score is not affected.

**Do not confuse this with `401` `Unrecognized or unsupported plugin version.`** That
one is not about the age of the run at all: the plugin always sends **the version it
is currently running**, so a `401` means **the installed plugin build is too old for
the site**. Re-running the benchmark on an old build will not fix it. **They must
update the plugin first**, then run the benchmark again.

### The score only compares runs made on the same test set

The score is `100000 / total ms`. It is a **relative** figure. Comparing it with a
score obtained on a different version of the test frames is meaningless, which is
exactly why the site refuses runs from an outdated set. If a user says their score
"changed for no reason" after re-downloading the frames, ask whether the test set
version changed, and escalate if in doubt.

### Sharing publishes the N.I.N.A. Observer name

Not a bug, but it catches people. A shared run is labelled with the **Observer name
set in N.I.N.A.** and with the **Machine name** (or the active profile's name if that
field is empty). Both are **public** on the leaderboard.

**To change it before sharing: Options → General → Astrometry, in the Observer field.**
Then run the benchmark again and share that run. (N.I.N.A.'s menus are translated, so
a French user sees that path in French; the plugin's own panels stay in English.)

**To remove a name from a run that is already shared: escalate.** It cannot be done
from the plugin, and there is no self-service way to delete or rename a published run.
Do not promise it, and do not suggest a workaround.

---

## Troubleshooting: symptom, cause, answer

**"There is no Run benchmark button."**
The test frames are not downloaded yet. The box shows a **Download test set** button
instead. Click it once (about 190 MB); the Run button appears when the frames are
verified.

**"It says my test set is corrupted / it keeps re-verifying."**
The frames are re-checked (sha256) before every run, and a damaged set is rejected.
Click **Download test set** again. It overwrites the cache and fixes it.

**"The download failed with a checksum mismatch."**
The file arrived damaged. Retry the download: it discards the partial file and starts
clean. A proxy, a captive portal or an antivirus scanning the stream is the usual
cause when it repeats.

**"Share does nothing / Submit failed."**
Read the exact message. If it says `Run was not produced against the current official
test set.` or `Unrecognized or unsupported plugin version.`, the run is too old:
**update the plugin if needed, run the benchmark again, and share the new run.** If it
says `Invalid or expired nonce.` or `Signature verification failed.`, just click
**Share** again.

**"StarDetection is huge and it is not counted in my score."**
Correct, and deliberate. `StarDetection (full)` is a superset of the other functions
and is marked `(not in total, superset)`. Counting it would count the primitives
twice. The score is built from the primitives only.

**"My friend's StarDetection time is completely different from mine."**
That line depends on the active N.I.N.A. profile's image settings and on the
star-detection behaviour selected in the profile (a plugin like Hocus Focus replaces
it). The **score is not affected** by any of that and remains comparable.

**"The debayer line is tiny compared to the rest."**
`BayerFilter16bpp (debayer)` is only timed on the one colour (OSC) frame of the test
set; the other frames are mono. It is expected.

**"My history is empty / my old runs vanished."**
Either **Clear all** was pressed (immediate, no undo), or the history hit its **25-run
cap** and the oldest were dropped. Runs already shared are still online on the
leaderboard.

**"I installed it and I don't see it."**
Restart N.I.N.A. Then look at **Plugins → Benchmark**. The two Imaging dockables
(**Benchmark**, in the **Tools** group, and **Benchmark System**) must be enabled from
the Imaging view's panel list before they appear.

**"Can I use it in a sequence?"**
No. The plugin adds no sequencer instruction, no trigger, no condition and no device.
It is a page plus two dockables.

---

## Escalation: when to stop and hand over to a human

**Escalate, and do not improvise, when:**

- the user wants a **shared run deleted, renamed or hidden** from the leaderboard, for
  example because it carries their real name. **The plugin cannot do it.** Do not
  promise it, do not suggest a workaround: hand it to the team.
- the submission fails with **`Inconsistent timings (sum ≠ total).`**, **`Score does
  not match the total time.`**, **`Database error`**, **`Server returned no nonce.`**
  or **"The site reports no test frames available."** These are site-side or
  integrity failures, never a user mistake.
- a **"Submit failed"** error repeats after the user has already updated the plugin
  and re-run the benchmark.
- the status line shows **"Error: <something>"** during a run and the message is not
  one of the known ones in this document.
- the user reports a score they believe is wrong, or suspects someone cheated on the
  leaderboard.
- anything about payment or licensing beyond "it is free and MPL-2.0".
- the question is not covered by this document. Say *"I don't know, I'm passing this
  to the team."* A plausible-sounding guess is worse than silence.

**Collect these five things before escalating.** Without them the report is not
actionable:

1. The **Benchmark plugin version** (shown next to **Benchmark** in N.I.N.A.'s plugin
   list) and the **N.I.N.A. version**.
2. The **exact error text**, copied, not described.
3. A **screenshot of the Benchmark results box**, which shows the status line, the
   score and the per-function breakdown in one image.
4. The **System information block** (the Benchmark System dockable, or the top of the
   Plugins → Benchmark page). It can be selected and copied as text.
5. For a sharing problem: **the run's date and score** from the history table, and
   whether the run was recorded **before or after** they last updated the plugin.

Bugs can also be filed directly at
https://github.com/caelo-works/nina.plugin.benchmark/issues
