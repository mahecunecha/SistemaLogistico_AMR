<!-- Funplay Unity MCP managed project skills -->
<!-- Funplay Unity MCP skill version: unity-ui-composition@1.0.12 -->

# Design Image To Editable Page Prefab

Read when a supplied image defines one or more requested UI pages/states. This is a composition and acceptance workflow, not permission to rebuild an existing prefab or change project settings.

## Establish the comparison contract

- Retain the original references at their pixel dimensions. Identify the actual game viewport, page/tab, locale, counters, scroll position and selected/disabled states. A device frame or cropped fragment is not a full viewport. Ask focused questions when references conflict or a hidden/unclear detail materially changes implementation; do not silently simplify it.
- Follow explicit user/design dimensions, otherwise preserve an established project's CanvasScaler baseline. For a new portrait project without a baseline use the intended full design viewport; only when no design exists use 1080x2340. Do not impose 720x1560 or rescale the existing project to fit a screenshot. Reuse the project's safe-area policy without proactively adding scripts, containers or insets.
- Map each significant region to its reference-pixel bounds, asset source, prefab/node path, stacking order, text/font/material, state and fixed/flexible edges. Distinguish visible artwork from its layout rectangle and clickable area. Preserve an existing hierarchy and its bindings; fix visual discrepancies at their source.

## Reuse assets and save real UI

- Prefer layered exports and existing project Sprites, fonts and approved material presets. Inspect Sprite rect, padding, alpha edges and borders before compensating with RectTransform scale. Use Sliced only with verified suitable non-zero borders; use the importer-safety reference before editing a shared Sprite.
- Do not put a flattened page image behind the UI to masquerade as editable content or use it as the acceptance capture. Separate background art, panel/button surfaces, icons, captions, dynamic values, badges, progress track/fill and state overlays. Use the project's native text convention (TMP by default for a new project), including required TMP-native text effects. Decorative graphical lettering may remain art; live/localized text must remain editable.
- Author a complete saved page prefab, with reusable navigation and repeated row prefabs where appropriate. In a navigation bar keep icon, caption, selected/normal backgrounds and dividers separate; their positions and hit areas must follow the slot, not a baked full-bar texture. Preserve icon proportions and intentional margins.
- For lists use ScrollRect -> clipped Viewport -> Content, with independent row art/text/bindings. Let the viewport occupy remaining space; assign one layout owner per driven axis. Use anchors for pinned controls and aspect-preserving cover/fit for flexible artwork, rather than uniformly stretching the whole page. Wire stable serialized references in the prefab; runtime code binds data/listeners or instantiates prepared rows, not reconstructs a stable page.
- Save using Unity APIs and verify exact asset/component references after reopening or fresh instantiation. Do not replace user-authored scenes or edit Unity YAML. Use an owned preview session only when needed and restore its state; a fixture overlay is not the actual authored page.

## Validate the rendered result, not just the boxes

1. Capture the saved page in its actual Unity Game View/camera under the comparison state and viewport dimensions. Check landmarks, crop and scaling: equal PNG dimensions alone do not prove equal content framing. Retain project/session and available capture geometry; do not mix screen, render, Canvas and reference coordinates.
2. Compare at one common scale, whole-page first and then every mapped region. Check icon silhouette/trim, spacing, glyph baselines, wrapping, outlines/shadows, alpha edges, corners, dividers and clipping. Same-scale side-by-side or overlay/difference views may help; do not independently shift/stretch regions to hide errors. Use available analysis tools without assuming image generation or a decoder is installed.
3. Read RectTransform/layout/reference state separately from visible pixels. A matching node box may contain the wrong Sprite, font, text or state. A large matching background must not outweigh a small missing control: if using metrics, choose project-appropriate tolerances and evaluate required regions independently. Never invent scores or declare visual verification from metadata alone.
4. Correct the source discrepancy and recapture under the same conditions. Keep the references, fresh captures, region results, approved departures and remaining mismatches. Missing artwork, placeholders and unknown states must be disclosed; do not claim fidelity while unapproved differences remain.
5. Separately test requested aspect ratios, locales and actual interactions: navigation states, changing values, list scrolling/masks and reopen behavior. Pointer delivery is not business acceptance; use current MCP tools and inspect the resulting state. Do not claim untested adaptation or device behavior.

Image generation is optional for genuinely missing/occluded art, not a substitute for composition. If an appropriate client tool or art pipeline is available and in scope, request only the missing asset with explicit dimensions/style/transparency, inspect it at its intended size and treat it as approximate until reviewed. Otherwise request clean source/layered assets when fidelity is blocked; do not fabricate hidden design details or install tools silently.

## Sources and adaptation

Adapted to Unity from the original Funplay Cocos v0.7.3 image-to-prefab workflow, preserving this project's prefab, baseline, TMP, safe-area and MCP-first policies:

- [Pinned Funplay Cocos reference](https://github.com/FunplayAI/funplay-cocos-mcp/blob/8062c0f9e2c7/lib/project-skill-references.js)
- [Unity prefab authoring](https://docs.unity3d.com/2022.3/Documentation/Manual/CreatingPrefabs.html)
- [Unity UI routing](https://github.com/Unity-Technologies/unity-agent-plugin/tree/cf6b2da24e424b0a60d560a57f39f676cb6f79f3/skills/ui)
