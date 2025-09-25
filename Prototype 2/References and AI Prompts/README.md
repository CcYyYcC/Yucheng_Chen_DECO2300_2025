# GenAI & Machine Translation (MT) Usage Acknowledgement – Prototype 2

## Tools Disclosed
- **Generative AI:** ChatGPT (GPT-5)  
- **Machine Translation:** *Not used* in Prototype 2  

## How GenAI Was Used (Appropriate Use)
- **Code commenting**  
  - AI was used to add detailed explanations and inline comments to Unity scripts, improving readability and maintainability.  
- **Debugging & troubleshooting**  
  - Helped analyse why **physical buttons** sometimes failed to press or the **pusher detached from the base**.  
  - Supported identifying causes of the **Skybox turning black in VR** despite displaying correctly in the Scene view. Suggestions included XR rendering pipeline checks, camera settings, and shader compatibility.  
- **Reflection & planning**  
  - Requested AI input on improving usability (e.g., refining button feedback, clarifying menu setup).  

## Boundaries Observed
- AI suggestions were always tested directly in Unity; only verified and functional changes were kept.  
- No full systems or large code blocks were copied directly; AI was used for **targeted guidance and analysis**.  
- Inaccurate or irrelevant suggestions were discarded or corrected manually.  

## Specific, Concrete Uses in Prototype 2
- **Physical buttons**  
  - Analysed misalignment issues where pressing caused **pusher/base separation**.  
  - Adjusted colliders, rigidbodies, and joint constraints.  
- **Skybox rendering issue**  
  - Diagnosed why the Skybox was black in VR but fine in Scene view.  
  - Suggested checking **Target Eye**, stereo rendering modes (single-pass vs multi-pass), and shader compatibility.  
- **Code annotations**  
  - AI generated inline comments for scripts such as **eraser logic** and **grab interactions**, improving documentation.  

## External References (non-AI sources)
- **YouTube tutorials and walkthroughs** were also consulted for XR interaction and debugging.  

| Source | Link | Purpose |
|--------|------|---------|
| YouTube Tutorial 1 | *(Add link here)* | Real button  |
| YouTube Tutorial 2 | *(Add link here)* | wrist menu in VR |
| YouTube Tutorial 3 | *(Add link here)* |  |

## Minimal Prompt/Response Excerpts (Condensed)
> **Prompt:** Why does my button sometimes detach from its base when pressed in VR?  
> **AI:** Likely collider/rigidbody misalignment; check joint anchors and use continuous collision detection.  

> **Prompt:** My Skybox looks fine in the Scene view but turns black in VR. Why?  
> **AI:** Possible XR rendering pipeline mismatch; verify camera “Target Eye”, test multi-pass vs single-pass rendering, and confirm shader stereo support.  

> **Prompt:** Please add explanatory comments to my eraser script.  
> **AI:** (Added inline explanations for parameters such as radius, softness, and erase-to-color logic).  

## Accountability, Integrity & Learning
- Followed the **GenAI/MT Usage Framework**.  
- AI support was limited to **debugging, annotation, and targeted fixes**.  
- Final implementation, testing, and iteration decisions remained author-controlled.  

**Prepared by:** Yucheng Chen  
**Project/Prototype:** Procreate VR — Prototype 2  
**Date:** 25/09/2025  
