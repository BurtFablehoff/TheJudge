# The Judge desktop companion

Version 0.1.7 has separate production and developer menus. This is a portable
Windows companion, built independently from this repository.

Download **TheJudge.exe** from [the latest release](https://github.com/BurtFablehoff/TheJudge/releases/latest).
No installer is required; keep the EXE in a writable folder and run it.

## Run the production menu

Quit older companions, then open `test-exes/TheJudge-0.1.7.exe`. Left- or right-click
any visible part of him; clicking outside dismisses the menu. Left-drag moves him.
The production menu follows Odin's sections:

- Size: Small, Normal, Large.
- Options: **Sit** and **Lay Down**, the only manual pose commands.
- Windows: **Start with Windows**, **Automatic updates**, **Check for updates**.
- Screen: **Bring to this screen**, **Send to other screen**.
- **Close**.

He chooses sleep himself. Choosing Sit or Lay Down preserves his automatic routine
and expressions; it does not pin an awake state forever. The approved transitions,
repaired ear pixels, 2–8 second tilt/smile holds and slower nose lick are unchanged.

Start with Windows is opt-in and uses only the per-user `TheJudge` Run value.
Production settings/mutex are `TheJudgeCompanionV1`; development uses the separate
`TheJudgeCompanionDevV1` identity. Development copies never register startup or
apply production updates. Use `test-exes/TheJudge-Dev-0.1.7.exe` for the existing
State / Actions / Windows / Quit developer controls and animation review.

The updater targets **BurtFablehoff/TheJudge**, with a release asset named
`TheJudge.exe`. It reuses Odin's digest, binary-identity, version, healthy-start and
rollback checks through an explicit shared-source snapshot. Automatic updates are
on by default and can be switched off. The updater reads this repository's latest published release. The public source
package excludes all original reference photos/videos, ZIP files and local
draft/review folders. Small approval records are retained in `art/approvals/`.

Both variants pass routine, menu/native and product checks and build independently
from `app/`, `art/` and `build.ps1`. Hidden isolated executable fixtures pass a healthy
update handoff and failed-health rollback. Tests opened no visible windows and
wrote no startup entries or owner settings. All five artwork approval snapshots
are intact; the approved 21-frame pose cycle is pixel- and timing-identical.

Current hashes, checks and remaining live-desktop review:
[build record](planning/current-build.json). Production menu preview:
`build/production-menu-0.1.7.png`.

## Build from this repository alone



With Windows PowerShell and the .NET Framework C# compiler:



```powershell

powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1

powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Verify.ps1

```



Builds embed `art/frames/*.png` after checking `art/manifest.json` digests. They

never regenerate artwork or sync shared files and need no Python, installations,

original reference ZIP, Odin clone or parent-folder dependencies.



Reusable framework source is snapshotted into `app/Shared*.cs`; its canonical

source lives in the separate local shared repository. [Provenance](planning/shared-framework.json)

records the explicit sync. Edit canonical source first, deliberately run

`../shared/tools/Sync-Framework.ps1 -Pet thejudge`, review and verify. That authoring

command is not a build dependency.



`tools/Promote-RuntimeArt.ps1` explicitly exported the passed renderers into the

runtime art directory, retaining the passed scale/masks/timing. It is separate

from normal builds and refuses to replace existing different pixels. Passed

source/review snapshots remain unchanged.



## Approved artwork reviews



Approved seated likeness: [sit-v1](draft-assets/approved-sit-v1/sit.png), passed

2026-11-11. Exact approval is alongside it; original prompt and generation metadata

remain in `draft-assets/sit-v1/`.



Current review: run `tools/Build-ArtReview.ps1`, then open

`test-exes/Judge-ArtReview-v1.exe`. It shows the approved sit, awake lying with a

four-stage blink, and the new [head-down sleep](draft-assets/sleep-v2/sleep.png)

at a common display head scale and floor. Space pauses; B switches light/dark.

The poses, scale and blink in this rest review PASSED on 2026-11-11

("pass great job!"). Its immutable approval

snapshot is in `draft-assets/approved-rest-v1/`.



Headless exports: `build/art-review-v1.png`, `build/blink-review-v1.gif` and

`build/art-review-v1.json`. Source prompts are saved with each draft. The former

`sleep-v1` head-up image was rejected as sleep and is only a blink endpoint source.



## Approved action review



Run `tools/Build-ArtReview.ps1 -ActionReview`, then open

`test-exes/Judge-ActionReview-v1.exe`. It previews seated tilt, cheesy ears-back

smile and nose lick, each returning to the unchanged approved sit. Space pauses,

B switches light/dark, and D switches between full bodies and face detail.

All three actions PASSED on 2026-11-11 ("perfect"); their immutable snapshot is in

`draft-assets/approved-actions-v1/`. Exports: `build/action-review-v1.png`, `build/actions-review-v1.gif`

and `build/action-review-v1.json`. Prompts and source metadata are saved in

`draft-assets/sit-tilt-v1/`, `sit-smile-v1/` and `sit-lick-v1/`.



## Local references



- `The judge.zip`: unchanged original archive, retained at the repository root.

- `local-data/references/`: 27 extracted photos and 14 short MOV clips, unchanged.

- `local-data/reference-inventory.json`: original archive entries, photo IDs and SHA-256 hashes.

- `local-data/video-inventory.json`: clip lengths, frame counts and frame rates.

- `local-data/review/photos-01.jpg` through `photos-03.jpg`: numbered photo contact sheets.

- `local-data/review/video-samples-01.jpg` through `video-samples-03.jpg`: five samples per clip.



Personal reference media and review derivatives are ignored by Git. Keep a separate

backup of the original archive. Finished runtime sprites and required build inputs

are self-contained in this repository for independent builds.



Build 0.1.6 restores sitting/lying transitions in the normal flow and uses a rear-body

reference through the sleep sequence. See `build/pose-cycle-0.1.6.gif` for the full

sit / lie / sleep / lie / sit cycle. Approved images and the repaired ear pixels

remain unchanged. Body registration/motion still awaits owner visual review.


The complete 0.1.6 pose cycle PASSED owner review ("pass! we all done?"). The
immutable snapshot is `draft-assets/approved-pose-cycle-v1/`. Feature and artwork
implementation for this portable scope is finished. The final actual desktop
smoke test (menus, dragging, screen placement) is unconfirmed; GIF approval does
not validate native desktop input. No installation/release work is in this scope.

## GitHub publication

Source and finished runtime artwork are published at
https://github.com/BurtFablehoff/TheJudge. Original
photos, videos, ZIP and local draft/review folders are excluded from publication.
Small immutable approval records are exported to art/approvals. The standalone
build only requires app/, art/ and build.ps1; local artwork-authoring helpers may
require the ignored draft sources. The intended release asset is TheJudge.exe.
