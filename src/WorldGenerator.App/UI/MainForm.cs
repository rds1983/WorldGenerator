using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI.File;
using Myra.Graphics2D.UI.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WorldGenerator.App.ThreeD;

namespace WorldGenerator.App.UI
{
	public partial class MainForm
	{
		private class Log : ILog
		{
			private readonly MainForm _mainForm;

			public Log(MainForm mainForm)
			{
				_mainForm = mainForm;
			}

			public void SetProgress(float? progress)
			{
				if (progress == null)
				{
					_mainForm._progressLog.Visible = false;
				}
				else
				{
					_mainForm._progressLog.Visible = true;
					_mainForm._progressLog.Value = progress.Value * _mainForm._progressLog.Maximum;
				}
			}

			void ILog.Log(string message)
			{
				_mainForm.LogMessage(message);
			}
		}

		private PropertyGrid _propertyGrid;
		private GeneratorSettings _config;
		private readonly List<Action> _uiThreadActions = new List<Action>();
		private string _logMessage;
		private GenerationResult _result;
		private View3D _view3d;
		private Texture2D _textureHeight, _textureHeat, _textureMoisture, _textureBiome;

		public MainForm()
		{
			BuildUI();

			_progressLog.Visible = false;

			_buttonWrapped.IsPressed = true;
			_buttonBiomeMap.IsPressed = true;

			_view3d = new View3D();
			_panelView.AddChild(_view3d);

			_config = new GeneratorSettings();
			_propertyGrid = new PropertyGrid
			{
				Object = _config
			};

			_splitPane.SetSplitterPosition(0, 0.35f);
			_panelProperties.Widgets.Add(_propertyGrid);

			_buttonGenerate.Click += _buttonGenerate_Click;

			_buttonHeightMap.PressedChanged += (s, a) => UpdateView();
			_buttonHeatMap.PressedChanged += (s, a) => UpdateView();
			_buttonMoistureMap.PressedChanged += (s, a) => UpdateView();
			_buttonBiomeMap.PressedChanged += (s, a) => UpdateView();

			UpdateSaveEnabled();

			_image2DView.MouseMoved += (s, e) =>
			{
				if (_result == null)
				{
					return;
				}

				var local = _image2DView.ToLocal(Desktop.MousePosition);
				var nx = local.X * _result.Width / _image2DView.ActualBounds.Width;
				var ny = local.Y * _result.Height / _image2DView.ActualBounds.Height;

				var tile = _result.Tiles[nx, ny];

				Debug.WriteLine($"X={nx}, Y={ny}, Height={tile.HeightValue}, HeightType={tile.HeightType}, BiomeType={tile.BiomeType}");
			};

			_buttonSave.Click += (s, a) => SaveResult();
			_buttonLoad.Click += (s, a) => LoadResult();
		}

		private void SaveResult()
		{
			var dialog = new FileDialog(FileDialogMode.SaveFile)
			{
				Filter = "*.bin"
			};

			dialog.Closed += (s, a) =>
			{
				if (!dialog.Result)
				{
					return;
				}

				using (var stream = File.Create(dialog.FilePath))
				using (var writer = new BinaryWriter(stream))
				{
					writer.Write((int)_result.MapType);
					writer.Write(_result.Width);
					writer.Write(_result.Height);

					for(var x = 0; x < _result.Width; ++x)
					{
						for (var y = 0; y < _result.Height; ++y)
						{
							var tile = _result.Tiles[x, y];
							writer.Write((int)tile.HeightType);
							writer.Write(tile.HeightValue);
							writer.Write((int)tile.HeatType);
							writer.Write(tile.HeatValue);
							writer.Write((int)tile.MoistureType);
							writer.Write(tile.MoistureValue);
						}
					}
				}
			};

			dialog.ShowModal(Desktop);
		}

