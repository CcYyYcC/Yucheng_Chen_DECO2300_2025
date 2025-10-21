# GenAI & Machine Translation (MT) Usage Acknowledgement – Prototype 3

## Tools Disclosed
- **Generative AI:** ChatGPT (GPT-5)  
- **Machine Translation:** *Not used* in Prototype 3  

## How GenAI Was Used (Appropriate Use)
- **Code debugging and logic design**  
  - AI was used to refine **drawer and shutter door interactions**, particularly for adjusting **joint limits** and **collider alignment** to ensure smooth motion and prevent clipping.  
  - Helped troubleshoot the **lever scene-switching mechanism**, clarifying how to detect **rotation angles**, apply **timed triggers**, and manage **scene transitions**.  
  - Assisted in implementing the **camera height adjustment system**, providing reasoning for handling **Y-axis input** and stabilizing XR Rig movement while avoiding unwanted drift.  

- **Reflection and iteration planning**  
  - Supported evaluation of **realism and usability** for the new environmental interactions.  
  - Suggested refinements to **physical feedback**, such as adding optional sound cues or spring-back effects for the lever and drawer.  
  - Helped organize ideas for **testing tasks**, participant questions, and documentation flow (e.g., task sequence, rating metrics, and success criteria).  

- **Code annotation and explanation**  
  - AI added inline explanations in scripts controlling cabinet physics, lever behavior, and camera movement, improving readability and peer understanding during group testing.  

## Boundaries Observed
- All AI-generated logic and code were manually verified in Unity and tested on the Meta Quest headset.  
- AI was not used to produce entire systems or automate design work; it provided **targeted guidance, syntax support, and conceptual advice**.  
- Any suggestions that caused instability, unrealistic movement, or performance issues were discarded.  
- Final design choices, implementation, and tuning were fully executed and validated by me through iterative testing.  

## Specific, Concrete Uses in Prototype 3
- **Drawer and Shutter Door Interactions**  
  - Diagnosed joint misalignment and inconsistent hinge behavior.  
  - AI suggested re-centering pivots, adjusting linear/angle limits, and ensuring colliders avoid overlapping at closed positions.  

- **Lever Scene Switcher**  
  - Helped define a method to track lever rotation angles using configurable joint drives.  
  - Guided implementation of event triggers when angles reached ±70°, with a 0.5-second hold delay for smoother transitions.  

- **Camera Height Adjustment System**  
  - Provided code logic examples for detecting **thumbstick press** and reading **controller Y-axis motion**.  
  - Suggested ways to prevent excessive motion or floor clipping by clamping camera height within defined limits.  

- **Testing Documentation**  
  - AI assisted in structuring the **Testing Plan** and **Participant Questionnaire**, ensuring clear goals, metrics, and user-friendly layout.  
  - Recommended including a combined task that integrates cabinet, lever, and camera interactions to evaluate realism holistically.  

## External References (non-AI sources)
In addition to AI guidance, external tutorials  were consulted to validate implementation details and ensure correct use of XR tools.  

| Source | Link | Title |
|--------|------|-------|
| YouTube Tutorial 1 | *https://www.youtube.com/watch?v=bYS35_hC6B0&t=210s* | Introduction to VR in Unity - PART 7 : DOOR, LEVER, DRAWER,... |
| YouTube Tutorial 2 | *https://www.youtube.com/watch?v=g51EGiL1_bk* | VR & Unity intro | Scenes, Switch Scenes With a Trigger |

### Unity Asset Store Resources
The following Unity Asset Store resources were explored or integrated to assist with interaction setup, materials, and environmental realism:  

| Asset Name | Link | Purpose / Usage |
|-------------|------|-----------------|
| *Interactive Physical Door Pack* | *https://assetstore.unity.com/packages/tools/physics/interactive-physical-door-pack-163249#releases* | e.g., Used for lever model base. |
| *Low Poly Kitchen Cabinets - SnapNSet Starter* | *https://assetstore.unity.com/packages/3d/props/interior/low-poly-kitchen-cabinets-snapnset-starter-183890* | e.g., Used for Cabinet. |


## Minimal Prompt/Response Excerpts (Condensed)
> **Prompt:** How can I make a lever that switches scenes only when pulled fully down or up?  
> **AI:** Track the lever’s local rotation angle using a configurable joint; trigger scene change events when reaching ±70°, and add a 0.5s delay to confirm user intent.

> **Prompt:** The drawer collides or clips when closing—how can I fix this?  
> **AI:** Ensure proper collider size and pivot alignment; use linear limit constraints on the joint and continuous collision detection on both rigidbodies.

> **Prompt:** How do I allow users to change camera height by moving the controller vertically?  
> **AI:** Detect thumbstick press, read Y-axis movement from controller transform, and adjust XR Rig’s localPosition with clamping to prevent floor penetration.

## Accountability, Integrity & Learning
- All AI outputs were critically reviewed, adapted, and tested to confirm accuracy.  
- No AI-generated code or text was used without manual validation in Unity.  
- AI served primarily as a **technical and reflective assistant**, offering design reasoning and code readability support.  
- This prototype demonstrated how GenAI can assist in **fine-tuning physical interactions** and simplifying documentation while preserving developer control.  
- By using AI responsibly, I deepened my understanding of **joint constraints, event handling, and spatial control** in XR development.  
- Future work will continue to use GenAI for debugging, annotation, and documentation improvement—never as a replacement for firsthand testing and critical design evaluation.  
