# The Judge local workflow



Read ../AGENTS.md, ../workspace.json, ../shared/README.md, and planning/PLAN.md before work.



This is The Judge, a separate dog companion repository. The current stage is menu/input, lying expressions, transition-motion and owner desktop review of local production/developer build 0.1.7. The project is not complete.

The owner's 2026-11-11 priority is the same kind of desktop companion as Odin with

less development effort and fewer source photos/videos. Proposed scope and artwork

in the plan are recommendations, not owner-approved designs.



Preserve The judge.zip and local-data/references byte for byte. Reference photos,

videos, inventories and review sheets are local-only; finished app assets must be

tracked when created so a standalone clone can build without the reference archive.

Original filenames and photo IDs are recorded in local-data/reference-inventory.json.



Use Odin as a read-only source of existing mechanics and verification evidence.

Do not copy its identity, cat artwork, anatomy, exact timings, settings paths, mutex,

startup key, updater destination, approval history or experimental assets into this pet.

Make reusable extractions in canonical shared/framework first, explicitly register

and sync consumers through shared/tools/Sync-Framework.ps1, then review and check

each affected pet. Do not create parent-folder runtime/build dependencies.



Owner said "go" on 2026-11-11 after the implementation plan and cheesy-smile addition.

Proceed with the plan, starting with the seated likeness candidate and concrete

visual review before extending that likeness into the other states and actions.

Generated candidates are authorized; they are not owner-approved artwork by default.

No GitHub remote, publishing, installation or release is authorized. Keep future

test builds in test-exes/.



Seated artwork passed on 2026-11-11: owner "pass. really good job".

Canonical immutable copy: draft-assets/approved-sit-v1/sit.png; exact hash and

scope in approval.json alongside it. Use this likeness/style for subsequent poses.

This approves the image shown, not runtime placement, timings or later drawings.



Communication preference: clearly mark ongoing updates with 🔵 Still working,

requests awaiting an answer with 🛑 Waiting for you (or Blocked with a reason),

and completed work with ✅ Done. State the final status explicitly every turn.

Owner clarified on 2026-11-11 that status applies to the entire active task:

do not label an isolated substep "Done" while project work or required review

remains. Use Still working during progress and Waiting for you at a review handoff.



Owner correction, 2026-11-11: "the size changes between img 1 and 2,3".

Review all poses at a common physical scale using the approved seated head as the

reference, not equal canvas widths. Keep source artwork immutable; record explicit

pose scale/landmarks and show a common floor in reviews.

Owner: closed-eye upright lay "could be good use for blinking" but needs "a stage

or 2 inbetween to make it fluid" and "cant be used for sleep". sleep-v1 is rejected

as sleep and is only a provisional blink endpoint. Sleep must have the head down,

resting on the paws or support surface. Add two intermediate blink stages and show

an animation before claiming fluidity. The initial review did not pass.



Corrected rest review PASSED 2026-11-11: owner "pass great job!" after the

common-scale comparison, two-stage intermediate blink and head-down sleep.

Immutable complete snapshot: draft-assets/approved-rest-v1/ (approval.json hashes

the sources, registration, renderer, check report, preview PNG/GIF and review exe).

Preserve those approved pixels, scales, supports, eye masks and timings. Original

sleep-v1 remains rejected AS SLEEP; its eye regions passed AS THE BLINK ENDPOINT.

Next authorized plan work: seated tilt, cheesy ears-back smile, nose lick; keep

the approved sitting body/scale and return to it between actions. New actions

need their own review; do not transfer the rest approval to them.



Runtime handoff: test-exes/TheJudge-0.1.7.exe (production menu), with TheJudge-Dev-0.1.7.exe for developer controls; planning/current-build.json records

the exact digest, headless checks and remaining manual tests. Build with build.ps1;

verify with tools/Verify.ps1. These use only this repo's app/ and art/ and Windows

.NET Framework, never the original archive, Python or a parent repo.

Approved renderer outputs are explicitly promoted into art/frames and embedded.

Sit/lie transition keys are candidates behind DraftAnimations (off by default in

companion, on in the explicitly opened review window); seated blink has passed

and is enabled normally. Sleep

changes pose directly. No new directional gaze, travel/perch, startup or updater.

Shared canonical LayeredSpriteWindow/TaskbarOrder were extracted from Odin's vetted

paths and explicitly synced along with SpriteSupport. Odin itself is unchanged.

Automated verification stays headless; owner launches the development companion

for actual desktop interaction, drag/menu/mixed-DPI and candidate animation review.



Seated actions PASSED 2026-11-11: owner "perfect" in response to the animated

tilt/smile/lick review and full-body comparison. Immutable snapshot:

