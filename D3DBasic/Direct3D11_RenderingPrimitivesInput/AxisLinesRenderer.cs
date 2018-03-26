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
    class AxisLinesRenderer : Common.RendererBase
    {
        Buffer axisLinesVertices;

        VertexBufferBinding axisLinesBinding;

        protected override void CreateDeviceDependentResources()
        {
            base.CreateDeviceDependentResources();

            RemoveAndDispose(ref axisLinesVertices);

            var device = this.DeviceManager.Direct3DDevice;

            axisLinesVertices = ToDispose(Buffer.Create(device, BindFlags.VertexBuffer, new[]
            {
                /*  Vertex Position                       Vertex Color */
                new Vector4(-1f, 0f, 0f, 1f), (Vector4)Color.Red, // - x-axis 
                new Vector4(1f, 0f, 0f, 1f), (Vector4)Color.Red,  // + x-axis
                new Vector4(0.9f, -0.05f, 0f, 1f), (Vector4)Color.Red,// arrow head start
                new Vector4(1f, 0f, 0f, 1f), (Vector4)Color.Red,
                new Vector4(0.9f, 0.05f, 0f, 1f), (Vector4)Color.Red,
                new Vector4(1f, 0f, 0f, 1f), (Vector4)Color.Red,  // arrow head end
                    
                new Vector4(0f, -1f, 0f, 1f), (Vector4)Color.Lime, // - y-axis
                new Vector4(0f, 1f, 0f, 1f), (Vector4)Color.Lime,  // + y-axis
                new Vector4(-0.05f, 0.9f, 0f, 1f), (Vector4)Color.Lime,// arrow head start
                new Vector4(0f, 1f, 0f, 1f), (Vector4)Color.Lime,
                new Vector4(0.05f, 0.9f, 0f, 1f), (Vector4)Color.Lime,
                new Vector4(0f, 1f, 0f, 1f), (Vector4)Color.Lime,  // arrow head end
                    
                new Vector4(0f, 0f, -1f, 1f), (Vector4)Color.Blue, // - z-axis
                new Vector4(0f, 0f, 1f, 1f), (Vector4)Color.Blue,  // + z-axis
                new Vector4(0f, -0.05f, 0.9f, 1f), (Vector4)Color.Blue,// arrow head start
                new Vector4(0f, 0f, 1f, 1f), (Vector4)Color.Blue,
                new Vector4(0f, 0.05f, 0.9f, 1f), (Vector4)Color.Blue,
                new Vector4(0f, 0f, 1f, 1f), (Vector4)Color.Blue,  // arrow head end
            }));
            axisLinesBinding = new VertexBufferBinding(axisLinesVertices, Utilities.SizeOf<Vector4>() * 2, 0);
        }

        protected override void DoRender()
        {
            var context = this.DeviceManager.Direct3DContext;

            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            context.InputAssembler.SetVertexBuffers(0, axisLinesBinding);
            context.Draw(18, 0);
        }
    }
}
