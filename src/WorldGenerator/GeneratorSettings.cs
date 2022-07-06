namespace WorldGenerator
{
	public sealed class GeneratorSettings
	{
		public int Width = 512;
		public int Height = 512;

		public int TerrainOctaves = 6;
		public double TerrainFrequency = 1.25;
		public float DeepWater = 0.2f;
		public float ShallowWater = 0.4f;
		public float Sand = 0.5f;
		public float Grass = 0.7f;
		public float Forest = 0.8f;
		public float Rock = 0.9f;

		public int HeatOctaves = 4;
		public double HeatFrequency = 3.0;
		public float ColdestValue = 0.05f;
		public float ColderValue = 0.18f;
		public float ColdValue = 0.4f;
		public float WarmValue = 0.6f;
		public float WarmerValue = 0.8f;

		public int MoistureOctaves = 4;
		public double MoistureFrequency = 3.0;
		public float DryerValue = 0.27f;
		public float DryValue = 0.4f;
		public float WetValue = 0.6f;
		public float WetterValue = 0.8f;
		public float WettestValue = 0.9f;

		public int RiverCount = 40;
		public float MinRiverHeight = 0.6f;
		public int MaxRiverAttempts = 1000;
		public int MinRiverTurns = 18;
		public int MinRiverLength = 20;
		public int MaxRiverIntersections = 2;
	}
}
