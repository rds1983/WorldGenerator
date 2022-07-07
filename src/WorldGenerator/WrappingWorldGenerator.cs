using System;
using System.Threading;
using TinkerWorX.AccidentalNoiseLibrary;

namespace WorldGenerator
{
	public class WrappingWorldGenerator : Generator
	{
		private readonly struct TaskParams
		{
			public readonly int x;
			public readonly int y;

			public TaskParams(int x, int y)
			{
				this.x = x;
				this.y = y;
			}
		}

		protected ImplicitFractal HeightMap;
		protected ImplicitCombiner HeatMap;
		protected ImplicitFractal MoistureMap;
		private int tasks;

		public WrappingWorldGenerator(GeneratorSettings settings, Action<string> infoHandler = null) : base(settings, infoHandler)
		{
		}

		protected override void Initialize()
		{
			// HeightMap
			HeightMap = new ImplicitFractal(FractalType.Multi,
											 BasisType.Simplex,
											 InterpolationType.Quintic)
			{
				Octaves = settings.TerrainOctaves,
				Frequency = settings.TerrainFrequency,
				Seed = Seed
			};

			// Heat Map
			ImplicitGradient gradient = new ImplicitGradient(1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1);
			ImplicitFractal heatFractal = new ImplicitFractal(FractalType.Multi,
															  BasisType.Simplex,
															  InterpolationType.Quintic)
			{
				Octaves = settings.HeatOctaves,
				Frequency = settings.HeatFrequency,
				Seed = Seed
			};

			HeatMap = new ImplicitCombiner(CombinerType.Multiply);
			HeatMap.AddSource(gradient);
			HeatMap.AddSource(heatFractal);

			// Moisture Map
			MoistureMap = new ImplicitFractal(FractalType.Multi,
											   BasisType.Simplex,
											   InterpolationType.Quintic)
			{
				Octaves = settings.MoistureOctaves,
				Frequency = settings.MoistureFrequency,
				Seed = Seed
			};
		}

		protected override void GetData()
		{
			HeightData = new MapData(settings.Width, settings.Height);
			HeatData = new MapData(settings.Width, settings.Height);
			MoistureData = new MapData(settings.Width, settings.Height);

			tasks = settings.Width * settings.Height;
			// loop through each x,y point - get height value
			for (var x = 0; x < settings.Width; x++)
			{
				for (var y = 0; y < settings.Height; y++)
				{
					var p = new TaskParams(x, y);

					if (settings.MultiThreadedGeneration)
					{
						ThreadPool.QueueUserWorkItem(ProcessPoint, p);
					}
					else
					{
						ProcessPoint(p);
					}
				}
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

			// WRAP ON BOTH AXIS
			// Noise range
			float x1 = 0, x2 = 2;
			float y1 = 0, y2 = 2;
			float dx = x2 - x1;
			float dy = y2 - y1;

			// Sample noise at smaller intervals
			float s = x / (float)settings.Width;
			float t = y / (float)settings.Height;

			// Calculate our 4D coordinates
			float nx = x1 + MathF.Cos(s * 2 * MathF.PI) * dx / (2 * MathF.PI);
			float ny = y1 + MathF.Cos(t * 2 * MathF.PI) * dy / (2 * MathF.PI);
			float nz = x1 + MathF.Sin(s * 2 * MathF.PI) * dx / (2 * MathF.PI);
			float nw = y1 + MathF.Sin(t * 2 * MathF.PI) * dy / (2 * MathF.PI);

			float heightValue = (float)HeightMap.Get(nx, ny, nz, nw);
			float heatValue = (float)HeatMap.Get(nx, ny, nz, nw);
			float moistureValue = (float)MoistureMap.Get(nx, ny, nz, nw);

			// keep track of the max and min values found
			if (heightValue > HeightData.Max) HeightData.Max = heightValue;
			if (heightValue < HeightData.Min) HeightData.Min = heightValue;

			if (heatValue > HeatData.Max) HeatData.Max = heatValue;
			if (heatValue < HeatData.Min) HeatData.Min = heatValue;

			if (moistureValue > MoistureData.Max) MoistureData.Max = moistureValue;
			if (moistureValue < MoistureData.Min) MoistureData.Min = moistureValue;

			HeightData.Data[x, y] = heightValue;
			HeatData.Data[x, y] = heatValue;
			MoistureData.Data[x, y] = moistureValue;

			Interlocked.Decrement(ref tasks);
			LogInfo("Points left: {0}", tasks);
		}

		protected override Tile GetTop(Tile t)
		{
			return GenerationResult.Tiles[t.X, MathHelper.Mod(t.Y - 1, settings.Height)];
		}
		protected override Tile GetBottom(Tile t)
		{
			return GenerationResult.Tiles[t.X, MathHelper.Mod(t.Y + 1, settings.Height)];
		}
		protected override Tile GetLeft(Tile t)
		{
			return GenerationResult.Tiles[MathHelper.Mod(t.X - 1, settings.Width), t.Y];
		}
		protected override Tile GetRight(Tile t)
		{
			return GenerationResult.Tiles[MathHelper.Mod(t.X + 1, settings.Width), t.Y];
		}
	}
}