using System.ComponentModel;

namespace WorldGenerator
{
	public sealed class GeneratorSettings
	{
		[Category("Generator Values")]
		public int Width = 512;
		[Category("Generator Values")]
		public int Height = 512;
		[Category("Generator Values")]
		public bool MultiThreadedGeneration = true;

		[Category("Height Map")]
		public int TerrainOctaves = 6;
		[Category("Height Map")]
		public double TerrainFrequency = 1.25;
		[Category("Height Map")]
		public float DeepWater = 0.2f;
		[Category("Height Map")]
		public float ShallowWater = 0.4f;
		[Category("Height Map")]
		public float Sand = 0.5f;
		[Category("Height Map")]
		public float Grass = 0.7f;
		[Category("Height Map")]
		public float Forest = 0.8f;
		[Category("Height Map")]
		public float Rock = 0.9f;

		[Category("Heat Map")]
		public int HeatOctaves = 4;
		[Category("Heat Map")]
		public double HeatFrequency = 3.0;
		[Category("Heat Map")]
		public float ColdestValue = 0.05f;
		[Category("Heat Map")]
		public float ColderValue = 0.18f;
		[Category("Heat Map")]
		public float ColdValue = 0.4f;
		[Category("Heat Map")]
		public float WarmValue = 0.6f;
		[Category("Heat Map")]
		public float WarmerValue = 0.8f;

		[Category("Moisture Map")] 
		public int MoistureOctaves = 4;
		[Category("Moisture Map")]
		public float MoistureFrequency = 3.0f;
		[Category("Moisture Map")]
		public float DryerValue = 0.27f;
		[Category("Moisture Map")]
		public float DryValue = 0.4f;
		[Category("Moisture Map")]
		public float WetValue = 0.6f;
		[Category("Moisture Map")]
		public float WetterValue = 0.8f;
		[Category("Moisture Map")]
		public float WettestValue = 0.9f;

		[Category("Rivers")]
		public int RiverCount = 40;
		[Category("Rivers")]
		public float MinRiverHeight = 0.6f;
		[Category("Rivers")]
		public int MaxRiverAttempts = 1000;
		[Category("Rivers")]
		public int MinRiverTurns = 18;
		[Category("Rivers")]
		public int MinRiverLength = 20;
		[Category("Rivers")]
		public int MaxRiverIntersections = 2;
	}
}
