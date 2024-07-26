﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SpriteTools;

[GameResource("2D Tileset", "tileset", "A 2D Tileset atlas", Icon = "calendar_view_month", IconBgColor = "#fab006")]
public class TilesetResource : GameResource
{
	[Property, ImageAssetPath, Title("Tileset Image"), Group("Tileset Setup")]
	public string FilePath { get; set; }

	[Property, Group("Tileset Setup")]
	public int TileSize { get; set; } = 32;

	[Property, Group("Tileset Setup")]
	public Vector2Int TileSeparation { get; set; } = 0;

	public int CurrentTileSize { get; set; } = 32;

	[Property, Group("Tiles")]
	public List<Tile> Tiles { get; set; } = new();

	public void InitFromAtlas(string filePath, int tileSize)
	{
		var texture = Texture.Load(FileSystem.Mounted, filePath);
		FilePath = filePath;
		TileSize = tileSize;
		CurrentTileSize = TileSize;
	}

	public Vector2 GetTiling()
	{
		return new Vector2(1, 1);
	}

	public Vector2 GetOffset(int index)
	{
		return new Vector2(0, 0);
	}

	public string Serialize()
	{
		var obj = new JsonObject()
		{
			["FilePath"] = FilePath,
			["TileSize"] = TileSize,
			["CurrentTileSize"] = CurrentTileSize,
			["TileSeparation"] = TileSeparation.ToString(),
			["Tiles"] = Json.Serialize(Tiles)
		};
		return obj.ToJsonString();
	}

	public void Deserialize(string json)
	{
		var obj = JsonNode.Parse(json);
		FilePath = obj["FilePath"]?.GetValue<string>() ?? "";
		TileSize = obj["TileSize"]?.GetValue<int>() ?? 32;
		CurrentTileSize = obj["CurrentTileSize"]?.GetValue<int>() ?? 32;
		TileSeparation = Vector2Int.Parse(obj["TileSeparation"]?.GetValue<string>() ?? "0,0");
		Tiles = Json.Deserialize<List<Tile>>(obj["Tiles"]?.GetValue<string>() ?? "[]");
	}

	protected override void PostLoad()
	{
		base.PostLoad();

		InternalUpdateTiles();
	}

	protected override void PostReload()
	{
		base.PostReload();

		InternalUpdateTiles();
	}

	public void InternalUpdateTiles()
	{
		foreach (var tile in Tiles)
		{
			tile.Tileset = this;
		}
	}

	public class Tile
	{
		public string Name { get; set; } = "";
		public Rect SheetRect { get; set; }

		[JsonIgnore]
		public TilesetResource Tileset;

		public Tile(Rect sheetRect)
		{
			SheetRect = sheetRect;
		}

		public Tile Copy()
		{
			var copy = new Tile(SheetRect)
			{
				Name = Name
			};
			return copy;
		}
	}
}
