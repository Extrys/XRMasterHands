using System;
using UnityEngine;
using UnityEngine.XR.Hands.Gestures;

[Serializable]
public class HandGestureData // Made class for later SO DB calibrations
{
	[Tooltip("The hand shape or pose that must be detected for the gesture to be performed.")]
	public XRHandPose handPose;
	[Tooltip("The minimum amount of time the hand must be held in the required shape and orientation for the gesture to be performed.")]
	public float minimumHoldTime;
	[Tooltip("The interval at which the gesture detection is performed.")]
	public float detectionInterval;
}
