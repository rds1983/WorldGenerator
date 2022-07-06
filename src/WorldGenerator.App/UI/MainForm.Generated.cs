/* Generated by MyraPad at 7/6/2022 9:36:43 PM */
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI.Properties;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
using System.Numerics;
#endif

namespace WorldGenerator.App.UI
{
	partial class MainForm: VerticalStackPanel
	{
		private void BuildUI()
		{
			_buttonGenerate = new TextButton();
			_buttonGenerate.Text = "Generate";
			_buttonGenerate.Width = 100;
			_buttonGenerate.Id = "_buttonGenerate";

			var horizontalStackPanel1 = new HorizontalStackPanel();
			horizontalStackPanel1.Widgets.Add(_buttonGenerate);

			var horizontalSeparator1 = new HorizontalSeparator();

			var label1 = new Label();
			label1.Text = "Type:";

			_buttonWrapped = new RadioButton();
			_buttonWrapped.Text = "Wrapped";
			_buttonWrapped.Id = "_buttonWrapped";

			_buttonSpherical = new RadioButton();
			_buttonSpherical.Text = "Spherical";
			_buttonSpherical.Id = "_buttonSpherical";

			var horizontalStackPanel2 = new HorizontalStackPanel();
			horizontalStackPanel2.Spacing = 8;
			horizontalStackPanel2.Widgets.Add(label1);
			horizontalStackPanel2.Widgets.Add(_buttonWrapped);
			horizontalStackPanel2.Widgets.Add(_buttonSpherical);

			_panelProperties = new Panel();
			_panelProperties.Id = "_panelProperties";

			var verticalStackPanel1 = new VerticalStackPanel();
			verticalStackPanel1.Widgets.Add(horizontalStackPanel2);
			verticalStackPanel1.Widgets.Add(_panelProperties);

			var scrollViewer1 = new ScrollViewer();
			scrollViewer1.Content = verticalStackPanel1;

			_buttonHeightMap = new RadioButton();
			_buttonHeightMap.Text = "Height Map";
			_buttonHeightMap.Id = "_buttonHeightMap";

			_buttonHeatMap = new RadioButton();
			_buttonHeatMap.Text = "Heat Map";
			_buttonHeatMap.Id = "_buttonHeatMap";

			_buttonMoistureMap = new RadioButton();
			_buttonMoistureMap.Text = "Moisture Map";
			_buttonMoistureMap.Id = "_buttonMoistureMap";

			_buttonBiomeMap = new RadioButton();
			_buttonBiomeMap.Text = "Biome Map";
			_buttonBiomeMap.Id = "_buttonBiomeMap";

			var horizontalStackPanel3 = new HorizontalStackPanel();
			horizontalStackPanel3.Spacing = 8;
			horizontalStackPanel3.Widgets.Add(_buttonHeightMap);
			horizontalStackPanel3.Widgets.Add(_buttonHeatMap);
			horizontalStackPanel3.Widgets.Add(_buttonMoistureMap);
			horizontalStackPanel3.Widgets.Add(_buttonBiomeMap);

			var horizontalSeparator2 = new HorizontalSeparator();

			_image2DView = new Image();
			_image2DView.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Center;
			_image2DView.VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Center;
			_image2DView.Id = "_image2DView";

			var verticalStackPanel2 = new VerticalStackPanel();
			verticalStackPanel2.Proportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Auto,
			});
			verticalStackPanel2.Proportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Auto,
			});
			verticalStackPanel2.Proportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Fill,
			});
			verticalStackPanel2.Widgets.Add(horizontalStackPanel3);
			verticalStackPanel2.Widgets.Add(horizontalSeparator2);
			verticalStackPanel2.Widgets.Add(_image2DView);

			_panelLog = new Panel();
			_panelLog.Id = "_panelLog";

			var panel1 = new Panel();
			panel1.Widgets.Add(verticalStackPanel2);
			panel1.Widgets.Add(_panelLog);

			_splitPane = new HorizontalSplitPane();
			_splitPane.Id = "_splitPane";
			_splitPane.Widgets.Add(scrollViewer1);
			_splitPane.Widgets.Add(panel1);

			
			Proportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Auto,
			});
			Proportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Auto,
			});
			Proportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Fill,
			});
			Widgets.Add(horizontalStackPanel1);
			Widgets.Add(horizontalSeparator1);
			Widgets.Add(_splitPane);
		}

		
		public TextButton _buttonGenerate;
		public RadioButton _buttonWrapped;
		public RadioButton _buttonSpherical;
		public Panel _panelProperties;
		public RadioButton _buttonHeightMap;
		public RadioButton _buttonHeatMap;
		public RadioButton _buttonMoistureMap;
		public RadioButton _buttonBiomeMap;
		public Image _image2DView;
		public Panel _panelLog;
		public HorizontalSplitPane _splitPane;
	}
}
