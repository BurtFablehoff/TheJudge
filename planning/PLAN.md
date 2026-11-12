# The Judge implementation plan



Date: 2026-11-11. Status: implementation in progress; explicit approvals are recorded below. Transition motion and visible desktop review remain pending.

Owner scope addition, 2026-11-11: "the cheesy ears back look that resembles a smile"

is included in the first version. Exact artwork and timing remain to be reviewed.

Owner said "go" on 2026-11-11 to begin the plan. First seated likeness candidate:

`draft-assets/sit-v1/sit.png`; prompt and generation/alpha checks are alongside it.

Seated artwork passed on 2026-11-11 ("pass. really good job"). Canonical copy:

`draft-assets/approved-sit-v1/sit.png`, with exact approval/hash in `approval.json`.

Not yet integrated into the desktop companion. Corrected rest review passed;

next is seated tilt, cheesy ears-back smile and nose lick.



Rejected review pair (2026-11-11): `draft-assets/lay-v2/lay.png` and

`draft-assets/sleep-v1/sleep.png`, generated from the approved seated likeness and

actual lying references. Both are unapproved candidates with saved prompts, hashes

and alpha/margin checks. `lay-v1` is retained but superseded because paw/tail reached

the canvas edges. Owner rejected the pose-size changes and sleep-v1 as sleep:

"cant be used for sleep. animals don't sleep with their heads up". The old sleep

image is only a provisional closed-blink endpoint; two intermediate eyelid stages

are required. It depicts a head-up closed-eye expression;

its generated body pixels are not exactly identical to the awake image. Register

the eye changes against a stable body before runtime animation. Physical display

scale and contact anchors are not defined by these raw canvas sizes.



Corrected review, 2026-11-11: `test-exes/Judge-ArtReview-v1.exe` embeds the approved

sit, scaled awake lay, new head-down `sleep-v2`, and four blink stages (open, early,

late, closed), played forward then backward. `tools/Build-ArtReview.ps1` builds it

and runs headless checks/exports; it never opens the review window automatically.

`planning/art-review-v1.json` records eye-span scale estimates, support rows and

eye masks. The approved sit is unchanged. New lay and sleep display scales are

about 1.34 times their raw artwork relative to sit, bringing the head sizes into

the same range. Sleep's different head pitch still needs visual assessment.

The runtime blink uses only generated eyelid pixels inside two feathered eye masks:

head/body pixels outside them and all alpha pixels remain exact base pixels

(headless check passed). Exported GIF has seven timed frames and repeats; smoothness

was shown for owner review. PNG/GIF exports are in `build/`.

OWNER PASS 2026-11-11: "pass great job!" approves this corrected size, blink and

head-down sleep review. Exact immutable sources, scales/supports, renderer,

previews and check report are preserved in `draft-assets/approved-rest-v1/`;

`approval.json` records hashes. This supersedes the earlier pending review status

for that exact review, not the earlier rejection of head-up sleep.



Current action review, 2026-11-11: `test-exes/Judge-ActionReview-v1.exe` and

`build/actions-review-v1.gif` show tilt (mid/peak), cheesy smile (mid/peak) and nose

lick (start/peak/retract). Seven new built-in imagegen keys are saved with prompts

and hashes under `draft-assets/sit-{tilt,smile,lick}-v1/`. Registration, timing and

scope are recorded in `planning/action-review-v1.json`. The renderer uses the

approved sit below row 560 exactly, with a feathered neck join for head/ear actions;

licking changes only eyelid/muzzle regions and keeps the base alpha. Uniform head

scale corrections use the approved eye span; expression-related pitch and eye

narrowing still need visual review. Headless checks passed for body stability,

lick region/alpha protection and return-to-neutral sequences; the exported GIF's

eight frame delays and loop were checked. These actions are candidates, not

owner-approved. Approved rest sources/renderer/exe remain unchanged.



OWNER PASS 2026-11-11: "perfect" approves all three seated action animations in

that exact review. The immutable `draft-assets/approved-actions-v1/` snapshot

preserves sources, renderer, registration/timing, previews, executable and checks

with hashes. This supersedes the pending status for the seated action review.



## Portable development build 0.1.0



Current build is **0.1.2**, not a completed project. Its sit/lie motion is exported

