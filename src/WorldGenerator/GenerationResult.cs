using System.Collections.Generic;

namespace WorldGenerator
{
	public class GenerationResult
	{
		public Tile[,] Tiles;

		public readonly List<TileGroup> Waters = new List<TileGroup>();
		public readonly List<TileGroup> Lands = new List<TileGroup>();

		public readonly List<River> Rivers = new List<River>();
		public readonly List<RiverGroup> RiverGroups = new List<RiverGroup>();
	}
}