		private void LoadResult()
		{
			var dialog = new FileDialog(FileDialogMode.OpenFile)
			{
				Filter = "*.bin"
			};

			dialog.Closed += (s, a) =>
			{
				if (!dialog.Result)
				{
					return;
				}

				var result = new GenerationResult();

				using (var stream = File.OpenRead(dialog.FilePath))
				using (var reader = new BinaryReader(stream))
				{
					result.MapType = (MapType)reader.ReadInt32();

					var width = reader.ReadInt32();
					var height = reader.ReadInt32();

					result.Tiles = new Tile[width, height];
					for (var x = 0; x < result.Width; ++x)
					{
						for (var y = 0; y < result.Height; ++y)
						{
							var tile = new Tile
							{
								X = x,
								Y = y,
								HeightType = (HeightType)reader.ReadInt32(),
								HeightValue = reader.ReadSingle(),
								HeatType = (HeatType)reader.ReadInt32(),
								HeatValue = reader.ReadSingle(),
								MoistureType = (MoistureType)reader.ReadInt32(),
								MoistureValue = reader.ReadSingle()
							};

							result.Tiles[x, y] = tile;
						}
					}
				}

				result.UpdateNeighbors();
				result.UpdateBitmask();
				result.UpdateBiomeMask();

				_result = result;
				UpdateSaveEnabled();
				UpdateTextures();
				UpdateView();
			};

			dialog.ShowModal(Desktop);
		}

		private void UpdateSaveEnabled()
		{
			_buttonSave.Enabled = _result != null;
		}

		public void LogMessage(string message)
		{
			_logMessage = message;
		}

		private void _buttonGenerate_Click(object sender, EventArgs e)
		{
			Task.Factory.StartNew(GenerateTask);
		}

		private void GenerateTask()
		{
			try
			{
				ExecuteAtUIThread(() =>
				{
					_buttonGenerate.Enabled = false;
					_buttonSave.Enabled = false;
					_buttonLoad.Enabled = false;
					_labelLog.Text = "Starting...";
				});

				Generator generator;
				var logger = new Log(this);

				if (_buttonWrapped.IsPressed)
				{
					generator = new WrappingWorldGenerator(_config, logger);
				}
				else
				{
					generator = new SphericalWorldGenerator(_config, logger);
				}

				generator.Go();

				_result = generator.GenerationResult;

				LogMessage("Building textures");

				UpdateTextures();

				ExecuteAtUIThread(() =>
				{
					UpdateView();
				});
			}
			finally
			{
				ExecuteAtUIThread(() =>
				{
					_buttonGenerate.Enabled = true;
					UpdateSaveEnabled();
					_buttonLoad.Enabled = true;

					_logMessage = string.Empty;
				});
			}
		}

		private void UpdateTextures()
		{
			var tiles = _result.Tiles;

			var width = _result.Width;
			var height = _result.Height;
			_textureHeight = TextureGenerator.GetHeightMapTexture(Game1.Instance.GraphicsDevice, width, height, tiles);
			_textureHeat = TextureGenerator.GetHeatMapTexture(Game1.Instance.GraphicsDevice, width, height, tiles);
			_textureMoisture = TextureGenerator.GetMoistureMapTexture(Game1.Instance.GraphicsDevice, width, height, tiles);
			_textureBiome = TextureGenerator.GetBiomeMapTexture(Game1.Instance.GraphicsDevice, width, height, tiles, _config.ColdestValue, _config.ColderValue, _config.ColdValue);
		}

		private void UpdateView()
		{
			if (_result == null)
			{
				return;
			}

			Texture2D texture;
			if (_buttonHeightMap.IsPressed)
			{
				texture = _textureHeight;
			}
			else if (_buttonHeatMap.IsPressed)
			{
				texture = _textureHeat;
			}
			else if (_buttonMoistureMap.IsPressed)
			{
				texture = _textureMoisture;
			}
			else
			{
				texture = _textureBiome;
			}

			if (_result.MapType == MapType.Wrapping)
			{
				// 2D
				_image2DView.Renderable = new TextureRegion(texture);
				_image2DView.Visible = true;
				_view3d.Visible = false;
			}
			else
			{
				// 3D
				_view3d.Texture = texture;
				_view3d.Visible = true;
				_image2DView.Visible = false;
			}
		}

		private void ExecuteAtUIThread(Action action)
		{
			lock (_uiThreadActions)
			{
				_uiThreadActions.Add(action);
			}
		}

		public override void InternalRender(RenderContext context)
		{
			base.InternalRender(context);

			lock (_uiThreadActions)
			{
				foreach (var action in _uiThreadActions)
				{
					action();
				}

				_uiThreadActions.Clear();

				_labelLog.Text = _logMessage;
			}
		}
	}
}