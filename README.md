

<p align="center">
  <img src="https://github.com/Extrys/XRMasterHands/assets/38926085/8f0f3ed4-f6ad-4ee5-9c91-ebf21cfc4553">
</p>
<p align="center">
  Documentation in progress...
</p>

## Table Of Contents
<details>
<summary>Details</summary>

  - [Introduction](#introduction)
  - [Features](#features)
  - [Setup](#setup)
  - [Basic Usage](#basic-usage) (wip)
  - [XRMasterHand SkeletonDriver](#xrmasterhand-skeletondriver) (wip)
  	- [SkeletonDriver Usages and Tricks](#skeletondriver-usages-and-tricks) (wip)
  - [Input Setup](#input-setup) (wip)
  - [Joystick Simulator](#joystick-simulator) (wip)

</details>


## Introduction

XRMasterHands is designed to simplify the implementation of OpenXR hand tracking using Unity's Input System, making it significantly easier for developers to add hand interactions to their projects. This tool streamlines the integration process, enabling developers to focus more on creativity and less on technical complexities.

This project is open source.

For general troubleshooting, support, or to report a bug, please submit an issue with the appropriate tag on this repo.  
If you have a fix or an enhancement to propose, feel free to submit a pull request.

You can also add me on discord `extrys` or [twitter/X](https://twitter.com/ExtrysGO) for direct contact. 

*As i have a slight level of dyslextia, im sory before hand (never better said) for any typo you may find, feel free to contribute fixing them if you find any!*

## Features

- **Generated Hand Proxy Device**
  - *Automatically generates hand controllers for seamless integration with Unity's Input System, simplifying the setup process and enhancing usability within VR applications.*
- **Pose Descriptor for Easy Device Generation Authoring**
  - *Utilize the pose descriptor to author and customize the hand controllers effortlessly. Add your own poses and integrate joystick simulators to adapt the controllers to specific needs and functionalities.*
- **Simple Skeleton Driver**
  - *Offers straightforward control over hand skeletons, simplifying interactions with hand rigs.*
- **Joystick Simulator Framework**
  - *Incorporate your own joystick logic into the hand controllers.*
- **Extends XR Hand Controller** ***(WORK IN PROGRESS)***
  - *Enhances XR hand controllers by adding advanced features such as velocity tracking and finger state recognition.*




## Setup

### Adding the Package to Unity

To install the XRMasterHands package:

1.  Go to `Window > Package Manager`.
2.  Click the '+' button and select `Add package from git URL...`.
3.  Enter the URL: `https://github.com/Extrys/XRMasterHands.git`.

![Package Installation](https://github.com/Extrys/XRMasterHands/assets/38926085/1ce14ffe-24df-4461-adbc-f2cf6c03e7d1)

After the installation is complete, XRMasterHands automatically creates a new folder within the Unity Asset folder named "XRMasterHands_Generated." This folder is crucial for the proper functioning of the package and includes key components:

-   **Input Device Proxy**: This is a procedural, auto-updated input device included in the folder, which facilitates interaction with Unity's input system, allowing it to react dynamically to user inputs.
-   **HandProxyGestureDescription**: Located inside the "XRMasterHands_Generated/Resources" folder.
This scriptable object is essential for setting up your controller.


### Initial Configuration

To configure your controller for hand interactions, follow these steps:

1.  **Navigate to the Resources Folder:**
    
    -   Go to the "XRMasterHands_Generated/Resources" directory in Unity and locate the `HandProxyGestureDescription` scriptable object. This object is crucial for defining hand poses and interactions specific to your application.
    
    ![Configuration Image](https://github.com/Extrys/XRMasterHands/assets/38926085/950252d9-c67b-475c-ac23-3605c4a536a3)
    
2.  **Know Your Settings:**
    
    -   **Poses:** This field is where you add the poses that your controller will react to. Before adding poses, you need to create them. Refer to the [XR Hands documentation](https://docs.unity3d.com/Packages/com.unity.xr.hands@1.4/manual/gestures/custom-gestures.html) for guidance on creating poses. Alternatively, you can use predefined poses by importing the Gestures sample from the XRMasterHands package. This option provides a quick way to implement common gestures and can be a great starting point for testing and further customization.
    -   **Joystick Pose (Optional):** Add a pose for joystick functionality if needed. Upon adding a pose, the "Scriptable Simulator" field will become available where you can include your custom joystick simulation logic. Use the "ScriptablePinchJoystickSimulator" from `Packages/XRMasterHands/Runtime/ScriptableObjects` for a pre-programmed air joystick option.
    - **Use Optimized Controls (Needs testing):** Toggle this option to inform the input system to use optimized controls for better performance.

3.  **Add Poses:**
    
    -   **Pose to Add:** Use the "Add" button below this field to include new poses. The scriptable object ensures no duplicate names in the list to prevent naming conflicts in the generated code.
    -   **Pose List:** Below the "Add" button, you will find a list of all added poses. Each pose has a foldout on the right where you can set additional parameters such as "Detection Interval" and "Minimum Hold Time."
4.  **Finalize Configuration:**
    
	-   **Refresh the Proxy Device:** Press the `Refresh Proxy Device` button to finalize the setup. This action ensures that all your settings are applied, and the device is ready for use. 
	-   **Force Recompilation (if needed):** If the Unity editor does not automatically recompile after these changes, press `Ctrl + R` to force a recompilation. This ensures that all recent modifications are updated and functioning correctly in the editor.


This setup prepares your development environment to utilize XRMasterHands fully, enhancing your VR projects with its capabilities.


## Basic Usage
Once everything is set up and you're ready to start using XRMasterHands, let's dive into a practical example to get you going. 

As this package is designed to work seamlessly with the unity input system we are going to use Input actions for it, you can use an InputActionMap or and InputAction field [please refeer to unity's input system documentation for more information]

## XRMasterHand SkeletonDriver
This is the XRMasterHandSkeletonDriver monobehaviour component
// Explain  

(WIP)

### SkeletonDriver Usages and Tricks
Tricks and so  

(WIP)




## Joystick Simulator
(WIP)


## Projects Using XRMasterHands

I love to see how XRMasterHands is being used in diverse projects! If you are using this package and would like your project to be featured in this list, please post an issue on my GitHub repository with the details of your project. I welcome submissions from all types of applications and look forward to showcasing how XRMasterHands is helping to innovate and enhance virtual reality experiences.

### How to Submit Your Project

To submit your project for inclusion:

1.  Go to the [Issues section of my GitHub repository].
2.  Create a new issue with the tag "project submission" and [Your Project Name] as title.
3.  In the body of the issue, you can include a brief description of your project, how XRMasterHands has been integrated, and any relevant links or images.
4.  Submit the issue. I will review submissions regularly and update the list accordingly.

I appreciate your contributions to the XRMasterHands community and am excited to feature your work!
