using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace WorldGenerator
{
	public static class TextureGenerator
	{
		// Height Map Colors
		private static Color DeepColor = new Color(15 / 255f, 30 / 255f, 80 / 255f, 1);
		private static Color ShallowColor = new Color(15 / 255f, 40 / 255f, 90 / 255f, 1);
		private static Color RiverColor = new Color(30 / 255f, 120 / 255f, 200 / 255f, 1);
		private static Color SandColor = new Color(198 / 255f, 190 / 255f, 31 / 255f, 1);
		private static Color GrassColor = new Color(50 / 255f, 220 / 255f, 20 / 255f, 1);
		private static Color ForestColor = new Color(16 / 255f, 160 / 255f, 0, 1);
		private static Color RockColor = new Color(0.5f, 0.5f, 0.5f, 1);
		private static Color SnowColor = new Color(1, 1, 1, 1);

		private static Color IceWater = new Color(210 / 255f, 255 / 255f, 252 / 255f, 1);
		private static Color ColdWater = new Color(119 / 255f, 156 / 255f, 213 / 255f, 1);
		private static Color RiverWater = new Color(65 / 255f, 110 / 255f, 179 / 255f, 1);

		// Height Map Colors
		private static Color Coldest = new Color(0, 1, 1, 1);
		private static Color Colder = new Color(170 / 255f, 1, 1, 1);
		private static Color Cold = new Color(0, 229 / 255f, 133 / 255f, 1);
		private static Color Warm = new Color(1, 1, 100 / 255f, 1);
		private static Color Warmer = new Color(1, 100 / 255f, 0, 1);
		private static Color Warmest = new Color(241 / 255f, 12 / 255f, 0, 1);

		//Moisture map
		private static Color Dryest = new Color(255 / 255f, 139 / 255f, 17 / 255f, 1);
		private static Color Dryer = new Color(245 / 255f, 245 / 255f, 23 / 255f, 1);
		private static Color Dry = new Color(80 / 255f, 255 / 255f, 0 / 255f, 1);
		private static Color Wet = new Color(85 / 255f, 255 / 255f, 255 / 255f, 1);
		private static Color Wetter = new Color(20 / 255f, 70 / 255f, 255 / 255f, 1);
		private static Color Wettest = new Color(0 / 255f, 0 / 255f, 100 / 255f, 1);

		//biome map
		private static Color Ice = Color.White;
		private static Color Desert = new Color(238 / 255f, 218 / 255f, 130 / 255f, 1);
		private static Color Savanna = new Color(177 / 255f, 209 / 255f, 110 / 255f, 1);
		private static Color TropicalRainforest = new Color(66 / 255f, 123 / 255f, 25 / 255f, 1);
		private static Color Tundra = new Color(96 / 255f, 131 / 255f, 112 / 255f, 1);
		private static Color TemperateRainforest = new Color(29 / 255f, 73 / 255f, 40 / 255f, 1);
		private static Color Grassland = new Color(164 / 255f, 225 / 255f, 99 / 255f, 1);
		private static Color SeasonalForest = new Color(73 / 255f, 100 / 255f, 35 / 255f, 1);
		private static Color BorealForest = new Color(95 / 255f, 115 / 255f, 62 / 255f, 1);
		private static Color Woodland = new Color(139 / 255f, 175 / 255f, 90 / 255f, 1);

		public static Texture2D GetCloud1Texture(GraphicsDevice device, int width, int height, Tile[,] tiles)
		{
			var texture = new Texture2D(device, width, height);
			var pixels = new Color[width * height];

			for (var x = 0; x < width; x++)
			{
				for (var y = 0; y < height; y++)
				{
					if (tiles[x, y].Cloud1Value > 0.45f)
						pixels[x + y * width] = Color.Lerp(new Color(1f, 1f, 1f, 0), Color.White, tiles[x, y].Cloud1Value);
					else
						pixels[x + y * width] = new Color(0, 0, 0, 0);
				}
			}

			texture.SetData(pixels);
			return texture;
		}

		public static Texture2D GetCloud2Texture(GraphicsDevice device, int width, int height, Tile[,] tiles)
		{
			var texture = new Texture2D(device, width, height);
			var pixels = new Color[width * height];

			for (var x = 0; x < width; x++)
			{
				for (var y = 0; y < height; y++)
				{
					if (tiles[x, y].Cloud2Value > 0.5f)
						pixels[x + y * width] = Color.Lerp(new Color(1f, 1f, 1f, 0), Color.White, tiles[x, y].Cloud2Value);
					else
						pixels[x + y * width] = new Color(0, 0, 0, 0);
				}
			}

			texture.SetData(pixels);
			return texture;
		}

		public static Texture2D GetBiomePalette(GraphicsDevice device)
		{
			var texture = new Texture2D(device, 128, 128);
			var pixels = new Color[128 * 128];

			for (var x = 0; x < 128; x++)
			{
				for (var y = 0; y < 128; y++)
				{
					var color = new Color(0, 0, 0, 0);
					if (x < 10)
						color = Ice;
					else if (x < 20)
						color = Desert;
					else if (x < 30)
						color = Savanna;
					else if (x < 40)
						color = TropicalRainforest;
					else if (x < 50)
						color = Tundra;
					else if (x < 60)
						color = TemperateRainforest;
					else if (x < 70)
						color = Grassland;
					else if (x < 80)
						color = SeasonalForest;
					else if (x < 90)
						color = BorealForest;
					else if (x < 100)
						color = Woodland;

					pixels[x + y * 128] = color;
				}
			}

			texture.SetData(pixels);
			return texture;
		}

		public static Texture2D GetBumpMap(GraphicsDevice device, int width, int height, Tile[,] tiles)
		{
			var texture = new Texture2D(device, width, height);
			var pixels = new Color[width * height];

			for (var x = 0; x < width; x++)
			{
				for (var y = 0; y < height; y++)
				{
					var colorValue = 0.0f;
					switch (tiles[x, y].HeightType)
					{
						case HeightType.DeepWater:
						case HeightType.ShallowWater:
						case HeightType.River:
							break;
						case HeightType.Sand:
							colorValue = 0.3f;
							break;
						case HeightType.Grass:
							colorValue = 0.45f;
							break;
						case HeightType.Forest:
							colorValue = 0.6f;
							break;
						case HeightType.Rock:
							colorValue = 0.75f;
							break;
						case HeightType.Snow:
							colorValue = 1.0f;
							break;
					}

					var color = new Color(colorValue, colorValue, colorValue, 1f);

					if (!tiles[x, y].Collidable)
					{
						color = Color.Lerp(Color.White, Color.Black, tiles[x, y].HeightValue * 2);
					}

					pixels[x + y * width] = color;
				}
			}

			texture.SetData(pixels);
			return texture;
		}

		public static Texture2D GetHeightMapTexture(GraphicsDevice device, int width, int height, Tile[,] tiles)
		{
			var texture = new Texture2D(device, width, height);
			var pixels = new Color[width * height];

			for (var x = 0; x < width; x++)
			{
				for (var y = 0; y < height; y++)
				{
					var colorValue = 0.0f;
					switch (tiles[x, y].HeightType)
					{
						case HeightType.DeepWater:
						case HeightType.ShallowWater:
						case HeightType.River:
							break;
						case HeightType.Sand:
							colorValue = 0.3f;
							break;
						case HeightType.Grass:
							colorValue = 0.45f;
							break;
						case HeightType.Forest:
							colorValue = 0.6f;
							break;
						case HeightType.Rock:
							colorValue = 0.75f;
							break;
						case HeightType.Snow:
							colorValue = 1.0f;
							break;
					}

					pixels[x + y * width] = new Color(colorValue, colorValue, colorValue, 1f);
				}
			}

			texture.SetData(pixels);
			return texture;
		}

		public static Texture2D GetHeatMapTexture(GraphicsDevice device, int width, int height, Tile[,] tiles)
		{
			var texture = new Texture2D(device, width, height);
			var pixels = new Color[width * height];

			for (var x = 0; x < width; x++)
			{
				for (var y = 0; y < height; y++)
				{
					var color = Color.Black;
					switch (tiles[x, y].HeatType)
					{
						case HeatType.Coldest:
							color = Coldest;
							break;
						case HeatType.Colder:
							color = Colder;
							break;
						case HeatType.Cold:
							color = Cold;
							break;
						case HeatType.Warm:
							color = Warm;
							break;
						case HeatType.Warmer:
							color = Warmer;
							break;
						case HeatType.Warmest:
							color = Warmest;
							break;
					}

					//darken the color if a edge tile
					if ((int)tiles[x, y].HeightType > 2 && tiles[x, y].Bitmask != 15)
						color = Color.Lerp(color, Color.Black, 0.4f);

					pixels[x + y * width] = color;
				}
			}

			texture.SetData(pixels);
			return texture;
		}

		public static Texture2D GetMoistureMapTexture(GraphicsDevice device, int width, int height, Tile[,] tiles)
		{
			var texture = new Texture2D(device, width, height);
			var pixels = new Color[width * height];

			for (var x = 0; x < width; x++)
			{
				for (var y = 0; y < height; y++)
				{
					Tile t = tiles[x, y];

					Color color;
					if (t.MoistureType == MoistureType.Dryest)
						color = Dryest;
					else if (t.MoistureType == MoistureType.Dryer)
						color = Dryer;
					else if (t.MoistureType == MoistureType.Dry)
						color = Dry;
					else if (t.MoistureType == MoistureType.Wet)
						color = Wet;
					else if (t.MoistureType == MoistureType.Wetter)
						color = Wetter;
					else
						color = Wettest;

					pixels[x + y * width] = color;
				}
			}

			texture.SetData(pixels);
			return texture;
		}

		public static Texture2D GetBiomeMapTexture(GraphicsDevice device, int width, int height, Tile[,] tiles, float coldest, float colder, float cold)
		{
			var texture = new Texture2D(device, width, height);
			var pixels = new Color[width * height];

			for (var x = 0; x < width; x++)
			{
				for (var y = 0; y < height; y++)
				{
					BiomeType value = tiles[x, y].BiomeType;

					var color = Color.Black;
					switch (value)
					{
						case BiomeType.Ice:
							color = Ice;
							break;
						case BiomeType.BorealForest:
							color = BorealForest;
							break;
						case BiomeType.Desert:
							color = Desert;
							break;
						case BiomeType.Grassland:
							color = Grassland;
							break;
						case BiomeType.SeasonalForest:
							color = SeasonalForest;
							break;
						case BiomeType.Tundra:
							color = Tundra;
							break;
						case BiomeType.Savanna:
							color = Savanna;
							break;
						case BiomeType.TemperateRainforest:
							color = TemperateRainforest;
							break;
						case BiomeType.TropicalRainforest:
							color = TropicalRainforest;
							break;
						case BiomeType.Woodland:
							color = Woodland;
							break;
					}

					// Water tiles
					if (tiles[x, y].HeightType == HeightType.DeepWater)
					{
						color = DeepColor;
					}
					else if (tiles[x, y].HeightType == HeightType.ShallowWater)
					{
						color = ShallowColor;
					}

					// draw rivers
					if (tiles[x, y].HeightType == HeightType.River)
					{
						float heatValue = tiles[x, y].HeatValue;

						if (tiles[x, y].HeatType == HeatType.Coldest)
							color = Color.Lerp(IceWater, ColdWater, (heatValue) / (coldest));
						else if (tiles[x, y].HeatType == HeatType.Colder)
							color = Color.Lerp(ColdWater, RiverWater, (heatValue - coldest) / (colder - coldest));
						else if (tiles[x, y].HeatType == HeatType.Cold)
							color = Color.Lerp(RiverWater, ShallowColor, (heatValue - colder) / (cold - colder));
						else
							color = ShallowColor;
					}


					// add a outline
					if (tiles[x, y].HeightType >= HeightType.Shore && tiles[x, y].HeightType != HeightType.River)
					{
						if (tiles[x, y].BiomeBitmask != 15)
							color = Color.Lerp(color, Color.Black, 0.35f);
					}

					pixels[x + y * width] = color;
				}
			}

			texture.SetData(pixels);
			return texture;
		}
	}
}