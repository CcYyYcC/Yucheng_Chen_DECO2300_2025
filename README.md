# Yucheng_Chen_DECO2300_2025

# Week 1 Activity Log – XR App Concept Development
 
## Project Title
XR Extension of Procreate

## Overview
In Week 1, I explored XR applications that support active user interactions. Based on the lecture guidelines, I selected Procreate as the app to redesign for XR. The focus is on creating a mixed reality version of Procreate using gesture, eye tracking, and voice control.

## Activities
- Reviewed Lecture 1 criteria and selected a productivity-focused app  
- Chose Procreate for its strong creative toolset and potential for XR extension  
- Developed an initial idea of drawing in real space using hand and eye interactions  
- Defined three main user tasks:  
  1. Draw and paint in space or on physical surfaces  
  2. Manage layers and tools using spatial controls  
  3. Switch between AR and VR modes for different workflows  
- Mapped core XR interactions: gesture-based control, eye tracking for selection, voice commands for quick actions

## Reflection
This week helped clarify the difference between passive VR experiences and active XR interaction. Procreate was chosen as a strong starting point for developing an immersive and productive creative tool in extended reality.


#
# Week 2 Activity Log – XR App Concept Development

## Project Title  
**XR Extension of Procreate**

## Overview  
In Week 2, I began prototyping the core interaction model of the XR version of Procreate. The concept centers around a framed canvas that users can scale and move within 3D space. Key controls are activated through intuitive hand gestures: the left hand manages global functions (e.g., export, settings), while the right hand handles creative tools (e.g., brush, eraser, layers). This week focused on building a paper-based prototype to simulate these interactions and gather early feedback.

## Activities  
- Sketched the physical layout: a floating canvas framed in space, with gesture zones on the left and right sides  
- Defined core gestures:
  - **Pinch/spread with both hands** to zoom the canvas
  - **Grab and drag** to move the canvas
  - **Raise left hand** to activate a radial menu with functions like **Add**, **Export**, and **Settings**
  - **Raise right hand** to activate a radial menu with tools like **Brush**, **Eraser**, and **Layers**
- Created cardboard and paper props representing the canvas, radial menus, and hand zones  
- Ran a Wizard-of-Oz test session, where one person pretended to be the system (triggering menu changes) while another acted as the user  
- Observed user expectations and confusion points (e.g., menu position, gesture clarity)  
- Took photos of the prototype setup and recorded feedback during the test

## Key Interactions Tested  
1. Zooming in and out of the canvas using two-handed pinch gestures  
2. Moving the canvas freely in space by grabbing and dragging  
3. Activating radial menus by raising each hand, then selecting a function with gaze or hand movement

## User Interview & Feedback

After the low-fidelity prototyping session, I conducted short interviews with two classmates who interacted with the prototype. The aim was to gather their impressions on usability, clarity of interactions, and overall user experience.

### Interview Questions
1. Did the interaction method feel intuitive or confusing?
2. Which part of the interface or interaction did you like the most?
3. Was there anything that felt difficult or unclear?
4. Do you think this XR interface is friendly to new users?

### Participant A Feedback
- **Most liked feature**: The ability to zoom and move the canvas using both hands felt natural and gave a strong sense of spatial control.
- **Confusing part**: The gesture to activate the radial menu was slightly unclear; they weren't sure when it was active without a visible signal.
- **User-friendliness**: Participant A felt that while the concept was exciting, first-time users would need visual cues (e.g., cursor, hand position indicators) to understand what's happening.

### Participant B Feedback
- **Most liked feature**: The separation of toolsets into left and right hands was appreciated, especially for focusing on drawing with the dominant hand.
- **Confusing part**: The radial menu seemed to float too freely—they suggested anchoring it more clearly to hand position or gaze.
- **User-friendliness**: Participant B thought it was promising, but suggested a quick onboarding tutorial or guided hints to help new users learn the gestures.


