using System;
using UnityEngine.XR.Hands;

public interface IHandProxyDevice : IDisposable
{
	Handedness Handeness { get; }
	void RefreshInputs();
	void SetJoystickSimulator(IJoystickSimulator joystickSimulator);
	void SetStreams(HandProxyGestureDescription handProxyGestureDescription, XRMasterHand xrMasterHand);
}