// Modified helper class from com.unity.xr.hands package to work with XRMasterHands package.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Hands;

public static class XRMasterHandSkeletonDriverUtility
{
	/// <summary>
	/// Set up the joint references for an <see cref="XRMasterHandSkeletonDriver"/> by checking the
	/// hierarchy and comparing the children GameObject's names with the names of the joints.
	/// The GameObjects names are expected to either start or end with the name of the joint.
	/// </summary>
	/// <remarks>
	/// This method is expensive because it searches the hierarchy by comparing the names of GameObjects so
	/// it should not be called frequently. Instead, the joint references can be set once and serialized
	/// in either the scene or a prefab.
	/// </remarks>
	/// <param name="skeletonDriver"> The <see cref="XRMasterHandSkeletonDriver"/> component to setup.</param>
	/// <param name="missingJointNames">A list of strings that will be cleared and populated with the joints that are missing.</param>
	public static void FindJointsFromRoot(XRMasterHandSkeletonDriver skeletonDriver, List<string> missingJointNames = null)
	{
		void SetupJointTransformReference(
		    XRHandJointID jointId,
		    Transform jointTransform)
		{
			var reference = new JointToTransformReference
			{
				jointTransform = jointTransform,
				xrHandJointID = jointId
			};

			skeletonDriver.jointTransformReferences.Add(reference);
		}

		if (missingJointNames != null)
			missingJointNames.Clear();

		skeletonDriver.jointTransformReferences.Clear();

		Transform wristRootTransform = null;

		// First check if the parent itself is the wrist
		var rootTransformName = skeletonDriver.rootTransform.name;
		if (StartsOrEndsWith(rootTransformName, XRHandJointID.Wrist.ToString()))
		{
			wristRootTransform = skeletonDriver.rootTransform;
		}
		else // Otherwise check under parent for wrist joint as a child
		{
			for (var childIndex = 0; childIndex < skeletonDriver.rootTransform.childCount; ++childIndex)
			{
				var child = skeletonDriver.rootTransform.GetChild(childIndex);
				if (child.gameObject.name.EndsWith(XRHandJointID.Wrist.ToString()))
					wristRootTransform = child;
			}
		}

		if (wristRootTransform == null)
		{
			if (missingJointNames != null)
				missingJointNames.Add(XRHandJointID.Wrist.ToString());
		}
		else
		{
			SetupJointTransformReference(XRHandJointID.Wrist, wristRootTransform);
			Transform palmTransform = null;

			// Find all the joints under the wrist
			for (var childIndex = 0; childIndex < wristRootTransform.childCount; ++childIndex)
			{
				var child = wristRootTransform.GetChild(childIndex);

				// Find the palm joint
				if (child.name.EndsWith(XRHandJointID.Palm.ToString()))
				{
					palmTransform = child;
					continue;
				}

				// Find the finger joints
				for (var fingerIndex = (int)XRHandFingerID.Thumb;
				     fingerIndex <= (int)XRHandFingerID.Little;
				     ++fingerIndex)
				{
					var fingerId = (XRHandFingerID)fingerIndex;
					var jointIdFront = fingerId.GetFrontJointID();
					if (!StartsOrEndsWith(child.name, jointIdFront.ToString()))
						continue;

					SetupJointTransformReference(jointIdFront, child);
					var lastChild = child;
					var jointIndexBack = fingerId.GetBackJointID().ToIndex();

					// Find the rest of the joints for the finger
					for (var jointIndex = jointIdFront.ToIndex() + 1;
					     jointIndex <= jointIndexBack;
					     ++jointIndex)
					{
						// Find the next child that ends with the joint name
						var jointName = XRHandJointIDUtility.FromIndex(jointIndex).ToString();
						for (var nextChildIndex = 0; nextChildIndex < lastChild.childCount; ++nextChildIndex)
						{
							var nextChild = lastChild.GetChild(nextChildIndex);
							if (StartsOrEndsWith(nextChild.name, jointName))
							{
								lastChild = nextChild;
								break;
							}
						}

						if (StartsOrEndsWith(lastChild.name, jointName))
						{
							var jointId = XRHandJointIDUtility.FromIndex(jointIndex);
							SetupJointTransformReference(jointId, lastChild);
						}
						else if (missingJointNames != null)
							missingJointNames.Add(jointName);
					}
				}
			}

			for (var fingerIndex = (int)XRHandFingerID.Thumb;
			     fingerIndex <= (int)XRHandFingerID.Little;
			     ++fingerIndex)
			{
				var fingerId = (XRHandFingerID)fingerIndex;
				var jointIdFront = fingerId.GetFrontJointID();

				// Check if front joint id is present in the list of joint references
				if (skeletonDriver.jointTransformReferences.Any(jointReference => jointReference.xrHandJointID == jointIdFront))
					continue;

				if (missingJointNames != null)
					missingJointNames.Add(jointIdFront.ToString());
			}

			if (palmTransform != null)
			{
				SetupJointTransformReference(XRHandJointID.Palm, palmTransform);
			}
			else if (missingJointNames != null)
			{
				missingJointNames.Add(XRHandJointID.Palm.ToString());
			}
		}
	}

	static bool StartsOrEndsWith(string value, string searchTerm) => value.StartsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase) || value.EndsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase);
}