# Procreate XR Prototype

## Overview
This is an XR project developed in Unity, designed as an XR version of Procreate.  
The project is currently at **Prototype 1** stage.

## How to Run
1. Open the project in **Unity 2022 or later**.  
2. Load `PrototypeScene1` and press **Play**.

---

## GenAI & Machine Translation (MT) Usage Acknowledgement

**Tools Disclosed**
- **Generative AI:** ChatGPT (GPT-5 Thinking)  
- **Machine Translation:** *Not used* in Prototype 1. If MT is used later (e.g., UI localisation), it will follow the course GenAI/MT Usage Framework and be disclosed.

### How GenAI Was Used (Appropriate Use)
- **Code ideation & alternatives:** Searched Google for similar patterns/snippets and, with AI assistance, **adapted and wrote my own implementation** (no direct copy–paste).  
- **Debugging & troubleshooting:** Consulted AI to reason about Unity errors, render-pipeline mismatches, and UI behaviours; **verified everything in-engine**.  
- **Documentation & explanations:** Used AI to **draft comments and usage notes**, then edited them to match my final code.  
- **Accessibility & UX tips:** Requested guidance on **readable UI** (rounded corners, borders, shadows), **World-Space text clarity**, and good contrast/scale/legibility.

**Boundaries Observed**
- No unreviewed, wholesale use of AI output.  
- Understood each change and **re-implemented** logic where necessary.  
- Tested behaviour inside Unity; inaccurate suggestions were **discarded or corrected**.  
- Third-party assets are credited and used under their respective licences.

**Scope & Exclusions**
- GenAI was **not** used to auto-generate large systems; it was limited to **targeted guidance**, small examples, and explanation.  
- No MT was used for core UI copy in Prototype 1.

### Specific, Concrete Uses in Prototype 1
- **VR-style cursors (pen/eraser):**  
  - Models follow the mouse over a World-Space canvas with **tip alignment** (convert plane hit → local rect; `tipWorld = hit + gap`; `modelPos = tipWorld − TransformVector(tipLocal)`).  
  - **Tool buttons** toggle visibility (Pen/Eraser). **Other actions** (Clear/Close/Reset) hide both models to prevent conflicts.
- **Welcome window & close behaviour:**  
  - **CanvasGroup** fade-out and an **“I know”** button to close the entire board root (`SetActive(false)` after fade).  
  - Minimal API: `Show(title, body)` / `Hide()`; optional **Esc** close.
- **Rendering & pipeline settings:**  
  - Diagnosed **pink materials** as shader/pipeline mismatch (Built-in vs URP) and provided conversion/rollback paths.  
  - Configured **global Fog**: Built-in via *Lighting → Environment → Fog*; URP via *Global Volume → Fog* (with camera layer checks).
- **UI polish & stability:**  
  - Implemented **rounded corners + border + shadow** without textures (parent-as-border or custom UI effect).  
  - Prevented **World-Space text distortion** by enforcing **uniform scales** (x=y=z) and using TMP **Distance Field** materials.
- **Controls & wiring:**  
  - Bound **R** to reset board + UI to initial state.  
  - Button events switch tools and show/hide respective models.

### External Code References (non-AI sources)
*(Consulted for ideas/patterns; all code in this project was adapted and rewritten to fit requirements.)*
- https://blog.csdn.net/weixin_48388330/article/details/138917103  
- https://blog.csdn.net/zhaocg00/article/details/142502240  
- https://blog.csdn.net/DllCoding/article/details/133149170

### Minimal Prompt/Response Excerpts (Condensed)
> **Prompt:** How to align a pen model’s **tip** to the canvas point under the mouse?  
> **AI:** Ray from the UI camera → plane hit → local-rect check; `tipTarget = hit + small gap`; `modelPos = tipTarget − TransformVector(tipLocal)`.

> **Prompt:** Why are materials **pink** after importing assets?  
> **AI:** Likely render-pipeline mismatch; verify Built-in vs URP; convert/revert materials; check Graphics/Quality settings.

> **Prompt:** World-Space text looks stretched.  
> **AI:** Remove non-uniform scales; use TMP UI Distance Field; adjust canvas scale/DPPU; don’t resize text via scale.

### Accountability, Integrity & Learning
- Followed the **GenAI/MT Usage Framework**; recorded when/why AI was consulted.  
- GenAI **augmented** my work; it did **not** replace understanding or effort.  
- All changes were validated in Unity scenes; guidance that didn’t fit the version/pipeline was removed.  
- Architecture and final code decisions remain author-controlled.

**Risks & Mitigations**
- **Outdated/inaccurate advice:** Validate in a throwaway scene; cross-check with Unity docs.  
- **Performance regressions:** Prefer simple layouts over heavy shaders; profile when adding effects.  
- **Over-reliance:** Re-implement critical parts and document rationale.

**Versioning & Traceability**
- Major AI-assisted edits are summarised in commit messages (e.g., “Pen tip alignment”, “Board fade-out close”).  
- External assets and licences are listed in **External Assets**.

**Prepared by:** Yucheng Chen 
**Project/Prototype:** Procreate VR — Prototype 1  
**Date:** 27/08/2025

---

## External Assets

- **The Island Village** — _UModeler, Inc._  
  **Source:** [Unity Asset Store](https://assetstore.unity.com/packages/3d/environments/fantasy/the-island-village-179778)  
  **License:** Standard EULA  
  **Use:** Low-poly stylised island environment (buildings, rocks, foliage, etc.).

- **Animals FREE — Low-Poly 3D Models Pack** — _ithappy_  
  **Source:** [Fab (Epic’s marketplace)](https://www.fab.com/listings/0c8f3917-2461-4775-a853-b995bb93bac5)  
  **License:** Standard License  
  **Use:** Low-poly animals used as player model.

- **Water Shaders V2.x** — _Nicholas Veselov_  
  **Source:** [Unity Asset Store](https://assetstore.unity.com/packages/vfx/shaders/water-shaders-v2-x-149916)  
  **License:** Standard EULA  
  **Use:** Water (specular/mirror).
