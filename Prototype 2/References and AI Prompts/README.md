# GenAI & Machine Translation (MT) Usage Acknowledgement – Prototype 2

## Tools Disclosed
- **Generative AI:** ChatGPT (GPT-5)  
- **Machine Translation:** *Not used* in Prototype 2  

## How GenAI Was Used (Appropriate Use)
- **Code commenting**  
  - AI was used to add detailed explanations and inline comments to Unity scripts, improving readability, maintainability, and helping peers understand the code structure during testing.  
- **Debugging & troubleshooting**  
  - Assisted in diagnosing why **physical buttons** occasionally failed to press or why the **pusher detached from the base**, which disrupted immersion.  
  - Supported investigation of the **Skybox turning black in VR** issue, which did not occur in the Scene view. AI suggested checking XR rendering pipeline configurations, camera “Target Eye” settings, and shader stereo compatibility.  
- **Reflection & planning**  
  - Provided input on potential usability improvements such as **refining button feedback** (e.g., haptic/audio cues) and clarifying **wrist menu placement and role**.  
  - Helped structure ideas for iteration by comparing design trade-offs and pointing out missing onboarding elements.  

## Boundaries Observed
- All AI suggestions were verified in Unity builds and only **functional, correct solutions were retained**.  
- AI was **not** used to auto-generate complete features or systems; instead, it provided **targeted guidance, reasoning, and annotations**.  
- Suggestions that were **inaccurate, outdated, or irrelevant** (e.g., references to Unity versions with different XR settings) were discarded or manually corrected.  
- The responsibility for final implementation, testing, and design decisions remained entirely with me.  

## Specific, Concrete Uses in Prototype 2
- **Physical buttons**  
  - Analysed instability where the **pusher/base separated** during presses.  
  - Guided adjustments to colliders, rigidbodies, and joint constraints to improve reliability.  
- **Skybox rendering issue**  
  - Diagnosed the discrepancy where the **Skybox appeared in Scene view but rendered black in VR**.  
  - Suggested verifying **camera Target Eye settings**, testing **multi-pass vs single-pass rendering**, and checking **shader graph XR compatibility**.  
- **Code annotations**  
  - AI generated inline explanations for key scripts such as **eraser logic** and **remote grab interactions**, clarifying variables like radius, softness, transparency, and interaction states.  
  - These annotations improved traceability and facilitated clearer communication in team reviews.  

## External References (non-AI sources)
- **YouTube tutorials and walkthroughs** were consulted alongside AI support to validate solutions and learn XR-specific patterns.  

| Source | Link | Title |
|--------|------|---------|
| YouTube Tutorial 1 | *https://youtu.be/HFNzVMi5MSQ?si=MbCKOG9TvkWDLf8-* | A Beginner's Guide to Making VR Buttons |
| YouTube Tutorial 2 | *https://youtu.be/YISa0PvQTGk?si=5dHOrNkDGtmQj5IG* | Unity VR Game Basics - PART 11 - Wrist Menu |
| YouTube Tutorial 3 | *https://youtu.be/ojZkl8q3YBI?si=f3KER8aIQEx3Vajo* | Unity VR Game Basics - PART 1 - Setup in 10 Minutes |

## Minimal Prompt/Response Excerpts (Condensed)
> **Prompt:** Why does my button sometimes detach from its base when pressed in VR?  
> **AI:** Likely collider/rigidbody misalignment; check joint anchors, adjust rigidbody constraints, and use continuous collision detection.  

> **Prompt:** My Skybox looks fine in the Scene view but turns black in VR. Why?  
> **AI:** Possible XR rendering pipeline mismatch; verify camera “Target Eye” setting, test multi-pass vs single-pass rendering, and confirm shader stereo support.  

> **Prompt:** Please add explanatory comments to my eraser script.  
> **AI:** (Added inline explanations clarifying radius, softness, erase-to-color logic, and transparency behaviour).  

## Accountability, Integrity & Learning
- Strictly followed the **GenAI/MT Usage Framework** to ensure transparency.  
- AI assistance was **limited to debugging, annotations, and targeted fixes**; final implementation was always tested, validated, and decided by me.  
- This prototype highlighted that GenAI is most effective as a **reasoning and documentation partner**, not as a replacement for practical testing.  
- By critically engaging with AI outputs, I improved my own understanding of Unity XR systems (e.g., physics joints, rendering pipelines).  
- Future iterations will continue to use AI for **debugging hints, annotation, and reflection**, but always alongside official documentation, tutorials, and direct testing in VR.  


