using System;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class HgtToTrn : EditorWindow
{
	private string hgtPath = "";
	private string trnPath = "";

	private bool hgtIsOneArcSec = false;
	private int trnResolution = 4096;

	private float[,] heighMap;
	Texture2D heightMapTexture;
	private bool MapGenerated;

	private Vector2 scrollPos;

	[MenuItem("Tools/Quad Terrain Gen/Hgt To Terrain")]
	public static void ShowExample()
	{
		HgtToTrn wnd = GetWindow<HgtToTrn>();
		
	}

	public void OnGUI()
	{
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

		//settings
		if (!MapGenerated)
		{
			GUILayout.Label("Hgt Settings", EditorStyles.boldLabel);
			hgtPath = EditorGUILayout.TextField(".hgt Path", hgtPath);
			hgtIsOneArcSec = EditorGUILayout.Toggle("is SRTM1", hgtIsOneArcSec);
			EditorGUILayout.Space(12);
		}
		else
		{
			GUILayout.Label("Saving Settings", EditorStyles.boldLabel);
			trnPath = EditorGUILayout.TextField("Save Folder Path", trnPath);
			trnResolution = EditorGUILayout.IntField("trn Resolution", trnResolution);
			EditorGUILayout.Space(12);
		}

		//height map image
		if (heightMapTexture != null)
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(heightMapTexture, GUILayout.Width(512), GUILayout.Height(512));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Label($"{heightMapTexture.width} x {heightMapTexture.height}");
		}

		
		EditorGUILayout.EndScrollView();
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		if (MapGenerated)
		{
			if (GUILayout.Button("Clear Map", GUILayout.Height(32)))
			{
				heightMapTexture = null;
				heighMap = null;
				MapGenerated = false;
			}
			if (GUILayout.Button("Save", GUILayout.Height(32)))
			{
				Save();
			}
			if (GUILayout.Button("Save All", GUILayout.Height(32)))
			{

			}
		}
		else
		{
			if (GUILayout.Button("Convert", GUILayout.Height(32)))
			{
				heighMap = Convert();
				heightMapTexture = GenerateGrayscaleTexture(heighMap);
				MapGenerated = true;
			}
		}

		GUILayout.EndHorizontal();
	}

	private float[,] Convert()
	{
		int spacing = hgtIsOneArcSec ? 10 : 30;
		int resolution = hgtIsOneArcSec ? 3601 : 1201;
		int dataSize = resolution * resolution;
		int expectedBytes = dataSize * 2; // 2 bytes per Int16

		byte[] bytes = File.ReadAllBytes(hgtPath);

		if (bytes.Length != expectedBytes)
			throw new Exception($"Invalid HGT file size: expected {expectedBytes} bytes, got {bytes.Length}");

		float[,] elevationData = new float[resolution, resolution];

		for (int i = 0; i < dataSize; i++)
		{
			int byteIndex = i * 2;
			short height = (short)((bytes[byteIndex] << 8) | bytes[byteIndex + 1]); // big-endian
			float elevation = (height == -32768) ? float.NaN : height;

			int row = i / resolution;
			int col = i % resolution;
			elevationData[row, col] = elevation;
		}

		return elevationData;
	}

	private void Save()
	{


		for (int x = 0; x < trnResolution; x++)
		{
			for (int y = 0; y < trnResolution; y++)
			{

			}
		}
	}

	Texture2D GenerateGrayscaleTexture(float[,] data)
	{
		int width = data.GetLength(1);
		int height = data.GetLength(0);
		Texture2D tex = new Texture2D(width, height);
		tex.filterMode = FilterMode.Point;

		// Normalize for grayscale mapping
		float min = float.MaxValue;
		float max = float.MinValue;
		foreach (float f in data)
		{
			if (!float.IsNaN(f))
			{
				if (f < min) min = f;
				if (f > max) max = f;
			}
		}

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (x % 400 == 0 || y % 400 == 0)
				{
					tex.SetPixel(x, y, Color.green);
				}
				else
				{
					float val = data[y, x];
					float normalized = float.IsNaN(val) ? 0f : Mathf.InverseLerp(min, max, val);
					tex.SetPixel(x, y, new Color(normalized, normalized, normalized));
				}
			}
		}
		tex.Apply();
		return tex;
	}
}