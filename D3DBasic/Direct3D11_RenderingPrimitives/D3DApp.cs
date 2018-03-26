using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

using Common;

// Resolve class name conflicts by explicitly stating
// which class they refer to:
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Direct3D11_RenderingPrimitives
{
    class D3DApp : D3DApplicationDesktop
    {
        ShaderBytecode vertexShaderBytecode;
        VertexShader vertexShader;

        ShaderBytecode pixelShaderBytecode;
        PixelShader pixelShader;

        InputLayout vertexLayout;

        Buffer worldViewProjectionBuffer;

        DepthStencilState depthStencilState;

        public D3DApp(System.Windows.Forms.Form window)
            : base(window)
        {

        }

        public override void Run()
        {
            #region Create renderers

            var axisLines = ToDispose(new AxisLinesRenderer());
            axisLines.Initialize(this);

            var triangle = ToDispose(new TriangleRenderer());
            triangle.Initialize(this);

            var quad = ToDispose(new QuadRenderer());
            quad.Initialize(this);

            #endregion

            #region Init MVP
            // Initialize the world matrix
            var worldMatrix = Matrix.Identity;

            // Set the camera position slightly to the right (x), above (y) and behind (-z)
            var cameraPosition = new Vector3(1, 1, -2);
            var cameraTarget = Vector3.Zero; // Looking at the origin 0,0,0
            var cameraUp = Vector3.UnitY; // Y+ is Up

            // Prepare matrices
            // Create the view matrix from our camera position, look target and up direction
            var viewMatrix = Matrix.LookAtLH(cameraPosition, cameraTarget, cameraUp);

            // Create the projection matrix
            /* FoV 60degrees = Pi/3 radians */
            // Aspect ratio (based on window size), Near clip, Far clip
            var projectionMatrix = Matrix.PerspectiveFovLH((float)Math.PI / 3f, Width / (float)Height, 0.5f, 100f);

            // Maintain the correct aspect ratio on resize
            Window.Resize += (s, e) =>
            {
                projectionMatrix = Matrix.PerspectiveFovLH((float)Math.PI / 3f, Width / (float)Height, 0.5f, 100f);
            };
            #endregion

            #region Render loop
            SharpDX.Windows.RenderLoop.Run(Window, () =>
            {
                var context = DeviceManager.Direct3DContext;

                // Clear depth stencil view
                context.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
                // Clear render target view
                context.ClearRenderTargetView(RenderTargetView, Color.White);

                // Create viewProjection matrix
                var viewProjection = Matrix.Multiply(viewMatrix, projectionMatrix);

                // Create WorldViewProjection Matrix
                var worldViewProjection = worldMatrix * viewProjection;
                // HLSL uses "column major" matrices so transpose from "row major" first
                worldViewProjection.Transpose();
                // Write the worldViewProjection to the constant buffer
                context.UpdateSubresource(ref worldViewProjection, worldViewProjectionBuffer);

                // Render the primitives
                axisLines.Render();
                triangle.Render();
                quad.Render();

                Present();
            });
            #endregion
        }

        // Override / extend the default SwaoChainDescription1
        protected override SwapChainDescription1 CreateSwapChainDescription()
        {
            return base.CreateSwapChainDescription();
        }

        // Event-handler for DeviceManager.OnInitialize
        protected override void CreateDeviceDependentResources(DeviceManager deviceManager)
        {
            base.CreateDeviceDependentResources(deviceManager);

            // Release all resources
            RemoveAndDispose(ref vertexShader);
            RemoveAndDispose(ref vertexShaderBytecode);
            RemoveAndDispose(ref pixelShader);
            RemoveAndDispose(ref pixelShaderBytecode);
            RemoveAndDispose(ref vertexLayout);
            RemoveAndDispose(ref worldViewProjectionBuffer);
            RemoveAndDispose(ref depthStencilState);

            // Get a reference to the Device1 instance and context
            var device = deviceManager.Direct3DDevice;
            var context = deviceManager.Direct3DContext;

            ShaderFlags shaderFlags = ShaderFlags.None;
#if DEBUG
            shaderFlags = ShaderFlags.Debug;
#endif
            // compile shader
            vertexShaderBytecode = ToDispose(ShaderBytecode.CompileFromFile("Primitive.hlsl", "VSMain", "vs_5_0", shaderFlags));
            vertexShader = ToDispose(new VertexShader(device, vertexShaderBytecode));

            pixelShaderBytecode = ToDispose(ShaderBytecode.CompileFromFile("Primitive.hlsl", "PSMain", "ps_5_0", shaderFlags));
            pixelShader = ToDispose(new PixelShader(device, pixelShaderBytecode));

            // vertex input signature
            vertexLayout = ToDispose(new InputLayout(device, 
                vertexShaderBytecode.GetPart(ShaderBytecodePart.InputSignatureBlob),
                new[]
                {
                    new InputElement("SV_Position", 0, Format.R32G32B32A32_Float, 0, 0),
                    new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
                }));

            // mvp matrix
            worldViewProjectionBuffer = ToDispose(new SharpDX.Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0));

            // depth & stencil
            depthStencilState = ToDispose(new DepthStencilState(device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = true,
                    DepthComparison = Comparison.Less,
                    DepthWriteMask = SharpDX.Direct3D11.DepthWriteMask.All,
                    IsStencilEnabled = false,
                    StencilReadMask = 0xff, // 0xff(no mask)
                    StencilWriteMask = 0xff, // 0xf(no mask)

                    FrontFace = new DepthStencilOperationDescription()
                    {
                        Comparison = Comparison.Always,
                        PassOperation = StencilOperation.Keep,
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Increment
                    },

                    BackFace = new DepthStencilOperationDescription()
                    {
                        Comparison = Comparison.Always,
                        PassOperation = StencilOperation.Keep,
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Decrement
                    }

                }));

            // assign input layout, constant buffer, vertex and pixel shaders and depth stencil state to appropriate graphics pipeline stages
            context.InputAssembler.InputLayout = vertexLayout;
            context.VertexShader.SetConstantBuffer(0, worldViewProjectionBuffer);
            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);
            context.OutputMerger.DepthStencilState = depthStencilState;
        }

        // Event-handler for D3DApplicationBase.OnSizeChanged
        protected override void CreateSizeDependentResources(D3DApplicationBase app)
        {
            base.CreateSizeDependentResources(app);
        }
    }
}