## Reflection  
This week helped validate the idea of spatial separation between tool controls (right hand) and global functions (left hand). Physical prototyping using sketches and enactment allowed rapid testing of the gesture flow and control layout. The feedback indicated that users expected visual confirmation when menus were activated, and clear anchoring of radial menus relative to the hand position. These insights will inform improvements in menu placement and gesture feedback for future iterations.

# Week 3 Activity Log – XR App Concept Development

## Project Title  
**XR Extension of Procreate**

## Overview  
In Week 3, the focus shifted from concept exploration to preparing and enacting the first testing phase.  
Following the course steps, I set up the repository structure on GitHub, built the initial Unity project under *Prototype 1*, and ensured version control with `.gitignore`. During class, I completed a Testing Plan document (IP1) to guide the evaluation of core XR interactions. Work also began on constructing the first Unity prototype to support these tests.

## Activities  
- **Repository Setup**  
  - Cloned the GitHub repository and added the required folder structure.  
  - Verified `.gitignore` for Unity files and made the first commit.  

- **Testing Plan Development (in class)**  
  - Defined testing objectives: gesture controls for canvas, wrist menu usability, and layer management clarity.  
  - Outlined testing methodologies: *Think-Aloud Protocol*, *Task-Based Testing*, and optional *A/B Comparison*.  
  - Specified prototype requirements: zoom/move canvas, left and right wrist menus, floating layer panels, and clear visual feedback.  
  - Finalized data collection methods (task completion time, success rate, qualitative feedback).  

- **Unity Prototype Setup**  
  - Created a core scene for testing spatial elements.  
  - Planned interactive objects (canvas, menus, layer cards).  
  - Began implementing gesture-based zoom, move, and menu activation.  

## Testing Plan (Summary)  
- **Objectives**:  
  - Evaluate ease and accuracy of gesture controls for the canvas.  
  - Test wrist menu usability for quick tool switching.  
  - Check whether spatial layer management improves workflow.  

- **Methodology**:  
  - Think-Aloud during interaction.  
  - Task-based tests (zoom, add/rearrange layers, switch tools).  
  - Optional A/B comparison of menu layouts.  

- **Success Criteria**:  
  - 80% of users complete all tasks successfully on the first try.  
  - Average satisfaction ≥ 4/5.  
  - Positive user feedback on gesture/menu design.  

## Reflection  
This week transitioned the project into practical testing preparation. Setting up Git ensured proper project management, while the Testing Plan clarified what to evaluate and how to measure success. Beginning Unity prototyping made the design ideas more concrete, highlighting the need for functional gesture input and visible feedback for effective user testing in Week 4.




# Week 4 Studio Summary  
Course: DECO2300/7230  
Topic: Prototype Progress & Testing Preparation  

## Testing Preparation Checklist  
- Student ID prepared for identity verification (or temporary one from student center)  
- Concept validated (refer to A1.1 feedback)  
- Clear goals, aims, and assumptions defined for testing  
- Valid methodology established for testing process  
- Clear data collection plan (what and how data will be collected)  
- Testing steps defined (should take ~5 minutes)  
- Horizontal Unity prototype prepared, including environment, affordances, and interactions  
- Git repository active for committing code and test results  

If any of these steps are unclear or incomplete, discuss with the teaching team.  

## Prototype Progress  
- Current focus: preparing Unity prototype to demonstrate key environment and interaction features  
- Ensuring workflow is test-ready with visual and functional affordances  
- Repository setup and commits ongoing  

## Tutor Discussion Notes  
- Testing format: testing should be short (≤5 min) and focus on clarity, measuring whether gestures, menus, or interactions are intuitive and functional  
- Unity demonstration tips:  
  - Keep prototype interactions simple and clear  
  - Ensure affordances are visually distinct so participants understand what can be interacted with  
  - Visual cues and minimal feedback systems (highlight, glow, menu animation) improve clarity  
  - A well-prepared scene with essential interactions works better than a complex unfinished setup  

## Next Steps  
- Finalise Unity environment with clear affordances and spatial interactions  
- Add minimal but effective visual feedback to menus and gestures  
- Define specific testing tasks (~5 min each) aligned with research aims  
- Prepare Git commits and documentation for testing outcomes  