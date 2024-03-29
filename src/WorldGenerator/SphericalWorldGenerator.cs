﻿using System;
using System.Threading;
using System.Threading.Tasks;
using TinkerWorX.AccidentalNoiseLibrary;

namespace WorldGenerator
{
	public class SphericalWorldGenerator : Generator
	{
		protected ImplicitFractal HeightMap;
		protected ImplicitFractal HeatMap;
		protected ImplicitFractal MoistureMap;
		protected ImplicitFractal Cloud1Map;
		protected ImplicitFractal Cloud2Map;

		protected override MapType MapType => MapType.Spherical;

		public SphericalWorldGenerator(GeneratorSettings settings, ILog logHandler = null) : base(settings, logHandler)
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
			float latExtent = northLatBound - southLatBound;

			float yDelta = latExtent / settings.Height;

			tasksLeft = settings.Width;

			// Loop through each tile using its lat/long coordinates
			Parallel.For(0, settings.Width, x =>
			{
				ProcessColumn(x, southLatBound + x * yDelta);
			});

			LogProgress(null);
		}

		private void ProcessColumn(int x, float curLat)
		{
			float westLonBound = -90;
			float eastLonBound = 90;

			float lonExtent = eastLonBound - westLonBound;
			float xDelta = lonExtent / settings.Width;

			var curLon = westLonBound;
			for (var y = 0; y < settings.Height; y++)
			{
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

				curLon += xDelta;

			}

			Interlocked.Decrement(ref tasksLeft);
			LogProgress((settings.Width - tasksLeft) / (float)settings.Width);
		}

		// Convert Lat/Long coordinates to x/y/z for spherical mapping
		private void LatLonToXYZ(float lat, float lon, ref float x, ref float y, ref float z)
		{
			float r = MathF.Cos(MathHelper.Deg2Rad * lon);
			x = r * MathF.Cos(MathHelper.Deg2Rad * lat);
			y = MathF.Sin(MathHelper.Deg2Rad * lon);
			z = r * MathF.Sin(MathHelper.Deg2Rad * lat);
		}
	}
}