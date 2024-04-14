using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine;
using UnityEngine.XR.Hands;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.XR;
using UnityEngine.InputSystem;

public class XRMasterHand : IDisposable
{
	static XRMasterHand left, right;
	public static XRMasterHand GetCreated(Handedness handedness) => handedness switch
	{
		Handedness.Left => left,
		Handedness.Right => right,
		_ => null,
	};

	public event Action OnTrackingAcquiredEvent, OnTrackingLostEvent;
	public event Action<bool> OnTrackingChanged;
	public event Action<Pose> OnPoseUpdated, OnPoseUpdatedVisual;
	public event Action<XRHandJointsUpdatedEventArgs> OnJointsUpdated, OnJointsUpdatedVisual;
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
	public XRMasterHand(IHandProxyDevice handProxyDevice)
	{
		handedness = handProxyDevice.Handeness;

		if(handedness == Handedness.Left)
		{
			if (left != null)
				return;
			left = this;
		}
		else if (handedness == Handedness.Right)
		{
			if (right != null)
				return;
			right = this;
		}

		var description = Resources.Load<HandProxyGestureDescription>("HandProxyGestureDescription");
		if (description != null)
			InputSystem.settings.SetInternalFeatureFlag("USE_OPTIMIZED_CONTROLS", description.useOptimizedControls);

		ProxyController = handProxyDevice;
		ProxyController.SetStreams(description, this);
		GetSubsystemAsync(cts.Token).GetAwaiter();
	}



	async Task GetSubsystemAsync(CancellationToken token)
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

	void SetSubsystem(XRHandSubsystem handSubsystem)
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
			switch (updateEventType)
			{
				case XRHandSubsystem.UpdateType.Dynamic:
					LogicUpdate(requestedJointsUpdated, requestedRootPoseUpdated);
					break;
				case XRHandSubsystem.UpdateType.BeforeRender:
					VisualUpdate(requestedJointsUpdated, requestedRootPoseUpdated);
					break;
			}
		
		}
	}
	void LogicUpdate(bool requestedJointsUpdated, bool requestedRootPoseUpdated)
	{
		if (requestedJointsUpdated)
			OnJointsUpdated?.Invoke(handJointsUpdatedEventArgs);
		if (requestedRootPoseUpdated)
			OnPoseUpdated?.Invoke(handJointsUpdatedEventArgs.hand.rootPose);
		ProxyController.RefreshInputs();
	}
	void VisualUpdate(bool requestedJointsUpdated, bool requestedRootPoseUpdated)
	{
		if (requestedJointsUpdated)
			OnJointsUpdatedVisual?.Invoke(handJointsUpdatedEventArgs);
		if (requestedRootPoseUpdated)
			OnPoseUpdatedVisual?.Invoke(handJointsUpdatedEventArgs.hand.rootPose);
	}

	public void Dispose()
	{
		cts.Cancel();
		cts.Dispose();
		UnsubscribeFromSubsystem();
		ProxyController.Dispose();
		left = null;
		right = null;
	}

	[Flags]
	public enum UpdateType
	{
		None = 0,
		Dynamic = 1 << 0,
		BeforeRender = 1 << 1,
	}
}
