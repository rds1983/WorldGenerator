using Microsoft.Xna.Framework.Graphics;
using Myra.Extended.Widgets;
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI.Properties;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorldGenerator.App.ThreeD;

namespace WorldGenerator.App.UI
{
	public partial class MainForm
	{
		private PropertyGrid _propertyGrid;
		private GeneratorSettings _config;
		private LogView _logView;
		private readonly List<Action> _uiThreadActions = new List<Action>();
		private AutoResetEvent _uiEvent = new AutoResetEvent(false);
		private Generator _generator;
		private View3D _view3d;

		public MainForm()
		{
			BuildUI();

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

			_logView = new LogView();
			_panelLog.Widgets.Add(_logView);
			_panelLog.Visible = false;

			_buttonGenerate.Click += _buttonGenerate_Click;

			_buttonHeightMap.PressedChanged += (s, a) => UpdateView();
			_buttonHeatMap.PressedChanged += (s, a) => UpdateView();
			_buttonMoistureMap.PressedChanged += (s, a) => UpdateView();
			_buttonBiomeMap.PressedChanged += (s, a) => UpdateView();
		}

		public void LogMessage(string message)
		{
			ExecuteAtUIThread(() =>
			{
				_logView.Log(message);
			});
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
					_logView.ClearLog();
					_panelLog.Visible = true;

					// HACK: Recalculate layout so _logView size gets updated
					Desktop.UpdateLayout();
				});

				Generator generator;

				if (_buttonWrapped.IsPressed)
				{
					generator = new WrappingWorldGenerator(_config, LogMessage);
				}
				else
				{
					generator = new SphericalWorldGenerator(_config, LogMessage);
				}

				generator.Go();

				_generator = generator;
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
					_panelLog.Visible = false;
				});
			}
		}

		private void UpdateView()
		{
			if (_generator == null)
			{
				return;
			}

			Texture2D texture;
			var tiles = _generator.Tiles;
			if (_buttonHeightMap.IsPressed)
			{
				texture = TextureGenerator.GetHeightMapTexture(Game1.Instance.GraphicsDevice, tiles.GetLength(0), tiles.GetLength(1), tiles);
			}
			else if (_buttonHeatMap.IsPressed)
			{
				texture = TextureGenerator.GetHeatMapTexture(Game1.Instance.GraphicsDevice, tiles.GetLength(0), tiles.GetLength(1), tiles);
			}
			else if (_buttonMoistureMap.IsPressed)
			{
				texture = TextureGenerator.GetMoistureMapTexture(Game1.Instance.GraphicsDevice, tiles.GetLength(0), tiles.GetLength(1), tiles);
			}
			else
			{
				texture = TextureGenerator.GetBiomeMapTexture(Game1.Instance.GraphicsDevice, tiles.GetLength(0), tiles.GetLength(1), tiles, _config.ColdestValue, _config.ColderValue, _config.ColdValue);
			}

			if (_generator is WrappingWorldGenerator)
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

			_uiEvent.WaitOne();
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
				_uiEvent.Set();
			}
		}
	}
}