using System;
using System.Threading;
using TinkerWorX.AccidentalNoiseLibrary;

namespace WorldGenerator
{
	public class SphericalWorldGenerator : Generator
	{
		private readonly struct TaskParams
		{
			public readonly int x;
			public readonly int y;
			public readonly float curLon;
			public readonly float curLat;

			public TaskParams(int x, int y, float curLon, float curLat)
			{
				this.x = x;
				this.y = y;
				this.curLon = curLon;
				this.curLat = curLat;
			}
		}

		protected ImplicitFractal HeightMap;
		protected ImplicitFractal HeatMap;
		protected ImplicitFractal MoistureMap;
		protected ImplicitFractal Cloud1Map;
		protected ImplicitFractal Cloud2Map;
		private int tasks;

		public SphericalWorldGenerator(GeneratorSettings settings, Action<string> infoHandler = null) : base(settings, infoHandler)
		{
		}

		protected override void Initialize()
		{
			HeightMap = new ImplicitFractal(FractalType.Multi,
											 BasisType.Simplex,
											 InterpolationType.Quintic)
			{
				Octaves = settings.TerrainOctaves,
				Frequency = settings.TerrainFrequency,
				Seed = Seed
			};

			HeatMap = new ImplicitFractal(FractalType.Multi,
										  BasisType.Simplex,
										  InterpolationType.Quintic)
			{
				Octaves = settings.HeatOctaves,
				Frequency = settings.HeatFrequency,
				Seed = Seed
			};

			MoistureMap = new ImplicitFractal(FractalType.Multi,
											   BasisType.Simplex,
											   InterpolationType.Quintic)
			{
				Octaves = settings.MoistureOctaves,
				Frequency = settings.MoistureFrequency,
				Seed = Seed
			};

			Cloud1Map = new ImplicitFractal(FractalType.Billow,
								BasisType.Simplex,
								InterpolationType.Quintic)
			{
				Octaves = 4,
				Frequency = 1.55f,
				Seed = Seed
			};

			Cloud2Map = new ImplicitFractal(FractalType.Billow,
											BasisType.Simplex,
											InterpolationType.Quintic)
			{
				Octaves = 5,
				Frequency = 1.75f,
				Seed = Seed
			};
		}

		protected override void GetData()
		{
			HeightData = new MapData(settings.Width, settings.Height);
			HeatData = new MapData(settings.Width, settings.Height);
			MoistureData = new MapData(settings.Width, settings.Height);
			Clouds1 = new MapData(settings.Width, settings.Height);
			Clouds2 = new MapData(settings.Width, settings.Height);

			// Define our map area in latitude/longitude
			float southLatBound = -180;
			float northLatBound = 180;
			float westLonBound = -90;
			float eastLonBound = 90;

			float lonExtent = eastLonBound - westLonBound;
			float latExtent = northLatBound - southLatBound;

			float xDelta = lonExtent / settings.Width;
			float yDelta = latExtent / settings.Height;

			float curLat = southLatBound;

			var totalSize = settings.Width * settings.Height;

			// Loop through each tile using its lat/long coordinates
			for (var x = 0; x < settings.Width; x++)
			{
				var curLon = westLonBound;

				for (var y = 0; y < settings.Height; y++)
				{
					var p = new TaskParams(x, y, curLon, curLat);

					if (settings.MultiThreadedGeneration)
					{
						ThreadPool.QueueUserWorkItem(ProcessPoint, p);
						Interlocked.Increment(ref tasks);
					} else {
						ProcessPoint(p);
					}

					curLon += xDelta;
				}
				curLat += yDelta;
			}

			if (settings.MultiThreadedGeneration)
			{
				while (true)
				{
					Thread.Sleep(1000);
					if (tasks <= 0)
					{
						break;
					}
				}
			}
		}

		private void ProcessPoint(object state)
		{
			var p = (TaskParams)state;
			var x = p.x;
			var y = p.y;
			var curLon = p.curLon;
			var curLat = p.curLat;

			float x1 = 0, y1 = 0, z1 = 0;

			// Convert this lat/lon to x/y/z
			LatLonToXYZ(curLat, curLon, ref x1, ref y1, ref z1);

			// Heat data
			float sphereValue = (float)HeatMap.Get(x1, y1, z1);
			if (sphereValue > HeatData.Max)
				HeatData.Max = sphereValue;
			if (sphereValue < HeatData.Min)
				HeatData.Min = sphereValue;
			HeatData.Data[x, y] = sphereValue;

			float coldness = MathF.Abs(curLon) / 90f;
			float heat = 1 - MathF.Abs(curLon) / 90f;
			HeatData.Data[x, y] += heat;
			HeatData.Data[x, y] -= coldness;

			// Height Data
			float heightValue = (float)HeightMap.Get(x1, y1, z1);
			if (heightValue > HeightData.Max)
				HeightData.Max = heightValue;
			if (heightValue < HeightData.Min)
				HeightData.Min = heightValue;
			HeightData.Data[x, y] = heightValue;

			// Moisture Data
			float moistureValue = (float)MoistureMap.Get(x1, y1, z1);
			if (moistureValue > MoistureData.Max)
				MoistureData.Max = moistureValue;
			if (moistureValue < MoistureData.Min)
				MoistureData.Min = moistureValue;
			MoistureData.Data[x, y] = moistureValue;

			// Cloud Data
			Clouds1.Data[x, y] = (float)Cloud1Map.Get(x1, y1, z1);
			if (Clouds1.Data[x, y] > Clouds1.Max)
				Clouds1.Max = Clouds1.Data[x, y];
			if (Clouds1.Data[x, y] < Clouds1.Min)
				Clouds1.Min = Clouds1.Data[x, y];

			Clouds2.Data[x, y] = (float)Cloud2Map.Get(x1, y1, z1);
			if (Clouds2.Data[x, y] > Clouds2.Max)
				Clouds2.Max = Clouds2.Data[x, y];
			if (Clouds2.Data[x, y] < Clouds2.Min)
				Clouds2.Min = Clouds2.Data[x, y];

			if (settings.MultiThreadedGeneration)
			{
				Interlocked.Decrement(ref tasks);
				LogInfo("Points left: {0}", tasks);
			} else {
				var pointsLeft = settings.Width * settings.Height - (x * settings.Height + y);
				LogInfo("Points left: {0}", pointsLeft);
			}
		}

		// Convert Lat/Long coordinates to x/y/z for spherical mapping
		private void LatLonToXYZ(float lat, float lon, ref float x, ref float y, ref float z)
		{
			float r = MathF.Cos(MathHelper.Deg2Rad * lon);
			x = r * MathF.Cos(MathHelper.Deg2Rad * lat);
			y = MathF.Sin(MathHelper.Deg2Rad * lon);
			z = r * MathF.Sin(MathHelper.Deg2Rad * lat);
		}

		protected override Tile GetTop(Tile t)
		{
			if (t.Y - 1 > 0)
				return Tiles[t.X, t.Y - 1];
			else
				return null;
		}
		protected override Tile GetBottom(Tile t)
		{
			if (t.Y + 1 < settings.Height)
				return Tiles[t.X, t.Y + 1];
			else
				return null;
		}
		protected override Tile GetLeft(Tile t)
		{
			return Tiles[MathHelper.Mod(t.X - 1, settings.Width), t.Y];
		}
		protected override Tile GetRight(Tile t)
		{
			return Tiles[MathHelper.Mod(t.X + 1, settings.Width), t.Y];
		}

	}
}