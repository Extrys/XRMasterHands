using UnityEngine.XR.Hands;

public struct NullJoystickSimulator : IJoystickSimulator
{
	public void Initialize(IStatePerformer statePerformer, XRHandJointsUpdatedEventArgs jointsUpdatedEventArgs) { }
	public byte X => 0;
	public byte Y => 0;
}
