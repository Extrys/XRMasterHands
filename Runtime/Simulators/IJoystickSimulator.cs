using UnityEngine.XR.Hands;

public interface IJoystickSimulator
{
	void Initialize(IStatePerformer statePerformer, XRHandJointsUpdatedEventArgs jointsUpdatedEventArgs);
	byte X { get; }
	byte Y { get; }
}
