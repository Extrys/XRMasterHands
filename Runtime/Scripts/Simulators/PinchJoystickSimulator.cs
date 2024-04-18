using UnityEngine;
using UnityEngine.XR.Hands;

public class PinchJoystickSimulator : MonoBehaviour, IJoystickSimulator //TODO: Avoid using static Active rig, use signals instead
{
	IStatePerformer joystickStarter;
	public Vector3 beginPos;
	public Transform character;
	public float maxDistance = 0.2f;
 	public byte x,y;
	public float visualVerticalPermisivity = .1f;
	XRHandJointsUpdatedEventArgs jointsUpdatedEventArgs;

	public byte X => x;
	public byte Y => y;

	public void Initialize(IStatePerformer statePerformer, XRHandJointsUpdatedEventArgs jointsUpdatedEventArgs)
	{
		this.joystickStarter = statePerformer;
		joystickStarter.PerformStateChanged += OnPerformStateChanged;
		OnRigChanged(XRMasterHandsRigHandler.ActiveRig);
		XRMasterHandsRigHandler.ActiveRigChanged += OnRigChanged;
		this.jointsUpdatedEventArgs = jointsUpdatedEventArgs;
	}
	void OnRigChanged(XRMasterHandsRig rig)
	{
		enabled = rig;
		if(rig != null)
			character = rig.Origin.transform;
	}

	public void OnPerformStateChanged(bool performState)
	{
		gameObject.SetActive(performState);
		Pose thumbPose;
		jointsUpdatedEventArgs.hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out thumbPose);
		if (performState)
			beginPos = thumbPose.position;
		else
   			x = y = 127;
	}
	void Update()
	{
		Pose thumbPose = default;
		if(jointsUpdatedEventArgs.hand.isTracked)
			jointsUpdatedEventArgs.hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out thumbPose);

		Vector3 vector = (thumbPose.position - beginPos);
		float oLength = vector.magnitude;
		Vector3 oNormalized = vector / (oLength>0 ? oLength : 1);
		float oClampedLength = Mathf.Clamp(oLength, 0, visualVerticalPermisivity);
		vector.y = 0;
		float length = vector.magnitude;
		Vector3 normalized = vector / (length > 0 ? length : 1); ;
		float clampedLength = Mathf.Clamp(length, 0, maxDistance);
		Vector3 jtVector = normalized * clampedLength / maxDistance;
		float viewAngle = Vector3.SignedAngle(Vector3.forward, XRMasterHandsRigHandler.ActiveRig.Camera.transform.forward, Vector3.up);
           	jtVector = Quaternion.Euler(0, -viewAngle + character.eulerAngles.y, 0) * jtVector;
	    
  		x = (byte)((1 + jtVector.x) * 127);
		y = (byte)((1 + jtVector.z) * 127);

		Vector3 limitedhorizontal = (normalized * clampedLength);
		Vector3 limitedVertical = (oNormalized * oClampedLength);

		Vector3 startPos = beginPos;
		Vector3 endPos = startPos + new Vector3(limitedhorizontal.x, limitedVertical.y, limitedhorizontal.z);

		transform.position = character.TransformPoint(beginPos);
		if(clampedLength > 0.001f)
			transform.forward = character.TransformDirection(endPos - startPos);
		transform.localScale = new Vector3(1, 1, (endPos - startPos).magnitude);
	}
	private void OnDestroy()
	{
		joystickStarter.PerformStateChanged -= OnPerformStateChanged;
		XRMasterHandsRigHandler.ActiveRigChanged -= OnRigChanged;
	}
}
