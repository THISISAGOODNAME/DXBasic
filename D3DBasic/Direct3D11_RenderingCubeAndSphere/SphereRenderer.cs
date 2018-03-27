using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

using Common;

// Resolve class name conflicts by explicitly stating
// which class they refer to:
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Direct3D11_RenderingCubeAndSphere
{
    public class SphereRenderer : Common.RendererBase
    {
        Buffer vertexBuffer;
        Buffer indexBuffer;
        VertexBufferBinding vertexBinding;

        int totalVertexCount = 0;

        protected override void CreateDeviceDependentResources()
        {
            RemoveAndDispose(ref vertexBuffer);
            RemoveAndDispose(ref indexBuffer);

            // Retrieve our SharpDX.Direct3D11.Device1 instance
            var device = this.DeviceManager.Direct3DDevice;

            Vertex[] vertices;
            int[] indices;
            GeometricPrimitives.GenerateSphere(out vertices, out indices, Color.Gray);

            vertexBuffer = ToDispose(Buffer.Create(device, BindFlags.VertexBuffer, vertices));
            vertexBinding = new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vertex>(), 0);

            indexBuffer = ToDispose(Buffer.Create(device, BindFlags.IndexBuffer, indices));
            totalVertexCount = indices.Length;
        }

        protected override void DoRender()
        {
            var context = this.DeviceManager.Direct3DContext;

            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            context.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);
            context.InputAssembler.SetVertexBuffers(0, vertexBinding);
            context.DrawIndexed(totalVertexCount, 0, 0);
        }
    }
}
