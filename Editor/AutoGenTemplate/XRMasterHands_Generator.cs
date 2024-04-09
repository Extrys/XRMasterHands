using UnityEngine;
using UnityEditor;
using System.IO;

public static class XRMasterHands_Generator
{
	static string templatesPath;
	static string resultPath = "Assets/XRMasterHands_Generated";
	public static string generatedHandProxyDeviceScriptPath = "Assets/XRMasterHands_Generated/HandProxyDevice.cs";

#if !XRMASTERHANDS_AUTOGEN_DONE
	[InitializeOnLoadMethod]
	static void GenerateFiles()
	{
		Debug.Log(GetTemplatesFolder());
		templatesPath = $"{GetTemplatesFolder()}/Templates";
		if (!Directory.Exists(resultPath))
			Directory.CreateDirectory(resultPath);
		
		string[] files = Directory.GetFiles(templatesPath);

		string templatePattern = "_Template."; 
		for(int i = 0; i < files.Length; i++)
		{
			string file = files[i];
			if(file.Contains(".meta"))
				continue;
			string fileName = Path.GetFileName(file);

			int templatesIndex = fileName.IndexOf(templatePattern);
			string resultFilename = fileName.Substring(0, templatesIndex);
			string extension = fileName.Substring(templatesIndex + templatePattern.Length).Split('.')[0];


			string newFilePath = Path.Combine(resultPath, $"{resultFilename}.{extension}");
			File.Copy(file, newFilePath, true);
			string text = File.ReadAllText(newFilePath);
			File.WriteAllText(newFilePath, text);
		}

		string directive = "XRMASTERHANDS_AUTOGEN_DONE";
		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, directive);
		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.PS5, directive);
		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, directive);

            HandProxyGestureDescription desc = ScriptableObject.CreateInstance<HandProxyGestureDescription>();
		if (!Directory.Exists(resultPath+ "/Resources"))
			Directory.CreateDirectory(resultPath+ "/Resources");
		if (!File.Exists($"{resultPath}/Resources/HandProxyGestureDescription.asset"))
			AssetDatabase.CreateAsset(desc, $"{resultPath}/Resources/HandProxyGestureDescription.asset");
		AssetDatabase.Refresh();
	}
#endif
	//hack to get "this" file path
	public static string GetTemplatesFolder()
	{
		string thisPath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
		thisPath = thisPath.Substring(0, thisPath.LastIndexOf("\\")+1);
		return thisPath.Replace(Path.GetFullPath(Application.dataPath), "Assets");
	}
} 