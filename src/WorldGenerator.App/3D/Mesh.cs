using Microsoft.Xna.Framework.Graphics;
using System;

namespace WorldGenerator.App.ThreeD
{
	public class Mesh: IDisposable
	{
		public VertexBuffer VertexBuffer { get; set; }
		public IndexBuffer IndexBuffer { get; set; }

		public int PrimitiveCount
		{
			get
			{
				return IndexBuffer.IndexCount / 3;
			}
		}

		public Mesh()
		{
		}

		~Mesh()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				VertexBuffer.Dispose();
				IndexBuffer.Dispose();
			}
		}

		public static Mesh Create<T>(GraphicsDevice device, T[] vertices, short[] indices) where T : struct, IVertexType
		{
			var vertexBuffer = new VertexBuffer(device, 
				new T().VertexDeclaration, 
				vertices.Length,
				BufferUsage.None);

			vertexBuffer.SetData(vertices);

			var indexBuffer = new IndexBuffer(device, 
				IndexElementSize.SixteenBits, 
				indices.Length, 
				BufferUsage.None);
			indexBuffer.SetData(indices);

			return new Mesh
			{
				VertexBuffer = vertexBuffer,
				IndexBuffer = indexBuffer
			};
		}
	}
}
