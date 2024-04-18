using Unity.XR.CoreUtils;

public class XRMasterHandsRig : XROrigin
{
	new void Awake()
	{
		base.Awake();
		XRMasterHandsRigHandler.SetActiveRig(this);
	}
}