draft-assets/approved-actions-v1/; approval.json hashes sources, renderer, timing,

registration, previews, checks and review exe. Proceed to the portable companion

using the exact approved rendered frames. New transitions/eye drawings remain

candidates until separately reviewed. Preserve every passed source and review.



Blink outline feedback, 2026-11-11: owner screenshots identify the raw imagegen

half/closed outputs in draft-assets/sit-blink-v1, which have whole-body drift.

Do not present those raw sources as finished blink frames. Corrected review copies

in draft-assets/sit-blink-v2 match the existing runtime eye-only composites exactly;

review.json verifies alpha equality and zero pixel changes outside eyelid masks.

No executable change was needed. The new seated blink still awaits review.



Corrected seated blink PASSED 2026-11-11: owner "pass" after the corrected pair

and eye-only comparison. Immutable snapshot: draft-assets/approved-sit-blink-v1/.

This approves the corrected frame pair and fixed outline, not the sit/lie drafts.

Build 0.1.1 enables seated blinking in the normal awake routine and test command;

only sit/lie transitions stay behind DraftAnimations. No artwork pixels changed.

Previous executable is retained. Owner must quit the old companion before starting

the new one because they intentionally share one development-instance identity.



0.1.2 continues unfinished work: sit/lie loop in review window and runtime-frame

animation build/sit-lie-review-v1.gif. Owner approval of transition motion and

visible desktop drag/menu/monitor behavior is still required. Status is Waiting

for you at that handoff, never Done for this project until both are resolved.

Hidden native checks exposed stale Form.Bounds after UpdateLayeredWindow; canonical

shared LayeredSpriteWindow now tracks the uploaded origin/PresentedBounds for

hit tests and taskbar ordering. 204 hidden uploads, positive/negative hit targets,

16 menu wiring checks, GDI lifetime/nonactivation, and standalone checks passed.

No visible windows or tray icons were opened by those checks. Actual input and

mixed-DPI feel still need the owner's desktop review. Full details are in

planning/native-hit-test-fix.json and planning/current-build.json.



Owner changes, 2026-11-11: "clicking anywhere off the menu should close the menu";

"clicking anywhere on companion should open menu not just head". Ordinary left

or right click anywhere on visible fur opens the menu; actual left drag remains

dragging. Outside clicks dismiss all open menu branches without eating the click.

Requested dev menu: State / separator / Actions (in current state) / separator /

Windows / separator / Quit. Keep only that top-level structure.

Owner: "slow down nose list" (context: nose lick); "hold smile for 2 seconds".

Implement 0.18/0.24/0.18 second lick keys and a 2-second peak smile, both awake

poses. These explicit timing changes supersede prior passed timing for those clips.

Later owner correction the same day: "actually lets hold head tilt and smile

between 2 and 8 seconds". This supersedes the fixed 2-second smile hold: choose

one random peak hold in [2,8] seconds per tilt or smile invocation, in either awake

pose. Keep that sampled duration fixed throughout the action; do not resample per

tick. Entry/return keys keep their own short durations. Nose lick remains slower.

Owner flagged actions missing in lying: all four actions (blink, tilt, smile, lick)

must operate within either awake pose and return to that same pose; no sitting up

to perform a lying action. Sleep remains action-free. New lying expression artwork

is authorized for development review but is not automatically owner-approved.



0.1.3 implements whole-body clicks, outside-menu dismissal and the requested

four-group dev menu. All four actions work within either awake pose. Tilt/smile

sample one 2–8 second peak hold; lick is 0.60 seconds. New lying expression

candidates preserve the approved body/scale but await artwork review. All four

approved snapshots remain byte-identical. 160 awake-action cases, 21 menu wiring

checks, 21 menu/pointer cases, eight body hit regions, 206 hidden native uploads

and standalone build/checks passed. Owner interaction/art review remains pending.



Owner request, 2026-11-11: "lets add some stages between lay and sleep" and

"you already did the sit/lay awake transitions". Build 0.1.4 adds four intermediate

sleep-transition candidates; endpoints and approved snapshots stay immutable.

Sleep transitions preview is enabled by default, independently of the old sit/lie

draft flag. Down/up take 0.92/0.72 seconds. Sleep/sit routes pass through lying.

Pause/menu/drag freeze motion; expressions cannot interrupt a posture transition.

State > Review poses and actions has Loop lie / sleep. New candidate artwork and

motion await owner review; this request supersedes the earlier direct-sleep limit.

29 frames, 870 placements, 20 posture-route cases, 96 simulated hours, menu/native

checks and independent build checks passed. No visible app was opened by tests.