from actual runtime placement/frames at 120 ms per draft key, in both directions:

`build/sit-lie-review-v1.gif`. The review window has Loop sit / lie and double

buffering. Transition approval and visible desktop interaction are still pending.



Native hidden-window checks caught a real stale-coordinate hit-test bug: after

native upload to (100,100), Form.Bounds could still be (0,0,300,300), causing an

opaque pixel to pass clicks through. Canonical shared LayeredSpriteWindow now

stores uploaded origin/PresentedBounds; hit testing and taskbar bounds use these.

204 hidden uploads, 3 moved hit-target positions including negative coordinates,

16 menu checks, focus/style and stable graphics handle tests pass. Both runtime

and hidden-native tests also pass in a standalone repo-only copy. No visible

window/tray or user-setting write was used. `planning/current-build.json` has the

handoff/digest, and `planning/native-hit-test-fix.json` records the bug evidence.

Reusable GIF export was extracted to shared tooling and explicitly synced into

this repo; the extracted exporter reproduces the previous eight-frame action

preview's pixels, delays and looping exactly. All approved art stays unchanged.



Current update: **0.1.1**, 2026-11-11. Owner "pass" approves the corrected seated

blink preview pair with identical body/alpha outside the eyelids. Exact sources

and comparison are preserved in `draft-assets/approved-sit-blink-v1/`, with hashes

and approval scope. Seated blinking is now enabled in the ordinary awake routine

without DraftAnimations; only sit/lie transitions remain gated. Artwork pixels,

passed scales, contact and existing blink timing are unchanged. New executable:

`test-exes/TheJudge-Dev-0.1.1.exe`; old 0.1.0 is preserved. The two builds use one

development identity, so the owner quits the old app before launching the update.

The following 0.1.0 notes remain historical provenance; current handoff is

`planning/current-build.json`.



Blink outline correction, 2026-11-11: the owner's two screenshots identify the raw

generated seated half/closed frames, whose outlines differ. The runtime had already

masked their eyelids onto one fixed approved sit. Corrected review copies are in

`draft-assets/sit-blink-v2/`; `review.json` verifies the same alpha and zero changed

pixels outside eyelid regions. These replace the raw images as review deliverables,

not the original source files. The executable and approved art remain unchanged.



`test-exes/TheJudge-Dev-0.1.0.exe` is ready for owner desktop testing. It embeds

explicit exports of the passed renderers and the three approved states/actions;

ordinary builds only verify asset digests and compile this repo's app/art inputs.

The quiet routine, alpha-hit-tested transparent nonactivating window, taskbar

stacking, dragging, size, monitor controls, pause, tray and test/review menus are

implemented with separate Judge identity/settings. No startup, updater, network,

installation or publishing is included.



New seated half/closed blink drawings use eye-only masks. Three new sit/lie keys

use uniform eye-span scaling and their own measured floor supports. These are

candidates, not approved: the companion's draft switch defaults off, while the

explicit review window enables them. Awake/sleep pose changes remain direct;

there is no invented sleep-lowering action.



`tools/Verify.ps1` passed actual asset digests/alpha bounds, 540 placements across

five monitor geometries and three sizes, 96 simulated hours, sleep/pause/drag/menu

gates, eight z-order cases and fixed-body/alpha checks. A standalone copy containing

only app/, art/ and build.ps1 also built and passed. Native window/tray/menu/drag,

mixed-DPI behavior and new candidate animation smoothness still require owner

desktop review; no visible UI was opened automatically. `planning/current-build.json`

is the build handoff and digest record.



Canonical shared additions are LayeredSpriteWindow (Odin's upload/nonactivation

path with alpha hit testing) and TaskbarOrder (the vetted z-order policy with no

pet anatomy/contact requirements). They were explicitly synced into The Judge;

Odin's existing files/snapshot are unchanged. Details: planning/shared-framework.json.



## Recommendation



Build three logical states: **Sit, Lie Down, Sleep**. Sleep has its own head-down pose.

Use a seated head tilt and the owner's requested ears-back cheesy smile as signature

actions, with blinks and an occasional nose lick. Sleep must lower and rest his

head on his paws or the support surface. Keep the first

version on the taskbar/desktop floor, with dragging, sizing, monitor controls and

Odin's non-focus-stealing desktop behavior.



