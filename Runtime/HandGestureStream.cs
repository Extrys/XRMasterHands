using System;
using UnityEngine;
using UnityEngine.XR.Hands;

[Serializable]
public class HandGestureStream : IDisposable, IStatePerformer
{
	public byte performState;
	byte PerformState // is private just for internal events handling, reading from direct variable is faster than using a property
	{
		get => performState; set
		{
			if (performState != value)
			{
				performState = value;
				PerformStateChanged?.Invoke(performState == 1);
			}
		}
	}
	public event Action<bool> PerformStateChanged;

	bool wasDetected, performedTriggered;
	float timeOfLastConditionCheck, holdStartTime;

	HandGestureData data;
	XRMasterHand masterHand;


	public HandGestureStream() { }
	public HandGestureStream(XRMasterHand masterHand, HandGestureData data)
	{
		this.masterHand = masterHand;
		this.data = data;
		this.masterHand.OnJointsUpdated += OnJointsUpdated;
	}

	void OnJointsUpdated(XRHandJointsUpdatedEventArgs eventArgs)
	{
		if (Time.timeSinceLevelLoad < timeOfLastConditionCheck + data.detectionInterval)
			return;

		var detected = masterHand.IsTracked && data.handPose != null && data.handPose.CheckConditions(eventArgs);

		if (!wasDetected && detected)
			holdStartTime = Time.timeSinceLevelLoad;
		else if (wasDetected && !detected)
		{
			performedTriggered = false;
			NotifyGestureEnd();
		}

		wasDetected = detected;
		if (!performedTriggered && detected)
		{
			var holdTimer = Time.timeSinceLevelLoad - holdStartTime;
			if (holdTimer > data.minimumHoldTime)
			{
				NotifyGestureStart();
				performedTriggered = true;
			}
		}

		timeOfLastConditionCheck = Time.timeSinceLevelLoad;
	}

	void NotifyGestureStart() => PerformState = 1;
	void NotifyGestureEnd() => PerformState = 0;

	public void Dispose()
	{
		masterHand.OnJointsUpdated -= OnJointsUpdated;
		PerformStateChanged = null;
		data = null;
		masterHand = null;
	}
}