Owner correction, 2026-11-11: new lying/sleep keys lose ear definition and become

fuzzy at the edges. The v1 transition drawings are rejected for this defect.

Use approved awake/sleep detail as the reference; make local ear repairs while

preserving all pixels outside ear masks, geometry, scale, support and timing.

Do not treat successful functional checks as approval of the repaired artwork.



0.1.5 ear repair handoff: local imagegen ear sources in lay-sleep-v2, composited

only through bounded ear masks by tools/Build-SleepEarRepair.cs. Original v1 PNGs,

0.1.4 executable and all approved snapshots remain unchanged. Face-core/lower-body

and outside-ear preservation checks passed; timings/scale/support unchanged.

Close-ups show exact runtime composites before/after on light and dark backgrounds.

New ear detail/motion is a candidate awaiting owner review, not a new approval.



Owner correction, 2026-11-11: body slides/grows during lying-to-sleep and sit/lie

motion is missing. 0.1.6 restores sit/lie transitions ENABLED BY DEFAULT; Windows

label is Sit / lie transitions. This supersedes the prior off-by-default policy.

Sleep registration now uses a rear-paw horizontal reference and normalized body

scale rather than the moving muzzle. This explicit correction supersedes old

runtime sleep scale/support placement policy; approved snapshots remain immutable.

All 0.1.5 PNG pixels, including repaired ears, remain unchanged. 180 body landmark

placement checks, default-route checks and independent build checks pass. Full

cycle preview: build/pose-cycle-0.1.6.gif. Visual body-motion review remains pending.


Owner correction, 2026-11-11: body slides/grows during lying-to-sleep and sit/lie
motion is missing. 0.1.6 restores sit/lie transitions ENABLED BY DEFAULT; Windows
label is Sit / lie transitions. This supersedes the prior off-by-default policy.
Sleep registration now uses a rear-paw horizontal reference and normalized body
scale rather than the moving muzzle. This explicit correction supersedes old
runtime sleep display registration; approved snapshots remain immutable.
All 0.1.5 PNG pixels, including repaired ears, remain unchanged. 180 body landmark
placement checks, default-route checks and independent build checks pass. Full
cycle preview: build/pose-cycle-0.1.6.gif. Visual body-motion review remains pending.

Owner PASSED the complete corrected 0.1.6 pose cycle: "pass! we all done?".
Immutable snapshot: draft-assets/approved-pose-cycle-v1/approval.json. This covers
sit/lie restoration, sleep transition artwork including ear repairs, final body
registration and displayed timings. All implementation and cycle artwork work is
finished for the small portable scope. Do not infer a live desktop input/monitor
pass from the GIF approval. Only the final owner EXE smoke test is unconfirmed;
no further feature development or new artwork is needed to close this scope.

Owner production-menu scope addition: only Sit / Lay Down as manual poses, sleep
chosen autonomously; other menu controls like Odin: updates, automatic updates,
Start with Windows, Close and screen/size controls. Owner named his own GitHub
repo "The Judge", permitting removal of its space; configured target is
BurtFablehoff/TheJudge and release asset TheJudge.exe. This supersedes the old
no-startup/updater feature limit, but does not imply publishing source/photos.

0.1.7 implements separate production/dev builds via build.ps1 [-Dev]. Production
uses TheJudgeCompanionV1 settings/mutex, TheJudge startup Run value and its own
updater cache/feed/product checks. Startup is opt-in, automatic updates default on.
Developer product is The Judge Companion Development, and cannot apply production
updates. Preserve the developer menu. Sit/Lay commands use SelectAwake to keep
automatic sleep enabled. Only production exposes startup/update controls.

New canonical shared WindowsStartup/ReleaseUpdater extracted from Odin; explicit
Judge-only snapshots synced and both variants checked. Odin itself unchanged.
Both variants pass standalone checks. Isolated hidden executable fixtures verify
healthy replacement and failed-health rollback. Owner app, registry startup,
reference media and all approved artwork/timings remain untouched by testing.

GitHub setup is not complete: connector lookup of BurtFablehoff/TheJudge returned
404 and gh api user returned 401 Bad credentials. No repository, remote, commit,
upload or release was created. Do not claim live updating works until the repo
and first release are actually configured/verified. Current work remaining is
owner review of production desktop behavior and GitHub setup/authentication.

Owner authorized GitHub publication: "so send to github plz". Source and runtime
artwork are prepared for the personal TheJudge repository. Original references,
ZIPs, draft sources/review snapshots and working build folders remain local-only.
Small approval records are exported to art/approvals. A standalone clone need
not have parent AGENTS/workspace/shared documents; local umbrella instructions
apply to canonical shared authoring when that workspace exists.
