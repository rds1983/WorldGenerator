using System;
using System.Collections.Generic;

namespace WorldGenerator
{
	public enum MapType
	{
		Wrapping,
		Spherical
	}

	public class GenerationResult
	{
		public MapType MapType;

		public Tile[,] Tiles;

		public int Width => Tiles.GetLength(0);
		public int Height => Tiles.GetLength(1);

		public readonly List<TileGroup> Waters = new List<TileGroup>();
		public readonly List<TileGroup> Lands = new List<TileGroup>();

		public readonly List<River> Rivers = new List<River>();
		public readonly List<RiverGroup> RiverGroups = new List<RiverGroup>();

		private Tile GetTop(Tile t)
		{
			if (MapType == MapType.Wrapping)
			{
				return Tiles[t.X, MathHelper.Mod(t.Y - 1, Height)];
			}

			if (t.Y - 1 > 0)
				return Tiles[t.X, t.Y - 1];
			else
				return null;
		}
		private Tile GetBottom(Tile t)
		{
			if (MapType == MapType.Wrapping)
			{
				return Tiles[t.X, MathHelper.Mod(t.Y + 1, Height)];
			}

			if (t.Y + 1 < Height)
				return Tiles[t.X, t.Y + 1];
			else
				return null;
		}
		private Tile GetLeft(Tile t)
		{
			return Tiles[MathHelper.Mod(t.X - 1, Width), t.Y];

		}
		private Tile GetRight(Tile t)
		{
			return Tiles[MathHelper.Mod(t.X + 1, Width), t.Y];
		}

		public void ProcessTiles(Action<int, int, Tile> processor)
		{
			for (var x = 0; x < Width; ++x)
			{
				for (var y = 0; y < Height; ++y)
				{
					var tile = Tiles[x, y];
					processor(x, y, tile);
				}
			}
		}

		public void UpdateNeighbors()
		{
			ProcessTiles((x, y, tile) =>
			{
				tile.Left = GetLeft(tile);
				tile.Right = GetRight(tile);
				tile.Top = GetTop(tile);
				tile.Bottom = GetBottom(tile);
			});
		}

		public void UpdateBitmask()
		{
			ProcessTiles((x, y, tile) =>
			{
				tile.UpdateBitmask();
			});
		}

		public void UpdateBiomeMask()
		{
			ProcessTiles((x, y, tile) =>
			{
				tile.UpdateBiomeBitmask();
			});
		}
	}
}
