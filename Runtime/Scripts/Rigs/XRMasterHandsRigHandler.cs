using System;

public class XRMasterHandsRigHandler
{
	static XRMasterHandsRig activeRig;
	public static XRMasterHandsRig ActiveRig => activeRig;
	public static event Action<XRMasterHandsRig> ActiveRigChanged;
	public static void SetActiveRig(XRMasterHandsRig rig)
	{
		if(activeRig != rig)
		{
			activeRig = rig;
			ActiveRigChanged?.Invoke(rig);
		}
	}
}
