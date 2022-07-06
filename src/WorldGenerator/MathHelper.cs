using System;

namespace WorldGenerator
{
	public static class MathHelper
	{
		public const float Deg2Rad = MathF.PI / 180.0f;

		public static readonly Random Random = new Random();

		public static double Clamp(double v, double l, double h)
		{
			if (v < l) v = l;
			if (v > h) v = h;
			return v;
		}

		public static double Lerp(double t, double a, double b)
		{
			return a + t * (b - a);
		}

		public static double QuinticBlend(double t)
		{
			return t * t * t * (t * (t * 6 - 15) + 10);
		}

		public static double Bias(double b, double t)
		{
			return Math.Pow(t, Math.Log(b) / Math.Log(0.5));
		}

		public static double Gain(double g, double t)
		{
			if (t < 0.5)
			{
				return Bias(1.0 - g, 2.0 * t) / 2.0;
			}
			else
			{
				return 1.0 - Bias(1.0 - g, 2.0 - 2.0 * t) / 2.0;
			}
		}

		public static int Mod(int x, int m)
		{
			int r = x % m;
			return r < 0 ? r + m : r;
		}

		public static int RandomRange(int min, int max) => (int)Random.NextInt64(min, max + 1);
	}
}