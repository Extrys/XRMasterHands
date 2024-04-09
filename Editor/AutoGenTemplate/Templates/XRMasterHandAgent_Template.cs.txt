using UnityEngine;
using UnityEngine.XR.Hands;

public class XRMasterHandAgent : MonoBehaviour
{
	XRMasterHand xrMasterHand;
	[SerializeField] Handedness handedness;
	void Awake() => xrMasterHand = new(HandProxyDevice.GetOrCreate(handedness));
	void OnDestroy() => xrMasterHand.Dispose();
}