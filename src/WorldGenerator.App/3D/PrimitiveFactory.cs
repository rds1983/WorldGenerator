using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace WorldGenerator.App.ThreeD
{
	public static class PrimitiveFactory
	{
		public static Mesh CreateSphere(GraphicsDevice device, float diameter = 1.0f, int tessellation = 16)
		{
			if (tessellation < 3) throw new ArgumentOutOfRangeException("tessellation", "Must be >= 3");

			var verticalSegments = tessellation;
			var horizontalSegments = tessellation * 2;

			var vertices = new VertexPositionNormalTexture[(verticalSegments + 1) * (horizontalSegments + 1)];
			var indices = new short[verticalSegments * (horizontalSegments + 1) * 6];

			var radius = diameter / 2;

			var vertexCount = 0;
			// Create rings of vertices at progressively higher latitudes.
			for (var i = 0; i <= verticalSegments; i++)
			{
				var v = 1.0f - (float)i / verticalSegments;

				var latitude = (float)((i * Math.PI / verticalSegments) - Math.PI / 2.0);
				var dy = (float)Math.Sin(latitude);
				var dxz = (float)Math.Cos(latitude);

				// Create a single ring of vertices at this latitude.
				for (var j = 0; j <= horizontalSegments; j++)
				{
					var u = (float)j / horizontalSegments;

					var longitude = (float)(j * 2.0 * Math.PI / horizontalSegments);
					var dx = (float)Math.Sin(longitude);
					var dz = (float)Math.Cos(longitude);

					dx *= dxz;
					dz *= dxz;

					var normal = new Vector3(dx, dy, dz);
					var textureCoordinate = new Vector2(u, v);

					vertices[vertexCount++] = new VertexPositionNormalTexture(normal * radius, normal, textureCoordinate);
				}
			}

			// Fill the index buffer with triangles joining each pair of latitude rings.
			var stride = horizontalSegments + 1;

			var indexCount = 0;
			for (var i = 0; i < verticalSegments; i++)
			{
				for (var j = 0; j <= horizontalSegments; j++)
				{
					var nextI = i + 1;
					var nextJ = (j + 1) % stride;

					indices[indexCount++] = (short)(i * stride + j);
					indices[indexCount++] = (short)(nextI * stride + j);
					indices[indexCount++] = (short)(i * stride + nextJ);

					indices[indexCount++] = (short)(i * stride + nextJ);
					indices[indexCount++] = (short)(nextI * stride + j);
					indices[indexCount++] = (short)(nextI * stride + nextJ);
				}
			}

			return Mesh.Create(device, vertices, indices);
		}
	}
}
