using UnityEngine;
using UnityEngine.XR.Hands;

public class XRMasterHandsAgent : MonoBehaviour
{
	XRMasterHand left, right;
	void Awake()
	{
		left = new(HandProxyDevice.GetOrCreate(Handedness.Left));
		right = new(HandProxyDevice.GetOrCreate(Handedness.Right));
	}
	void OnDestroy()
	{
		left.Dispose();
		right.Dispose();
	}

	void Reset()
	{
		if (!Application.isPlaying)
			Destroy(this);
	}

#if !SKIP_MASTERXRHANDS_AUTO_INSTANCING
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void CreateHandPair()
	{
		if (!Application.isPlaying) 
			return;
		GameObject lifecicle = new("XRMasterHandsLifecicle", typeof(XRMasterHandsAgent));
		DontDestroyOnLoad(lifecicle);
		lifecicle.hideFlags = HideFlags.HideInHierarchy;
	}
#endif
}
