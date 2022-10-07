using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace WorldGenerator
{
	public abstract class Generator
	{
		private readonly struct TileDirection
		{
			public readonly Tile Tile;
			public readonly Direction Direction;

			public TileDirection(Tile tile, Direction direction)
			{
				Tile = tile;
				Direction = direction;
			}
		}

		protected int Seed;
		protected readonly GeneratorSettings settings;
		private ILog logHandler;
		protected int tasksLeft;

		protected MapData HeightData;
		protected MapData HeatData;
		protected MapData MoistureData;
		protected MapData Clouds1;
		protected MapData Clouds2;

		public GenerationResult GenerationResult;

		protected abstract MapType MapType { get; }


		public Generator(GeneratorSettings settings, ILog logHandler)
		{
			this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
			this.logHandler = logHandler;
		}

		public void LogInfo(string message, params object[] args)
		{
			if (logHandler == null)
			{
				return;
			}

			logHandler.Log(Utils.FormatMessage(message, args));
		}

		public void LogProgress(float? progress)
		{
			if (logHandler == null)
			{
				return;
			}

			logHandler.SetProgress(progress);
		}

		public void Go()
		{
			Instantiate();
			Generate();
		}

		protected abstract void Initialize();
		protected abstract void GetData();

		protected virtual void Instantiate()
		{
			Seed = MathHelper.RandomRange(0, int.MaxValue);
			GenerationResult = new GenerationResult
			{
				MapType = MapType
			};

			Initialize();
		}

		protected virtual void Generate()
		{
			LogInfo("GetData");
			GetData();

			LogInfo("LoadTiles");
			LoadTiles();

			LogInfo("UpdateNeighbors");
			UpdateNeighbors();

			LogInfo("GenerateRivers");
			GenerateRivers();

			LogInfo("BuildGenerationResult.RiverGroups");
			BuildRiverGroups();

			LogInfo("DigGenerationResult.RiverGroups");
			DigRiverGroups();

			LogInfo("AdjustMoistureMap");
			AdjustMoistureMap();

			LogInfo("UpdateBitmasks");
			UpdateBitmasks();

			LogInfo("FloodFill");
			FloodFill();

			LogInfo("UpdateBiomeBitmask");
			UpdateBiomeBitmask();
		}

		private void UpdateBiomeBitmask()
		{
			GenerationResult.UpdateBiomeMask();
		}

		private void AddMoisture(Tile t, int radius)
		{
			Vector2 center = new Vector2(t.X, t.Y);
			int curr = radius;

			while (curr > 0)
			{

				int x1 = MathHelper.Mod(t.X - curr, settings.Width);
				int x2 = MathHelper.Mod(t.X + curr, settings.Width);
				int y = t.Y;

				AddMoisture(GenerationResult.Tiles[x1, y], 0.025f / (center - new Vector2(x1, y)).Length());

				for (int i = 0; i < curr; i++)
				{
					AddMoisture(GenerationResult.Tiles[x1, MathHelper.Mod(y + i + 1, settings.Height)], 0.025f / (center - new Vector2(x1, MathHelper.Mod(y + i + 1, settings.Height))).Length());
					AddMoisture(GenerationResult.Tiles[x1, MathHelper.Mod(y - (i + 1), settings.Height)], 0.025f / (center - new Vector2(x1, MathHelper.Mod(y - (i + 1), settings.Height))).Length());

					AddMoisture(GenerationResult.Tiles[x2, MathHelper.Mod(y + i + 1, settings.Height)], 0.025f / (center - new Vector2(x2, MathHelper.Mod(y + i + 1, settings.Height))).Length());
					AddMoisture(GenerationResult.Tiles[x2, MathHelper.Mod(y - (i + 1), settings.Height)], 0.025f / (center - new Vector2(x2, MathHelper.Mod(y - (i + 1), settings.Height))).Length());
				}
				curr--;
			}
		}

		private void AddMoisture(Tile t, float amount)
		{
			MoistureData.Data[t.X, t.Y] += amount;
			t.MoistureValue += amount;
			if (t.MoistureValue > 1)
				t.MoistureValue = 1;

			//set moisture type
			if (t.MoistureValue < settings.DryerValue) t.MoistureType = MoistureType.Dryest;
			else if (t.MoistureValue < settings.DryValue) t.MoistureType = MoistureType.Dryer;
			else if (t.MoistureValue < settings.WetValue) t.MoistureType = MoistureType.Dry;
			else if (t.MoistureValue < settings.WetterValue) t.MoistureType = MoistureType.Wet;
			else if (t.MoistureValue < settings.WettestValue) t.MoistureType = MoistureType.Wetter;
			else t.MoistureType = MoistureType.Wettest;
		}

		private void AdjustMoistureMap()
		{
			tasksLeft = settings.Width;

			Parallel.For(0, settings.Width, x =>
			{
				for (var y = 0; y < settings.Height; y++)
				{
					Tile t = GenerationResult.Tiles[x, y];

					if (t.HeightType == HeightType.River)
					{
						AddMoisture(t, 60);
					}
				}

				Interlocked.Decrement(ref tasksLeft);
				LogProgress((settings.Width - tasksLeft) / (float)settings.Width);
			});

			LogProgress(null);
		}

		private void DigRiverGroups()
		{
			for (int i = 0; i < GenerationResult.RiverGroups.Count; i++)
			{

				RiverGroup group = GenerationResult.RiverGroups[i];
				River longest = null;

				//Find longest river in this group
				for (int j = 0; j < group.Rivers.Count; j++)
				{
					River river = group.Rivers[j];
					if (longest == null)
						longest = river;
					else if (longest.Tiles.Count < river.Tiles.Count)
						longest = river;
				}

				if (longest != null)
				{
					//Dig out longest path first
					DigRiver(longest);

					for (int j = 0; j < group.Rivers.Count; j++)
					{
						River river = group.Rivers[j];
						if (river != longest)
						{
							DigRiver(river, longest);
						}
					}
				}
			}
		}

		private void BuildRiverGroups()
		{
			//loop each tile, checking if it belongs to multiple rivers
			for (var x = 0; x < settings.Width; x++)
			{
				for (var y = 0; y < settings.Height; y++)
				{
					Tile t = GenerationResult.Tiles[x, y];

					if (t.Rivers.Count > 1)
					{
						// multiple rivers == intersection
						RiverGroup group = null;

						// Does a rivergroup already exist for this group?
						for (int n = 0; n < t.Rivers.Count; n++)
						{
							River tileriver = t.Rivers[n];
							for (int i = 0; i < GenerationResult.RiverGroups.Count; i++)
							{
								for (int j = 0; j < GenerationResult.RiverGroups[i].Rivers.Count; j++)
								{
									River river = GenerationResult.RiverGroups[i].Rivers[j];
									if (river.ID == tileriver.ID)
									{
										group = GenerationResult.RiverGroups[i];
									}
									if (group != null) break;
								}
								if (group != null) break;
							}
							if (group != null) break;
						}

						// existing group found -- add to it
						if (group != null)
						{
							for (int n = 0; n < t.Rivers.Count; n++)
							{
								if (!group.Rivers.Contains(t.Rivers[n]))
									group.Rivers.Add(t.Rivers[n]);
							}
						}
						else   //No existing group found - create a new one
						{
							group = new RiverGroup();
							for (int n = 0; n < t.Rivers.Count; n++)
							{
								group.Rivers.Add(t.Rivers[n]);
							}
							GenerationResult.RiverGroups.Add(group);
						}
					}
				}
			}
		}

		public float GetHeightValue(Tile tile)
		{
			if (tile == null)
				return int.MaxValue;
			else
				return tile.HeightValue;
		}

		private void GenerateRivers()
		{
			int attempts = 0;
			int rivercount = settings.RiverCount;

			// Generate some rivers
			while (rivercount > 0 && attempts < settings.MaxRiverAttempts)
			{

				// Get a random tile
				int x = MathHelper.RandomRange(0, settings.Width);
				int y = MathHelper.RandomRange(0, settings.Height);
				Tile tile = GenerationResult.Tiles[x, y];

				// validate the tile
				if (!tile.Collidable) continue;
				if (tile.Rivers.Count > 0) continue;

				if (tile.HeightValue > settings.MinRiverHeight)
				{
					// Tile is good to start river from
					River river = new River(rivercount);

					// Figure out the direction this river will try to flow
					river.CurrentDirection = tile.GetLowestNeighbor(this);

					// Find a path to water
					FindPathToWater(new TileDirection(tile, river.CurrentDirection), ref river);

					// Validate the generated river 
					if (river.TurnCount < settings.MinRiverTurns || river.Tiles.Count < settings.MinRiverLength || river.Intersections > settings.MaxRiverIntersections)
					{
						//Validation failed - remove this river
						for (int i = 0; i < river.Tiles.Count; i++)
						{
							Tile t = river.Tiles[i];
							t.Rivers.Remove(river);
						}
					}
					else if (river.Tiles.Count >= settings.MinRiverLength)
					{
						//Validation passed - Add river to list
						GenerationResult.Rivers.Add(river);
						tile.Rivers.Add(river);
						rivercount--;
					}
				}
				attempts++;
			}
		}

		// Dig river based on a parent river vein
		private void DigRiver(River river, River parent)
		{
			int intersectionID = 0;
			int intersectionSize = 0;

			// determine point of intersection
			for (int i = 0; i < river.Tiles.Count; i++)
			{
				Tile t1 = river.Tiles[i];
				for (int j = 0; j < parent.Tiles.Count; j++)
				{
					Tile t2 = parent.Tiles[j];
					if (t1 == t2)
					{
						intersectionID = i;
						intersectionSize = t2.RiverSize;
					}
				}
			}

			int counter = 0;
			int intersectionCount = river.Tiles.Count - intersectionID;
			int size = MathHelper.RandomRange(intersectionSize, 5);
			river.Length = river.Tiles.Count;

			// randomize size change
			int two = river.Length / 2;
			int three = two / 2;
			int four = three / 2;
			int five = four / 2;

			int twomin = two / 3;
			int threemin = three / 3;
			int fourmin = four / 3;
			int fivemin = five / 3;

			// randomize length of each size
			int count1 = MathHelper.RandomRange(fivemin, five);
			if (size < 4)
			{
				count1 = 0;
			}
			int count2 = count1 + MathHelper.RandomRange(fourmin, four);
			if (size < 3)
			{
				count2 = 0;
				count1 = 0;
			}
			int count3 = count2 + MathHelper.RandomRange(threemin, three);
			if (size < 2)
			{
				count3 = 0;
				count2 = 0;
				count1 = 0;
			}
			int count4 = count3 + MathHelper.RandomRange(twomin, two);

			// Make sure we are not digging past the river path
			if (count4 > river.Length)
			{
				int extra = count4 - river.Length;
				while (extra > 0)
				{
					if (count1 > 0) { count1--; count2--; count3--; count4--; extra--; }
					else if (count2 > 0) { count2--; count3--; count4--; extra--; }
					else if (count3 > 0) { count3--; count4--; extra--; }
					else if (count4 > 0) { count4--; extra--; }
				}
			}

			// adjust size of river at intersection point
			if (intersectionSize == 1)
			{
				count4 = intersectionCount;
				count1 = 0;
				count2 = 0;
				count3 = 0;
			}
			else if (intersectionSize == 2)
			{
				count3 = intersectionCount;
				count1 = 0;
				count2 = 0;
			}
			else if (intersectionSize == 3)
			{
				count2 = intersectionCount;
				count1 = 0;
			}
			else if (intersectionSize == 4)
			{
				count1 = intersectionCount;
			}
			else
			{
				count1 = 0;
				count2 = 0;
				count3 = 0;
				count4 = 0;
			}

			// dig out the river
			for (int i = river.Tiles.Count - 1; i >= 0; i--)
			{

				Tile t = river.Tiles[i];

				if (counter < count1)
				{
					t.DigRiver(river, 4);
				}
				else if (counter < count2)
				{
					t.DigRiver(river, 3);
				}
				else if (counter < count3)
				{
					t.DigRiver(river, 2);
				}
				else if (counter < count4)
				{
					t.DigRiver(river, 1);
				}
				else
				{
					t.DigRiver(river, 0);
				}
				counter++;
			}
		}

		// Dig river
		private void DigRiver(River river)
		{
			int counter = 0;

			// How wide are we digging this river?
			int size = MathHelper.RandomRange(1, 5);
			river.Length = river.Tiles.Count;

			// randomize size change
			int two = river.Length / 2;
			int three = two / 2;
			int four = three / 2;
			int five = four / 2;

			int twomin = two / 3;
			int threemin = three / 3;
			int fourmin = four / 3;
			int fivemin = five / 3;

			// randomize lenght of each size
			int count1 = MathHelper.RandomRange(fivemin, five);
			if (size < 4)
			{
				count1 = 0;
			}
			int count2 = count1 + MathHelper.RandomRange(fourmin, four);
			if (size < 3)
			{
				count2 = 0;
				count1 = 0;
			}
			int count3 = count2 + MathHelper.RandomRange(threemin, three);
			if (size < 2)
			{
				count3 = 0;
				count2 = 0;
				count1 = 0;
			}
			int count4 = count3 + MathHelper.RandomRange(twomin, two);

			// Make sure we are not digging past the river path
			if (count4 > river.Length)
			{
				int extra = count4 - river.Length;
				while (extra > 0)
				{
					if (count1 > 0) { count1--; count2--; count3--; count4--; extra--; }
					else if (count2 > 0) { count2--; count3--; count4--; extra--; }
					else if (count3 > 0) { count3--; count4--; extra--; }
					else if (count4 > 0) { count4--; extra--; }
				}
			}

			// Dig it out
			for (int i = river.Tiles.Count - 1; i >= 0; i--)
			{
				Tile t = river.Tiles[i];

				if (counter < count1)
				{
					t.DigRiver(river, 4);
				}
				else if (counter < count2)
				{
					t.DigRiver(river, 3);
				}
				else if (counter < count3)
				{
					t.DigRiver(river, 2);
				}
				else if (counter < count4)
				{
					t.DigRiver(river, 1);
				}
				else
				{
					t.DigRiver(river, 0);
				}
				counter++;
			}
		}

		private void FindPathToWater(TileDirection? td, ref River river)
		{
			while (td != null)
			{
				var tile = td.Value.Tile;
				var direction = td.Value.Direction;

				td = null;
				if (tile.Rivers.Contains(river))
					continue;

				// check if there is already a river on this tile
				if (tile.Rivers.Count > 0)
					river.Intersections++;

				river.AddTile(tile);

				// get neighbors
				Tile left = tile.Left;
				Tile right = tile.Right;
				Tile top = tile.Top;
				Tile bottom = tile.Bottom;

				float leftValue = int.MaxValue;
				float rightValue = int.MaxValue;
				float topValue = int.MaxValue;
				float bottomValue = int.MaxValue;

				// query height values of neighbors
				if (left != null && left.GetRiverNeighborCount(river) < 2 && !river.Tiles.Contains(left))
					leftValue = left.HeightValue;
				if (right != null && right.GetRiverNeighborCount(river) < 2 && !river.Tiles.Contains(right))
					rightValue = right.HeightValue;
				if (top != null && top.GetRiverNeighborCount(river) < 2 && !river.Tiles.Contains(top))
					topValue = top.HeightValue;
				if (bottom != null && bottom.GetRiverNeighborCount(river) < 2 && !river.Tiles.Contains(bottom))
					bottomValue = bottom.HeightValue;

				// if neighbor is existing river that is not this one, flow into it
				if (bottom != null && bottom.Rivers.Count == 0 && !bottom.Collidable)
					bottomValue = 0;
				if (top != null && top.Rivers.Count == 0 && !top.Collidable)
					topValue = 0;
				if (left != null && left.Rivers.Count == 0 && !left.Collidable)
					leftValue = 0;
				if (right != null && right.Rivers.Count == 0 && !right.Collidable)
					rightValue = 0;

				// override flow direction if a tile is significantly lower
				if (direction == Direction.Left)
					if (MathF.Abs(rightValue - leftValue) < 0.1f)
						rightValue = int.MaxValue;
				if (direction == Direction.Right)
					if (MathF.Abs(rightValue - leftValue) < 0.1f)
						leftValue = int.MaxValue;
				if (direction == Direction.Top)
					if (MathF.Abs(topValue - bottomValue) < 0.1f)
						bottomValue = int.MaxValue;
				if (direction == Direction.Bottom)
					if (MathF.Abs(topValue - bottomValue) < 0.1f)
						topValue = int.MaxValue;

				// find mininum
				float min = MathF.Min(MathF.Min(MathF.Min(leftValue, rightValue), topValue), bottomValue);

				// if no minimum found - exit
				if (min == int.MaxValue)
					continue;

				//Move to next neighbor
				if (min == leftValue)
				{
					if (left != null && left.Collidable)
					{
						if (river.CurrentDirection != Direction.Left)
						{
							river.TurnCount++;
							river.CurrentDirection = Direction.Left;
						}

						td = new TileDirection(left, direction);
					}
				}
				else if (min == rightValue)
				{
					if (right != null && right.Collidable)
					{
						if (river.CurrentDirection != Direction.Right)
						{
							river.TurnCount++;
							river.CurrentDirection = Direction.Right;
						}
						td = new TileDirection(right, direction);
					}
				}
				else if (min == bottomValue)
				{
					if (bottom != null && bottom.Collidable)
					{
						if (river.CurrentDirection != Direction.Bottom)
						{
							river.TurnCount++;
							river.CurrentDirection = Direction.Bottom;
						}
						td = new TileDirection(bottom, direction);
					}
				}
				else if (min == topValue)
				{
					if (top != null && top.Collidable)
					{
						if (river.CurrentDirection != Direction.Top)
						{
							river.TurnCount++;
							river.CurrentDirection = Direction.Top;
						}
						td = new TileDirection(top, direction);
					}
				}
			}
		}

		// Build a Tile array from our data
		private void LoadTiles()
		{
			GenerationResult.Tiles = new Tile[settings.Width, settings.Height];

			for (var x = 0; x < settings.Width; x++)
			{
				for (var y = 0; y < settings.Height; y++)
				{
					Tile t = new Tile();
					t.X = x;
					t.Y = y;

					//set heightmap value
					float heightValue = HeightData.Data[x, y];
					heightValue = (heightValue - HeightData.Min) / (HeightData.Max - HeightData.Min);
					t.HeightValue = heightValue;


					if (heightValue < settings.DeepWater)
					{
						t.HeightType = HeightType.DeepWater;
					}
					else if (heightValue < settings.ShallowWater)
					{
						t.HeightType = HeightType.ShallowWater;
					}
					else if (heightValue < settings.Sand)
					{
						t.HeightType = HeightType.Sand;
					}
					else if (heightValue < settings.Grass)
					{
						t.HeightType = HeightType.Grass;
					}
					else if (heightValue < settings.Forest)
					{
						t.HeightType = HeightType.Forest;
					}
					else if (heightValue < settings.Rock)
					{
						t.HeightType = HeightType.Rock;
					}
					else
					{
						t.HeightType = HeightType.Snow;
					}

					//adjust moisture based on height
					if (t.HeightType == HeightType.DeepWater)
					{
						MoistureData.Data[t.X, t.Y] += 8f * t.HeightValue;
					}
					else if (t.HeightType == HeightType.ShallowWater)
					{
						MoistureData.Data[t.X, t.Y] += 3f * t.HeightValue;
					}
					else if (t.HeightType == HeightType.Shore)
					{
						MoistureData.Data[t.X, t.Y] += 1f * t.HeightValue;
					}
					else if (t.HeightType == HeightType.Sand)
					{
						MoistureData.Data[t.X, t.Y] += 0.2f * t.HeightValue;
					}

					//Moisture Map Analyze	
					float moistureValue = MoistureData.Data[x, y];
					moistureValue = (moistureValue - MoistureData.Min) / (MoistureData.Max - MoistureData.Min);
					t.MoistureValue = moistureValue;

					//set moisture type
					if (moistureValue < settings.DryerValue) t.MoistureType = MoistureType.Dryest;
					else if (moistureValue < settings.DryValue) t.MoistureType = MoistureType.Dryer;
					else if (moistureValue < settings.WetValue) t.MoistureType = MoistureType.Dry;
					else if (moistureValue < settings.WetterValue) t.MoistureType = MoistureType.Wet;
					else if (moistureValue < settings.WettestValue) t.MoistureType = MoistureType.Wetter;
					else t.MoistureType = MoistureType.Wettest;


					// Adjust Heat Map based on settings.Height - Higher == colder
					if (t.HeightType == HeightType.Forest)
					{
						HeatData.Data[t.X, t.Y] -= 0.1f * t.HeightValue;
					}
					else if (t.HeightType == HeightType.Rock)
					{
						HeatData.Data[t.X, t.Y] -= 0.25f * t.HeightValue;
					}
					else if (t.HeightType == HeightType.Snow)
					{
						HeatData.Data[t.X, t.Y] -= 0.4f * t.HeightValue;
					}
					else
					{
						HeatData.Data[t.X, t.Y] += 0.01f * t.HeightValue;
					}

					// Set heat value
					float heatValue = HeatData.Data[x, y];
					heatValue = (heatValue - HeatData.Min) / (HeatData.Max - HeatData.Min);
					t.HeatValue = heatValue;

					// set heat type
					if (heatValue < settings.ColdestValue) t.HeatType = HeatType.Coldest;
					else if (heatValue < settings.ColderValue) t.HeatType = HeatType.Colder;
					else if (heatValue < settings.ColdValue) t.HeatType = HeatType.Cold;
					else if (heatValue < settings.WarmValue) t.HeatType = HeatType.Warm;
					else if (heatValue < settings.WarmerValue) t.HeatType = HeatType.Warmer;
					else t.HeatType = HeatType.Warmest;

					if (Clouds1 != null)
					{
						t.Cloud1Value = Clouds1.Data[x, y];
						t.Cloud1Value = (t.Cloud1Value - Clouds1.Min) / (Clouds1.Max - Clouds1.Min);
					}

					if (Clouds2 != null)
					{
						t.Cloud2Value = Clouds2.Data[x, y];
						t.Cloud2Value = (t.Cloud2Value - Clouds2.Min) / (Clouds2.Max - Clouds2.Min);
					}

					GenerationResult.Tiles[x, y] = t;
				}
			}
		}

		private void UpdateNeighbors()
		{
			GenerationResult.UpdateNeighbors();
		}

		private void UpdateBitmasks()
		{
			GenerationResult.UpdateBitmask();
		}

		private void FloodFill()
		{
			// Use a stack instead of recursion
			Stack<Tile> stack = new Stack<Tile>();

			for (int x = 0; x < settings.Width; x++)
			{
				for (int y = 0; y < settings.Height; y++)
				{

					Tile t = GenerationResult.Tiles[x, y];

					//Tile already flood filled, skip
					if (t.FloodFilled) continue;

					// Land
					if (t.Collidable)
					{
						TileGroup group = new TileGroup();
						group.Type = TileGroupType.Land;
						stack.Push(t);

						while (stack.Count > 0)
						{
							FloodFill(stack.Pop(), ref group, ref stack);
						}

						if (group.Tiles.Count > 0)
							GenerationResult.Lands.Add(group);
					}
					// Water
					else
					{
						TileGroup group = new TileGroup();
						group.Type = TileGroupType.Water;
						stack.Push(t);

						while (stack.Count > 0)
						{
							FloodFill(stack.Pop(), ref group, ref stack);
						}

						if (group.Tiles.Count > 0)
							GenerationResult.Waters.Add(group);
					}
				}
			}
		}

		private void FloodFill(Tile tile, ref TileGroup tiles, ref Stack<Tile> stack)
		{
			// Validate
			if (tile == null)
				return;
			if (tile.FloodFilled)
				return;
			if (tiles.Type == TileGroupType.Land && !tile.Collidable)
				return;
			if (tiles.Type == TileGroupType.Water && tile.Collidable)
				return;

			// Add to TileGroup
			tiles.Tiles.Add(tile);
			tile.FloodFilled = true;

			// floodfill into neighbors
			Tile t = tile.Top;
			if (t != null && !t.FloodFilled && tile.Collidable == t.Collidable)
				stack.Push(t);
			t = tile.Bottom;
			if (t != null && !t.FloodFilled && tile.Collidable == t.Collidable)
				stack.Push(t);
			t = tile.Left;
			if (t != null && !t.FloodFilled && tile.Collidable == t.Collidable)
				stack.Push(t);
			t = tile.Right;
			if (t != null && !t.FloodFilled && tile.Collidable == t.Collidable)
				stack.Push(t);
		}
	}
}