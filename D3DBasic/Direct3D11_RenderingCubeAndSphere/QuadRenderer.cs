using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

using Common;

// Resolve class name conflicts by explicitly stating
// which class they refer to:
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Direct3D11_RenderingCubeAndSphere
{
    class QuadRenderer : Common.RendererBase
    {
        Buffer quadVertices;
        Buffer quadIndices;
        VertexBufferBinding quadBinding;
        Color color;

        public QuadRenderer(Color color)
        {
            this.color = color;
        }

        public QuadRenderer() : this(Color.LightGray) { }

        protected override void CreateDeviceDependentResources()
        {
            // Ensure that if already set the device resources are correctly disposed of before recreating
            RemoveAndDispose(ref quadVertices);
            RemoveAndDispose(ref quadIndices);

            // Retrieve our SharpDX.Direct3D11.Device1 instance
            var device = this.DeviceManager.Direct3DDevice;

            // Create vertex buffer for quad
            quadVertices = ToDispose(Buffer.Create(device, BindFlags.VertexBuffer, new Vertex[] {
                /*  Position: float x 3, Normal: Vector3, Color */
                new Vertex(-0.5f, 0f, -0.5f, Vector3.UnitY, color),
                new Vertex(-0.5f, 0f,  0.5f, Vector3.UnitY, color),
                new Vertex( 0.5f, 0f,  0.5f, Vector3.UnitY, color),
                new Vertex( 0.5f, 0f, -0.5f, Vector3.UnitY, color),
            }));
            quadBinding = new VertexBufferBinding(quadVertices, Utilities.SizeOf<Vertex>(), 0);

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