Start with approximately 18 unique sprite drawings, rather than replicating

Odin's numerous gaze and movement families. Two optional seated glances bring the

target to 20. These are planning budgets, not finished assets or a guarantee that

the first drawings will animate cleanly.



Running and window perches are a separate optional expansion. Their mechanics are

tested in Odin, but creating convincing dog movement is a substantial art task.

The first version therefore retains the companion interaction while reducing

autonomous movement. If movement is essential, choose that expansion before art

production; a complete Odin reskin cannot be achieved through three pose changes.



## What the reference archive supports



27 photos and 14 MOV clips were extracted without changing source bytes. All photos

were visually reviewed in contact sheets; each clip was sampled at five positions

across its duration. Clips are about 2.0-3.0 seconds and appear to be paired photo

motion clips. This was a sampled visual assessment, not a full playback or audio

review. No usable complete running/jumping cycle was established by that review.



The strongest coverage is seated front/three-quarter poses, expressive ears, head

tilts, small head turns, open/closed mouth expressions, and the fluffy tail. Lying

and sleepy coverage exists but is much thinner. Lighting, apparent age and coat

length vary; use a consistent adult likeness before extending it to other poses.

P11/P14 and possibly P12 show a younger-looking appearance and should not silently

determine adult proportions.



### Proposed states



| State | Best references | Implementation and limitation |

|---|---|---|

| Sit, default | P01/P05/P13/P23; P08/P22 for side and tail | Front or mild three-quarter seated body. Pick one view; other seated views remain references, not additional states. |

| Lie Down, awake | P26 / IMG_5140; P16 / IMG_9715; P18 / IMG_4165 | P26 gives the clearest long body; its warm night lighting needs reconciliation with daylight coat references. P16 is closer to the face; P18 is low/downward facing. |

| Sleep | P06 / IMG_6471 and P18 / IMG_4165 for head-down support; approved sit for identity | Separate head-down sleeping pose, chin/cheek resting on forepaws or support surface. Owner rejected head-up closed-eye lay as sleep. P27 remains an optional later curled body. |



### Proposed actions



| Action | Reference | Reuse and effort | First version |

|---|---|---|---|

| Blink | P15 closed eyes; brief eye closures in IMG_1582.MOV | Reuse scheduling/suppression concepts in RestEffects.cs. New eye artwork for this face. Low effort. | Yes, awake states |

| Head tilt and return | P12 / IMG_8409.MOV; P16 / IMG_9715.MOV | Clear observed personality. New short clip using the existing animation playback approach; tilt anatomy and registration need review. | Yes, seated only |

| Cheesy ears-back smile and return | P19 / IMG_3995.JPEG and IMG_3995/3996/3997.MOV for ears-back changes; P15 as a secondary smile/eye reference | Owner-requested action. Briefly ease the ears back into the photographed smile-like expression, hold, then return to neutral. Reuse clip playback and expression arbitration; new dog artwork, no new locomotion. Budget two keys: intermediate and peak, reused on return. Preserve the seated body, scale and support; follow his actual muzzle and eyes without inventing a human grin. | Yes, seated only; occasional, mutually exclusive with tilt, lick and glances |

| Nose lick and return | P25, 123_1-2.jpeg | Strong still reference. Existing clip playback can be reused, but Odin has no finished dog nose-lick action. Medium effort. | Yes, seated only; first action to cut if art stalls |

| Look left/right and return | IMG_3995/3996/3997.MOV; IMG_0226.MOV; P24 profile | Reuse attention arbitration and cooldown concepts. Odin's clock gaze assumes many frames and specific mappings; two glances require adaptation. | Optional, seated only |

| Short tail wag | P03/P08/P21/P22 show tail shape; IMG_5137/5252 offer limited motion evidence | Odin's occasional tail-flick scheduler is useful; cat flick frames are not a dog wag. New attachment masks, motion and edge checks needed. | Defer |

| Pant/happy mouth | P04/P09/P10/P15; IMG_3920.MOV | Well-supported expression, but adds another face variant and action scheduling. | Defer; neutral closed mouth by default |

| Small bark gesture | P07/P19/P20 and IMG_3995/3996/3997.MOV mouth/ear changes | Possible visual reference; sampled frames do not establish audio or intent. New clip, not Odin grooming renamed. | Defer; no sound planned |

