using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI.Properties;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorldGenerator.App.ThreeD;

namespace WorldGenerator.App.UI
{
	public partial class MainForm
	{
		private enum ViewType
		{
			View2D,
			View3D
		};

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
		private ViewType _viewType = ViewType.View2D;

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
				_viewType = generator is WrappingWorldGenerator ? ViewType.View2D : ViewType.View3D;

				LogMessage("Building textures");

				var tiles = _result.Tiles;

				_textureHeight = TextureGenerator.GetHeightMapTexture(Game1.Instance.GraphicsDevice, tiles.GetLength(0), tiles.GetLength(1), tiles);
				_textureHeat = TextureGenerator.GetHeatMapTexture(Game1.Instance.GraphicsDevice, tiles.GetLength(0), tiles.GetLength(1), tiles);
				_textureMoisture = TextureGenerator.GetMoistureMapTexture(Game1.Instance.GraphicsDevice, tiles.GetLength(0), tiles.GetLength(1), tiles);
				_textureBiome = TextureGenerator.GetBiomeMapTexture(Game1.Instance.GraphicsDevice, tiles.GetLength(0), tiles.GetLength(1), tiles, _config.ColdestValue, _config.ColderValue, _config.ColdValue);

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
					_logMessage = string.Empty;
				});
			}
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

			if (_viewType == ViewType.View2D)
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