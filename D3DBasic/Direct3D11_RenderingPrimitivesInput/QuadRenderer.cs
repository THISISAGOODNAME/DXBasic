using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Buffer = SharpDX.Direct3D11.Buffer;

namespace Direct3D11_RenderingPrimitivesInput
{
    class QuadRenderer : Common.RendererBase
    {
        Buffer quadVertices;

        Buffer quadIndices;

        VertexBufferBinding quadBinding;

        protected override void CreateDeviceDependentResources()
        {
            // Ensure that if already set the device resources
            // are correctly disposed of before recreating
            RemoveAndDispose(ref quadVertices);
            RemoveAndDispose(ref quadIndices);

            // Retrieve our SharpDX.Direct3D11.Device1 instance
            var device = this.DeviceManager.Direct3DDevice;

            // Create a quad (two triangles)
            quadVertices = ToDispose(Buffer.Create(device, BindFlags.VertexBuffer, new[] {
            /*  Vertex Position                       Vertex Color */
                new Vector4(0.25f, 0.5f, -0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // Top-left
                new Vector4(0.75f, 0.5f, -0.5f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), // Top-right
                new Vector4(0.75f, 0.0f, -0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), // Base-right
                new Vector4(0.25f, 0.0f, -0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), // Base-left
            }));
            quadBinding = new VertexBufferBinding(quadVertices, Utilities.SizeOf<Vector4>() * 2, 0);

            // v0    v1
            // |-----|
            // | \ A |
            // | B \ |
            // |-----|
            // v3    v2
            quadIndices = ToDispose(Buffer.Create(device, BindFlags.IndexBuffer, new ushort[] {
                0, 1, 2, // A
                2, 3, 0  // B
            }));
        }

        protected override void DoRender()
        {
            var context = this.DeviceManager.Direct3DContext;

            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            context.InputAssembler.SetIndexBuffer(quadIndices, Format.R16_UInt, 0);
            context.InputAssembler.SetVertexBuffers(0, quadBinding);
            context.DrawIndexed(6, 0, 0);
        }
    }
}