| Run/zoomies | Standing body P02/P09/P21, no established gait cycle | Odin run timing, movement and edge handling are useful. Dog gait artwork must be new. High effort. | Optional expansion |

| Jump/window perch | No established complete takeoff/flight/landing sequence | Odin perch selection, moving/closed-window handling, arc and landing clamps are useful. New dog jump poses/contact data. High effort. | Optional expansion |

| Groom/stretch/roll/play bow | P06/P18/P27 provide partial pose ideas only | Existing clip playback is reusable; these references do not establish full actions. | Defer |



No cat loaf, scratching, breathing effect, walking cycle, rear-facing state, full

clock gaze grid, separate puppy skin, toys or bespoke behavior system in the proposed

first version. This is a scope choice for The Judge, not an inherited Odin rule.



## Odin reuse and evidence



Reviewed local Odin source at commit

`f1999e54567a8f9e1d155dba9eadb9d3f73bd3f3`, its current approval records and existing

check reports. No Odin files were changed and no fresh build or desktop test was

run for this planning task. Reports establish Odin behavior, not automatic approval

or correctness for The Judge.



The latest 3.2.0 candidate records owner passes for running/jumps (G2), the corrected

floor/gaze placement ("Looks right"), and DEV/release layouts ("Both look right").

The release record still contains mixed historical status fields; this plan does

not claim a GitHub release was published. Use the promoted 3.2.0 path, not older

rejected head layers, clock experiments or the superseded floor blend.



| Component | What can be reused | Evidence and adaptation |

|---|---|---|

| Desktop shell | Transparent nonactivating window, drag, tray/context menu, size/monitor handling | app/Desktop.cs and Program.cs; production-test-menus.json and production-test-screen-edges.json pass. Shell still contains Odin identity and rendering dependencies; isolate them. Dragging also needs manual checking in the dog build. |

| Ground placement and edges | Support-point placement, per-family visible margins, taskbar stacking | shared/framework/SpriteSupport.cs already canonical; app/ScreenEdges.cs, TaskbarOrder.cs. Existing support/edge reports pass; production self-test has 8 taskbar-order cases. Judge needs its own measured contacts and sizes. Never copy Odin's support rows or body widths. |

| Blink timing and quiet expression arbitration | Nonlooping blink schedule, suppress during competing expressions | RestEffects.cs / Desktop.cs; production self-test restEffects and eyelids pass. Reuse scheduling; author dog eye assets and verify the changed regions. |

| Attention | Delayed cursor/window attention, hover priority, no pointer attention in sleep | Behavior.cs / SittingBehavior.cs; local-test-follow-mouse.json passes and production self-test covers priority and sleep. Full ClockGaze/SittingGaze frame tables are Odin-specific. Reduce directions deliberately. |

| Quiet state routine | Rest holds, cooldowns, pause, action sequencing | Behavior.cs; production self-test behavior simulates 96 hours and reports 97.11% resting. Its separate production-test-behavior report uses a different asset path/results. Neither is a Judge behavior target. Keep the approach; simplify its state/action selection. |

| Running | Travel, boundary handling, frame stepping, registration concepts | production-test-run-motion.json passes; owner G2 pass recorded. New gait, registration, speed and loop seam still need dog review. |

| Jump/perch | Window eligibility, ascent/descent, safe landing, disappearing/moving windows | production-test-jump-tilt.json and local-test-perch-zoomies.json pass; production behavior reports cover window changes. Judge-specific sprite margins and contact metadata required. |

| Settings/startup/updater | Persistence and update verification approach | Existing menu/updater reports pass. Assign separate product paths, keys, mutex and identity. Keep updater unconfigured/off until this repo has an explicitly chosen remote; never retain Odin's update target. |



Important implementation trap: `ConfigureArt` and `Supports` do not make the entire

Odin routine automatically safe for a smaller asset set. `Routine()` explicitly

chooses loaf, groom and stretch, and waking invokes stretch. Replace/filter that

selection for the three-state plan and test sleep/wake and unavailable actions.

Do not simply remove artwork and assume every branch will adapt.



Only SpriteSupport is currently an extracted shared component. Desktop, behavior,

attention and animation code remain coupled to Odin. Extract the smallest pieces

