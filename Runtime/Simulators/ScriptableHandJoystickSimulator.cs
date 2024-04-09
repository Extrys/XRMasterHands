using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Hands;

[CreateAssetMenu(fileName = "ScriptableHandJoystickSimulator", menuName = "XRMasterHands/ScriptableHandJoystickSimulator")]
public class ScriptableHandJoystickSimulator : ScriptableJoystickSimulator
{
	public float arrowWidth = 0.1f;
	public Material arrowMaterial;
	public override IJoystickSimulator GenerateSimulator(IStatePerformer statePerformer, XRHandJointsUpdatedEventArgs jointsUpdatedEventArgs)
	{
		IJoystickSimulator pinchJoystickSimulator = GenerateSimulatorObject();
		pinchJoystickSimulator.Initialize(statePerformer, jointsUpdatedEventArgs);
		return pinchJoystickSimulator;
	}

	IJoystickSimulator GenerateSimulatorObject()
	{
		Mesh mesh = new Mesh();
		Vector3[] vertices = new Vector3[8];
		int ringVertexLength = (vertices.Length - 2);
		vertices[0].Set(0, 0, 1);
		for (int i = 1; i < ringVertexLength+1; i++)
		{
			float angle = (i-1) * ((2f*Mathf.PI) / ringVertexLength);
			vertices[i] = new Vector3(Mathf.Cos(angle) * arrowWidth, Mathf.Sin(angle) * arrowWidth, .75f);
		}


		int[] triangles = new int[ringVertexLength * 2 * 3];
		for (int i = 0; i < ringVertexLength * 2; i++)
		{
			triangles[(i*3)] = i>=ringVertexLength ? (vertices.Length-1) : 0;
			triangles[(i*3) + (i >= ringVertexLength ? 2 : 1)] = (i % ringVertexLength) + 1;
			triangles[(i*3) + (i >= ringVertexLength ? 1 : 2)] = (triangles[(i * 3) + (i >= ringVertexLength ? 2 : 1)] % ringVertexLength) + 1;
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		GameObject arrow = new GameObject("JoystickArrow");
		arrow.AddComponent<MeshRenderer>().material = arrowMaterial;
		arrow.AddComponent<MeshFilter>().mesh = mesh;
		PinchJoystickSimulator pinchJoystickSimulator = arrow.AddComponent<PinchJoystickSimulator>();
		pinchJoystickSimulator.gameObject.SetActive(false);
		if(Application.isPlaying)
			pinchJoystickSimulator.hideFlags = HideFlags.HideInHierarchy;
		return pinchJoystickSimulator;
	}

	public InputAction joystickStarter;
}
