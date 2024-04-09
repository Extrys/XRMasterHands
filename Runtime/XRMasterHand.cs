using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine;
using UnityEngine.XR.Hands;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.XR;

public class XRMasterHand : IDisposable
{
	public event Action OnTrackingAcquiredEvent, OnTrackingLostEvent;
	public event Action<bool> OnTrackingChanged;
	public event Action<Pose> OnPoseUpdated;
	public event Action<XRHandJointsUpdatedEventArgs> OnJointsUpdated;
	public bool IsTracked => m_Subsystem != null && m_Subsystem.running && handIsTracked.Value;
	public Pose rootPose => handJointsUpdatedEventArgs.hand.rootPose;

	public readonly XRHandJointsUpdatedEventArgs handJointsUpdatedEventArgs = new XRHandJointsUpdatedEventArgs();

	static readonly List<XRHandSubsystem> k_SubsystemsReuse = new List<XRHandSubsystem>();
	XRHandSubsystem m_Subsystem;

	public IHandProxyDevice ProxyController { get; private set; }

	public IReadOnlyBindableVariable<bool> BindableHandIsTracked => handIsTracked;
	readonly BindableVariable<bool> handIsTracked = new BindableVariable<bool>();
	CancellationTokenSource cts = new CancellationTokenSource();

	public Handedness handedness { get; private set; }
	UpdateType updateType = UpdateType.Dynamic;
	public XRMasterHand(IHandProxyDevice handProxyDevice)
	{
		handedness = handProxyDevice.Handeness;
		ProxyController = handProxyDevice;
		ProxyController.SetStreams(Resources.Load<HandProxyGestureDescription>("HandProxyGestureDescription"), this);
		GetSubsystemAsync(cts.Token).GetAwaiter();
	}


	public async Task GetSubsystemAsync(CancellationToken token)
	{
		if (m_Subsystem != null && m_Subsystem.running)
			return;

		Debug.Log("Waiting for XR Hand subsystem.");
		while (m_Subsystem == null)
		{
			SubsystemManager.GetSubsystems(k_SubsystemsReuse);
			bool subSystemFound = false;
			for (var i = 0; i < k_SubsystemsReuse.Count; ++i)
			{
				var handSubsystem = k_SubsystemsReuse[i];
				if (handSubsystem.running)
				{
					SetSubsystem(handSubsystem);
					subSystemFound = true;
					Debug.Log("XR Hand subsystem found.");
					break;
				}
			}
			if (!subSystemFound)
				await Task.Delay(200,token);
		}
	}

	void UnsubscribeFromSubsystem()
	{
		if (m_Subsystem != null)
		{
			m_Subsystem.trackingAcquired -= OnTrackingAcquired;
			m_Subsystem.trackingLost -= OnTrackingLost;
			m_Subsystem.updatedHands -= OnUpdatedHands;
			m_Subsystem = null;
		}
	}

	internal void SetSubsystem(XRHandSubsystem handSubsystem)
	{
		UnsubscribeFromSubsystem();

		m_Subsystem = handSubsystem;

		XRHand hand;
		if (handedness == Handedness.Left)
			hand = m_Subsystem.leftHand;
		else if (handedness == Handedness.Right)
			hand = m_Subsystem.rightHand;
		else
			hand = default;

		handJointsUpdatedEventArgs.hand = hand;

		m_Subsystem.trackingAcquired += OnTrackingAcquired;
		m_Subsystem.trackingLost += OnTrackingLost;
		m_Subsystem.updatedHands += OnUpdatedHands;

		TrackingAcquiredOrLost(hand.isTracked);
	}

	void OnTrackingAcquired(XRHand hand)
	{
		if (hand.handedness == handedness)
			TrackingAcquiredOrLost(true);
	}

	void OnTrackingLost(XRHand hand)
	{
		if (hand.handedness == handedness)
			TrackingAcquiredOrLost(false);
	}

	void TrackingAcquiredOrLost(bool isTracked)
	{
		handIsTracked.Value = isTracked;
		if (handIsTracked.Value)
			OnTrackingAcquiredEvent?.Invoke();
		else
			OnTrackingLostEvent?.Invoke();

		OnTrackingChanged?.Invoke(isTracked);
	}

	void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateEventType)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool UpdatingAs(UpdateType type) => (this.updateType & type) == type;
		if (updateEventType == XRHandSubsystem.UpdateType.Dynamic && !UpdatingAs(UpdateType.Dynamic) || updateEventType == XRHandSubsystem.UpdateType.BeforeRender && !UpdatingAs(UpdateType.BeforeRender))
			return;


		XRHandSubsystem.UpdateSuccessFlags jointUpdateFlagToCheck;
		XRHandSubsystem.UpdateSuccessFlags poseUpdateFlagToCheck;
		if (handedness == Handedness.Left)
		{
			jointUpdateFlagToCheck = XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints;
			poseUpdateFlagToCheck = XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose;
		}
		else
		{
			jointUpdateFlagToCheck = XRHandSubsystem.UpdateSuccessFlags.RightHandJoints;
			poseUpdateFlagToCheck = XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose;
		}

		bool requestedJointsUpdated = (updateSuccessFlags & jointUpdateFlagToCheck) != XRHandSubsystem.UpdateSuccessFlags.None;
		bool requestedRootPoseUpdated = (updateSuccessFlags & poseUpdateFlagToCheck) != XRHandSubsystem.UpdateSuccessFlags.None;

		if (requestedJointsUpdated || requestedRootPoseUpdated)
		{
			handJointsUpdatedEventArgs.hand = handedness == Handedness.Left ? m_Subsystem.leftHand : m_Subsystem.rightHand;
			if (requestedJointsUpdated)
				OnJointsUpdated?.Invoke(handJointsUpdatedEventArgs);
			if (requestedRootPoseUpdated)
				OnPoseUpdated?.Invoke(handJointsUpdatedEventArgs.hand.rootPose);
			ProxyController.RefreshInputs();
		}
	}

	public void Dispose()
	{
		cts.Cancel();
		cts.Dispose();
		UnsubscribeFromSubsystem();
		ProxyController.Dispose();
	}

	[Flags]
	public enum UpdateType
	{
		None = 0,
		Dynamic = 1 << 0,
		BeforeRender = 1 << 1,
	}
}
