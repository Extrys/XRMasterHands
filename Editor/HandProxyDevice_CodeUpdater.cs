using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

public class HandProxyDevice_CodeUpdater
{
	public static void RegenerateClass(HandProxyGestureDescription desc)
	{
#if XRMASTERHANDS_AUTOGEN_DONE
		string filePath = XRMasterHands_Generator.generatedHandProxyDeviceScriptPath;
		string fileContent = File.ReadAllText(filePath);

		string editedFileContent = fileContent;

		string[] proxiedButtonControls = new string[desc.gestureDatas.Length];
		for (int i = 0; i < desc.gestureDatas.Length; i++)
			proxiedButtonControls[i] = $"[InputControl] public ButtonControl {Regex.Replace(desc.gestureDatas[i].handPose.name, @"[^a-zA-Z0-9]", "_")} {{ get; private set; }}";
		editedFileContent = WriteBlockInsideRegion(editedFileContent, "PROXIED_BUTTONCONTROLS", proxiedButtonControls);

		string[] buttonControlsSetups = new string[desc.gestureDatas.Length];
		for (int i = 0; i < desc.gestureDatas.Length; i++)
			buttonControlsSetups[i] = $"{Regex.Replace(desc.gestureDatas[i].handPose.name, @"[^a-zA-Z0-9]", "_")}  = GetChildControl<ButtonControl>(\"{Regex.Replace(desc.gestureDatas[i].handPose.name, @"[^a-zA-Z0-9]", "_")}\");";
		editedFileContent = WriteBlockInsideRegion(editedFileContent, "BUTTONCONTROLS_SETUP", buttonControlsSetups);

		string[] poseWrites = new string[desc.gestureDatas.Length];
		for (int i = 0; i < desc.gestureDatas.Length; i++)
			poseWrites[i] = $"{Regex.Replace(desc.gestureDatas[i].handPose.name, @"[^a-zA-Z0-9]", "_").ToLower()} = streams[{i}].performState,";
		editedFileContent = WriteBlockInsideRegion(editedFileContent, "POSE_WRITES", poseWrites);

		string[] poseUints = new string[desc.gestureDatas.Length];
		for (int i = 0; i < desc.gestureDatas.Length; i++)
		{
			string fieldName = Regex.Replace(desc.gestureDatas[i].handPose.name, @"[^a-zA-Z0-9]", "_").ToLower();
			poseUints[i] = $"[InputControl(name = \"{fieldName}\", layout = \"Button\", bit = 0)] public byte {fieldName};";
		}
		editedFileContent = WriteBlockInsideRegion(editedFileContent, "POSE_UINTS", poseUints);


		File.WriteAllText(filePath, editedFileContent);
#endif
	}

	static string WriteBlockInsideRegion(string source, string regionName, string[] block)
	{
		string startPattern = $@"#region {regionName}.*(\r?\n)";
		string endPattern = @".*#endregion";

		Match startRegionMatch = Regex.Match(source, startPattern);
		if (!startRegionMatch.Success)
		{
			Debug.LogError($"Start region '{regionName}' not found in source string");
			return source;
		}
		int startRegionIndex = startRegionMatch.Index;

		// Encuentra el final de la línea de la región para mantener todo después de #region {regionName}.
		int lineEndIndex = source.IndexOf(Environment.NewLine, startRegionIndex);
		if (lineEndIndex == -1) lineEndIndex = source.Length; // Si no hay salto de línea, asume el final del texto.

		// Encuentra la cantidad de tabulaciones antes del #region.
		int lineStart = source.LastIndexOf(Environment.NewLine, startRegionIndex) + Environment.NewLine.Length;
		string indent = source.Substring(lineStart, startRegionIndex - lineStart); // Consigue la indentación actual antes de #region.

		Match endRegionMatch = Regex.Match(source.Substring(startRegionIndex), endPattern);
		if (!endRegionMatch.Success)
		{
			Debug.LogError($"End region for '{regionName}' not found in source string");
			return source;
		}

		int endRegionIndex = startRegionIndex + endRegionMatch.Index;

		string sourceBeforeRegion = source.Substring(0, startRegionIndex + startRegionMatch.Length);
		string sourceAfterRegion = source.Substring(endRegionIndex);

		string indentedBlock = indent + string.Join(Environment.NewLine + indent, block);

		string result = sourceBeforeRegion + indentedBlock + Environment.NewLine  + sourceAfterRegion;
		return result;
	}
}