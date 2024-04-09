using UnityEngine;
using UnityEngine.XR.Hands;

public abstract class ScriptableJoystickSimulator : ScriptableObject
{
	public abstract IJoystickSimulator GenerateSimulator(IStatePerformer statePerformer, XRHandJointsUpdatedEventArgs jointsUpdatedEventArgs);
}