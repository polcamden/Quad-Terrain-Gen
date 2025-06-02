using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class HgtToTrn : EditorWindow
{
	private string hgtPath = "";
	private string trnPath = "Assets";

	private bool hgtIsOneArcSec = true;
	private int trnResolution = 4096;
	private Vector2Int positionOffset = Vector2Int.zero;
	private Vector2Int savePos = Vector2Int.zero;

	private float[,] heightMap;
	Texture2D heightMapTexture;
	private bool MapGenerated;

	private Vector2 scrollPos;

	public int hgtSpacing
	{
		get
		{
			return hgtIsOneArcSec ? 30 : 92;
		}
	}

	[MenuItem("Tools/Quad Terrain Gen/Hgt To Terrain")]
	public static void ShowExample()
	{
		HgtToTrn wnd = GetWindow<HgtToTrn>();

		Vector2 size = new Vector2(512 + 24, 720);
		wnd.minSize = size;
		wnd.maxSize = size;

		wnd.position = new Rect(100, 100, size.x, size.y);
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
			trnResolution = EditorGUILayout.IntField("Resolution", trnResolution);
			positionOffset = EditorGUILayout.Vector2IntField("Offset", positionOffset);
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
			if (GUILayout.Button("Clear Map", GUILayout.Height(64)))
			{
				heightMapTexture = null;
				heightMap = null;
				MapGenerated = false;
			}

			GUILayout.BeginVertical();
			savePos = EditorGUILayout.Vector2IntField("Save Position", savePos, GUILayout.Height(38));
			if (GUILayout.Button("Save", GUILayout.Height(24)))
			{
				Save(trnResolution, savePos);
			}
			GUILayout.EndVertical();
		}
		else
		{
			if (GUILayout.Button("Convert", GUILayout.Height(32)))
			{
				heightMap = Convert();
				heightMapTexture = GenerateGrayscaleTexture(heightMap);
				MapGenerated = true;
			}
		}

		GUILayout.EndHorizontal();
	}

	private float[,] Convert()
	{
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

	private void Save(int resolution, Vector2Int position)
	{
		int spacing = hgtSpacing;

		Vector2Int HgtPos = position * resolution / spacing;

		float[,] chunkMap = new float[resolution, resolution];

		for (int x = 0; x < resolution; x++)
		{
			for (int y = 0; y < resolution; y++)
			{
				//Todo: smoothing

				int hgtX = x / spacing + HgtPos.x;
				int hgtY = y / spacing + HgtPos.y;

				chunkMap[x, y] = heightMap[hgtX, hgtY];
			}
		}

		Vector2Int finalPosition = position + positionOffset;
		string filePath = $"{trnPath}/{finalPosition}.trn";
		using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
		{
			for (int x = 0; x < resolution; x++)
			{
				for (int y = 0; y < resolution; y++)
				{
					writer.Write(chunkMap[x, y]);
				}
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