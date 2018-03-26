using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D11;

using Buffer = SharpDX.Direct3D11.Buffer;

namespace Direct3D11_RenderingPrimitivesInput
{
    class TriangleRenderer : Common.RendererBase
    {
        Buffer triangleVertices;

        VertexBufferBinding triangleBinding;

        protected override void CreateDeviceDependentResources()
        {
            base.CreateDeviceDependentResources();

            RemoveAndDispose(ref triangleVertices);

            var device = this.DeviceManager.Direct3DDevice;

            triangleVertices = ToDispose(Buffer.Create(device, BindFlags.VertexBuffer, new[]
            {
                /*  Vertex Position                       Vertex Color */
                new Vector4(0.0f, 0.0f, 0.5f, 1.0f),  new Vector4(0.0f, 0.0f, 1.0f, 1.0f), // Base-right
                new Vector4(-0.5f, 0.0f, 0.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), // Base-left
                new Vector4(-0.25f, 1f, 0.25f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // Apex
            }));

            triangleBinding = new VertexBufferBinding(triangleVertices, Utilities.SizeOf<Vector4>() * 2, 0);
        }

        protected override void DoRender()
        {
            var context = this.DeviceManager.Direct3DContext;

            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, triangleBinding);
            context.Draw(3, 0);
        }
    }
}
