using Myra.Extended.Widgets;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Properties;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorldGenerator;

namespace FantasyMapGenerator.App.UI
{
	public partial class MainForm
	{
		private PropertyGrid _propertyGrid;
		private GeneratorSettings _config;
		private Image _mapView;
		private LogView _logView;
		private readonly List<Action> _uiThreadActions = new List<Action>();
		private AutoResetEvent _uiEvent = new AutoResetEvent(false);

		public MainForm()
		{
			BuildUI();

			_config = new GeneratorSettings();
			_propertyGrid = new PropertyGrid
			{
				Object = _config
			};

			_splitPane.SetSplitterPosition(0, 0.25f);
			_panelProperties.Widgets.Add(_propertyGrid);

			_mapView = new Image();
			_panelMap.Widgets.Add(_mapView);

			_logView = new LogView();
			_panelLog.Widgets.Add(_logView);
			_panelLog.Visible = false;

			_buttonGenerate.Click += _buttonGenerate_Click;
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
				});

/*				var landGenerator = new LandGenerator(_config);
				var result = landGenerator.Generate();
				var locationsGenerator = new LocationsGenerator(_config);
				locationsGenerator.Generate(result);

				_mapView.Map = result;*/
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

		private void ExecuteAtUIThread(Action action)
		{
			lock(_uiThreadActions)
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
				foreach(var action in _uiThreadActions)
				{
					action();
				}

				_uiThreadActions.Clear();
				_uiEvent.Set();
			}
		}
	}
}