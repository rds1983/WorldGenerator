using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

namespace WorldGenerator.App.ThreeD
{
	public sealed class View3D : Widget
	{
		private const float NearPlaneDistance = 0.1f;
		private const float FarPlaneDistance = 1000.0f;

		private Mesh _mesh;
		private BasicEffect _basicEffect;
		private readonly Camera _camera = new Camera();
		private float _angle;

		public Texture2D Texture { get; set; }

		public View3D()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Stretch;

			_camera.SetLookAt(new Vector3(0, 0, -15), Vector3.Zero);
		}

		public override void InternalRender(RenderContext context)
		{
			base.InternalRender(context);

			if (Texture == null)
			{
				return;
			}

			var device = MyraEnvironment.GraphicsDevice;

			if (_basicEffect == null)
			{
				_basicEffect = new BasicEffect(device);
			}

			if (_mesh == null)
			{
				_mesh = PrimitiveFactory.CreateSphere(device, 10);
			}

			context.End();

			// Save current device state
			var oldViewPort = device.Viewport;
			var oldDepthStencilState = device.DepthStencilState;
			var oldRasterizerState = device.RasterizerState;
			var oldBlendState = device.BlendState;
			var oldSamplesState = device.SamplerStates[0];

			// Set the new one
			var screenPosition = ToGlobal(Point.Zero);
			device.Viewport = new Viewport(screenPosition.X, screenPosition.Y, ActualBounds.Width, ActualBounds.Height);

			device.DepthStencilState = DepthStencilState.Default;
			device.RasterizerState = RasterizerState.CullCounterClockwise;
			device.BlendState = BlendState.AlphaBlend;
			device.SamplerStates[0] = SamplerState.LinearWrap;

			// Set vertex/index buffers
			device.SetVertexBuffer(_mesh.VertexBuffer);
			device.Indices = _mesh.IndexBuffer;

			// Calculate and set effect params
			var view = _camera.View;
			var projection = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.ToRadians(_camera.ViewAngle),
				device.Viewport.AspectRatio,
				NearPlaneDistance, FarPlaneDistance
			);

			var world = Matrix.CreateRotationY(_angle);

			_basicEffect.View = view;
			_basicEffect.World = world;
			_basicEffect.Projection = projection;

			_basicEffect.Texture = Texture;
			_basicEffect.TextureEnabled = true;

			// Render
			foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
			{
				pass.Apply();

				device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
					0,
					_mesh.VertexBuffer.VertexCount,
					0,
					_mesh.PrimitiveCount);
			}

			// Update the rotation angle
			_angle += 0.01f;

			// Restore the device state
			device.Viewport = oldViewPort;
			device.DepthStencilState = oldDepthStencilState;
			device.RasterizerState = oldRasterizerState;
			device.BlendState = oldBlendState;
			device.SamplerStates[0] = oldSamplesState;

			context.Begin();
		}
	}
}
