using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class HgtToTrn : EditorWindow
{
	private string hgtPath;
	private string trnPath;
	private int hgtResolution = 30;

	[MenuItem("Tools/Quad Terrain Gen/Hgt To Terrain")]
	public static void ShowExample()
	{
		HgtToTrn wnd = GetWindow<HgtToTrn>();
		wnd.titleContent = new GUIContent("Hgt To Trn");
	}

	public void CreateGUI()
	{
		hgtPath = EditorGUILayout.TextField(".hgt Path", hgtPath);
		trnPath = EditorGUILayout.TextField(".trn Path", trnPath);

		if (GUILayout.Button("Convert"))
		{
			Convert();
		}
	}

	private void Convert()
	{

	}
}