needed as work reaches them; avoid a large framework rewrite. Canonical shared

source goes in shared/framework, tools in shared/tools; register The Judge in the

explicit sync tool once it has a consuming application. Each pet tracks snapshots

and builds independently. No build-time sync or absolute parent dependencies.



## Small artwork budget



| Group | Unique drawings |

|---|---:|

| Seated, lying awake, lying asleep | 3 |

| Seated half/closed blink; lying two intermediate stages and closed endpoint | 5 |

| Seated tilt intermediate and peak, reused on return | 2 |

| Seated ears-back cheesy smile intermediate and peak, reused on return | 2 |

| Seated nose lick keys, returning to seated base | 3 |

| Sit-to-lie transition keys, reverse only if visually convincing | 3 |

| Initial total | 18 |

| Optional seated left/right glance keys | +2 |



Frame entries can repeat these drawings. They are not 18 independent animation

families. This is deliberately below Odin's 19 seated, 12 loaf and 14 laying gaze

frames plus separate action families. If the transition reverses poorly or tilts

need more in-betweens, either add them explicitly or reduce scope. Avoid smoothing

by crossfading faces or scaling the entire body to imply breathing.



Draft routine: mostly hold Sit or Lie Down, occasionally Sleep; blink while awake;

choose one brief seated expression (tilt, cheesy smile or lick) at a time, then

return to the same base. The smile eases in, briefly holds and releases, rather

than becoming a fourth state or a continuous loop. Sleep

ignores attention and wakes to Lie Down, followed by Sit only when needed. Initial

state weights and cooldowns are tuning choices, not copied owner approvals. All

size/contact measurements belong to The Judge.



## Delivery sequence



1. Confirm one consistent adult likeness using P01/P05/P13/P23, with P08/P21/P22

   clarifying tail and body. First concrete review is the seated sprite, then the

   lying/sleep pair on a common floor at actual desktop size. Resolve collar/tag

   appearance consistently from the photos before drawing all frames.

2. Build a portable development companion with the three poses, independent app

   identity, drag, tray/menu, sizes and monitor placement. Keep startup opt-in for

   initial development and updates inactive. Finished assets and build inputs must

   live in the pet repo. This plan does not authorize publishing or installations.

3. Integrate blink, tilt, cheesy ears-back smile, then lick in that order. Add the short sit/lie transition;

   keep head-up closed-eye lying as the blink endpoint and use separate head-down

   sleep art. Show two intermediate blink stages in motion. Add glances only after the smaller

   action set looks coherent. Keep original references and reviewable candidates.

4. Reuse/adapt relevant headless checks: assets present; only allowed actions over

   long simulated routines; sleep/wake paths; pause; one expression at a time;

   stable scale/support across expressions; transparent margins at all sizes and

   both monitor edges; no writes to Odin settings/startup/update identity. Follow

   with owner visual review of posture, likeness and transitions on a test exe.

5. Stop at the small scope if it feels complete. If autonomous travel is wanted,

   add both run directions first and check gait/loop/boundaries, then jump/perch

   art and moving/closed-window landings. Reuse Odin's mechanics with new dog data.



## Photo index



IDs match the local contact sheets and inventory. Original filenames are preserved.



| ID | Filename | Primary use |

|---|---|---|

| P01 | 123_1.jpeg | Front seated likeness and chest |

| P02 | IMG_2454.JPEG | Standing front/three-quarter proportions |

| P03 | IMG_5137.JPEG | Side seated silhouette and tail; night lighting |

| P04 | IMG_3920.JPEG | Happy standing face; paired motion clip |

| P05 | 123_1.JPG | Seated front/three-quarter face and ears |

| P06 | IMG_6471.JPEG | Side resting/sleeping reference, small subject |

| P07 | IMG_3997.JPEG | Seated mouth/ear expression |

| P08 | IMG_2810.JPEG | Seated body, profile and tail |

| P09 | IMG_3010.JPEG | Standing alert/happy |

| P10 | IMG_3612.JPEG | Happy face and chest |

| P11 | IMG_7412.JPG | Younger-looking seated proportions; secondary |

| P12 | IMG_8409.JPEG | Head tilt; paired clip is especially useful |

| P13 | 123_1-1.JPG | Adult seated three-quarter shape and coat |

