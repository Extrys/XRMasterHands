using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Hands;

public class PinchJoystickSimulator : MonoBehaviour, IJoystickSimulator
{
	IStatePerformer joystickStarter;
	public Vector3 beginPos;
	public Transform character;
	public float maxDistance = 0.2f;
	public Vector2 joystickVector;
	public float visualVerticalPermisivity = 1f;
	XRHandJointsUpdatedEventArgs jointsUpdatedEventArgs;

	public byte X => (byte)((1 + joystickVector.x) * 127);
	public byte Y => (byte)((1 + joystickVector.y) * 127);

	public void Initialize(IStatePerformer statePerformer, XRHandJointsUpdatedEventArgs jointsUpdatedEventArgs)
	{
		this.joystickStarter = statePerformer;
		joystickStarter.PerformStateChanged += OnPerformStateChanged;
		XROrigin origin =Object.FindObjectOfType<XROrigin>(true);
		character = origin.Origin.transform;
		this.jointsUpdatedEventArgs = jointsUpdatedEventArgs;
	}

	public void OnPerformStateChanged(bool performState)
	{
		gameObject.SetActive(performState);
		Pose thumbPose;
		jointsUpdatedEventArgs.hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out thumbPose);
		if (performState)
			beginPos = thumbPose.position;
		else
			joystickVector = Vector2.zero;
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
		joystickVector = new Vector2(jtVector.x, jtVector.z);

		Vector3 limitedhorizontal = (normalized * clampedLength);
		Vector3 limitedVertical = (oNormalized * oClampedLength);

		Vector3 startPos = beginPos;
		Vector3 endPos = startPos + new Vector3(limitedhorizontal.x, limitedVertical.y, limitedhorizontal.z);

		transform.position = character.TransformPoint(beginPos);
		if(clampedLength > 0.001f)
			transform.forward = character.TransformDirection(endPos - startPos);
		transform.localScale = new Vector3(1, 1, (endPos - startPos).magnitude);

		//character.position += new Vector3(joystickVector.x, 0, joystickVector.y) * Time.deltaTime * 3;
	}
	private void OnDestroy() => joystickStarter.PerformStateChanged -= OnPerformStateChanged;

}