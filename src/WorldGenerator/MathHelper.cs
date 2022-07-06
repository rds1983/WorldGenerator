using System;

namespace WorldGenerator
{
	internal static class MathHelper
	{
		public const float Deg2Rad = MathF.PI / 180.0f;

		public static readonly Random Random = new();

		public static int Mod(int x, int m)
		{
			int r = x % m;
			return r < 0 ? r + m : r;
		}

		public static int RandomRange(int min, int max) => Random.Next(min, max);
	}
}