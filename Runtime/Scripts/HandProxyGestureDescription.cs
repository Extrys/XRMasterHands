using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.XR.Hands;

public class HandProxyGestureDescription : ScriptableObject
{
	[Tooltip("If this is enabled, this component will enable the Input System internal feature flag 'USE_OPTIMIZED_CONTROLS'. You must have at least version 1.5.0 of the Input System and have its backend enabled for this to take effect.")]
	public bool useOptimizedControls;

	public HandGestureData joystickStarter;
	public ScriptableHandJoystickSimulator scriptableSimulator;

	public HandGestureData[] gestureDatas;

	public HandGestureData[] AllGestureDatas()
	{
		List<HandGestureData> datas = new List<HandGestureData>();
		if(joystickStarter != null)
			datas.Add(joystickStarter);
		for (int i = 0; i < gestureDatas.Length; i++)
			datas.Add(gestureDatas[i]);
		return datas.ToArray();
	}
	public IJoystickSimulator GenerateJoystickSimulator(IStatePerformer statePerformer, XRHandJointsUpdatedEventArgs jointsUpdatedEventArgs) => scriptableSimulator ? scriptableSimulator.GenerateSimulator(statePerformer, jointsUpdatedEventArgs) : new NullJoystickSimulator();
	public HandGestureStream GenerateJoystickStarterStream(XRMasterHand masterXRHand) => joystickStarter != null ? new HandGestureStream(masterXRHand, joystickStarter) : new HandGestureStream();
	public HandGestureStream[] GenerateStreams(XRMasterHand masterXRHand)
	{
		HandGestureStream[] streams = new HandGestureStream[gestureDatas.Length];
		for (int i = 0; i < gestureDatas.Length; i++)
			streams[i] = new HandGestureStream(masterXRHand, gestureDatas[i]);
		return streams;
	}

	public bool TryAddGestureData(HandGestureData data)
	{
		if (gestureDatas == null)
			gestureDatas = new HandGestureData[0];

		string nameToAdd = Regex.Replace(data.handPose.name, @"[^a-zA-Z0-9]", "_");
		string[] currentNames = GetCleanPoseNames();
		for (int i = 0; i < currentNames.Length; i++)
			if (currentNames[i] == nameToAdd)
				return false;

		Array.Resize(ref gestureDatas, gestureDatas.Length + 1);
		gestureDatas[gestureDatas.Length - 1] = data;
		return true;
	}
	public bool CheckForErrors(out string error)
	{
		string[] poseNames = GetCleanPoseNames();
		HashSet<string> uniques = new HashSet<string>(poseNames);
		if (poseNames.Length != uniques.Count)
		{
			error = "There is posible generated pose name duplicates, Remove duplicates and try again, note that all non alphanumerical characters are replaced internally by an underscore (_) try avoiding special characters";
			return true;
		}

		error = "";
		return false;
	}

	string[] GetCleanPoseNames()
	{
		string[] poseNames = new string[gestureDatas.Length];
		for (int i = 0; i < gestureDatas.Length; i++)
			poseNames[i] = Regex.Replace(gestureDatas[i].handPose.name, @"[^a-zA-Z0-9]", "_");
		return poseNames;
	}
}