| P14 | IMG_7418.JPG | Younger-looking side sit; secondary |

| P15 | 80066AC9-2F02-4375-847A-225368DFB422.jpg | Closed eyes, happy mouth and ears |

| P16 | IMG_9715.JPEG | Front lying face and paws; slight tilt in clip |

| P17 | IMG_8110.JPG | Front seated on chair |

| P18 | IMG_4165.JPEG | Low lying/down pose from above |

| P19 | IMG_3995.JPEG | Seated lifted muzzle and ears-back expression; cheesy smile reference and paired turns |

| P20 | IMG_3996.JPEG | Seated mouth/ear changes; paired clips support cheesy smile motion |

| P21 | IMG_5252.JPEG | Standing side/back body and raised fluffy tail |

| P22 | IMG_0226.JPEG | Seated side, tail, head turns in clip |

| P23 | 123_1-1.jpeg | Adult frontal face, chest and sit |

| P24 | IMG_0760.JPEG | Muzzle/ear profile, window-looking clip |

| P25 | 123_1-2.jpeg | Nose lick and lowered eyelids |

| P26 | IMG_5140.JPEG | Long reclining body; warm lighting |

| P27 | IMG_1582.JPEG | Curled rest, awake and blinking; optional later body |



## Current owner changes — 2026-11-11, build 0.1.3



The owner expanded all four expressions to both awake poses, each returning to

its own base. This supersedes the seated-only expression draft above. Tilt and

smile hold a peak sampled once per invocation between 2 and 8 seconds, replacing

the brief/fixed-two-second hold. Nose lick is 0.60 seconds. New lying drawings are

candidates enabled in the development build; passed sitting/rest/blink snapshots

remain immutable. Menu/input changes and tests are recorded in current-build.json.

Visible desktop input, lying art and sit/lie motion still await owner review.



## Lying/sleep stages — 2026-11-11, build 0.1.4



Owner requested intermediate stages between lying awake and sleep. Four new built-in

imagegen keys lower the head progressively, then play backward for waking. Existing

approved endpoint PNGs and placements remain unchanged. This supersedes the direct

awake/sleep draft scope. Sleep motion preview is enabled in this development build;

its new artwork awaits review. See lay-sleep-v1.json and current-build.json for

registration, prompts, timings and verification. The review window includes Loop

lie / sleep; the GIF uses the same runtime frames/placement and timings.



## Ear definition correction, build 0.1.5



Owner rejected the first sleep-transition drawings for fuzzy edges and lost ear

definition. Four local imagegen repairs use the approved awake/sleep detail as

reference. Bounded ear compositing preserves the existing face, pose, support,

scale, timing and pixels outside the ear regions. The v1 drawings/executable are

retained as provenance. Inspect the new close-ups and motion before approval.


## Body registration and restored sit/lie flow - build 0.1.6

Owner reported sliding/growth during sleep motion and missing sit/lie transitions.
The latter was an off-by-default gate: restored to the normal flow with both
sit-to-lie and lie-to-sit checks. Sleep stages now use a rear-paw horizontal anchor,
with body scale corrected relative to awake lying. No PNG artwork changed. This
explicit feedback supersedes previous display registration and default-gate choices.
Full cycle and registration evidence are linked in current-build.json; visible
body deformation/scale still requires owner review.

## Pose cycle passed - final smoke test

Owner passed the corrected 0.1.6 full cycle. All planned small-scope feature and
artwork implementation is finished. Approval snapshot: approved-pose-cycle-v1.
Only the final live EXE smoke test is unconfirmed; no new feature/artwork work
is required. Do not add deferred travel/gaze/startup/updater work to this scope.

## Production menu and lifecycle controls - build 0.1.7

Owner added production behavior: only Sit/Lay Down manual poses, autonomous sleep,
and Odin-style size/windows/screen/close controls including startup and updates.
This supersedes the earlier no-startup/updater feature scope. A separate developer
build preserves the existing testing menu. The owner named TheJudge as the GitHub
repo slug; production targets BurtFablehoff/TheJudge/TheJudge.exe releases.
Both local builds and hidden healthy-update/rollback fixtures pass. Source/art
publishing is not performed; repository setup is pending GitHub authentication.
See production-menu-v1.json and current-build.json for the reviewable result.
