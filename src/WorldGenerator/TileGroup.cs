using System.Collections.Generic;

namespace WorldGenerator
{
	public enum TileGroupType
	{
		Water,
		Land
	}

	public class TileGroup
	{

		public TileGroupType Type;
		public List<Tile> Tiles;

		public TileGroup()
		{
			Tiles = new List<Tile>();
		}
	}
}