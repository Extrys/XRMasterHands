using System;
using UnityEngine;
using UnityEngine.XR.Hands;

[Serializable]
public class HandVelocityStream : IDisposable
{
	public Vector3 velocity;
	public Vector3 angularVelocity;
	XRMasterHand masterHand;
	public HandVelocityStream() { }
	public HandVelocityStream(XRMasterHand masterHand)
	{
		this.masterHand = masterHand;
		this.masterHand.OnJointsUpdated += OnJointsUpdated;
	}

	Vector3 lastPos;
	Quaternion lastRot;
	void OnJointsUpdated(XRHandJointsUpdatedEventArgs eventArgs)
	{
		Vector3 position = eventArgs.hand.rootPose.position;
		velocity = (position - lastPos) / Time.deltaTime;
		lastPos = eventArgs.hand.rootPose.position;
		Quaternion orientation = eventArgs.hand.rootPose.rotation;
		(orientation * Quaternion.Inverse(lastRot)).ToAngleAxis(out float angle, out Vector3 axis);
		angularVelocity = axis * (angle * Mathf.Deg2Rad / Time.deltaTime);
		lastRot = orientation;
	}

	public void Dispose()
	{
		masterHand.OnJointsUpdated -= OnJointsUpdated;
		masterHand = null;
	}
}
