using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

using Common;

// Resolve class name conflicts by explicitly stating
// which class they refer to:
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Direct3D11_RenderingPrimitivesInput
{
    class D3DApp : D3DApplicationDesktop
    {
        ShaderBytecode vertexShaderBytecode;
        VertexShader vertexShader;

        ShaderBytecode pixelShaderBytecode;
        PixelShader pixelShader;

        ShaderBytecode depthVertexShaderBytecode;
        VertexShader depthVertexShader;

        ShaderBytecode depthPixelShaderBytecode;
        PixelShader depthPixelShader;

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

            // Create and initialize a Direct2D FPS text renderer
            var fps = ToDispose(new Common.FpsRenderer("Calibri", Color.CornflowerBlue, new Point(8, 8), 16));
            fps.Initialize(this);

            // Create and initialize a general purpose Direct2D text renderer
            // This will display some instructions and the current view and rotation offsets
            var textRenderer = ToDispose(new Common.TextRenderer("Calibri", Color.CornflowerBlue, new Point(8, 30), 12));
            textRenderer.Initialize(this);

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

            #region Rotation and window event handlers
            // Create a rotation vector to keep track of the rotation
            // around each of the axes
            var rotation = new Vector3(0.0f, 0.0f, 0.0f);

            // We will call this action to update text
            // for the text renderer
            Action updateText = () =>
            {
                textRenderer.Text =
                    String.Format("World rotation ({0}) (Up/Down Left/Right Wheel+-)\nView ({1}) (A/D, W/S, Shift+Wheel+-)"
                    + "\nPress X to reinitialize the device and resources (device ptr: {2})"
                    + "\nPress Z to show/hide depth buffer",
                        rotation,
                        viewMatrix.TranslationVector,
                        DeviceManager.Direct3DDevice.NativePointer);
            };

            bool useDepthShaders = false;

            // Support keyboard/mouse input to rotate or move camera view
            var moveFactor = 0.02f; // how much to change on each keypress
            var shiftKey = false;
            var ctrlKey = false;
            Window.KeyDown += (s, e) =>
            {
                shiftKey = e.Shift;
                ctrlKey = e.Control;

                switch (e.KeyCode)
                {
                    // WASD -> pans view
                    case Keys.A:
                        viewMatrix.TranslationVector += new Vector3(moveFactor * 2, 0f, 0f);
                        break;
                    case Keys.D:
                        viewMatrix.TranslationVector -= new Vector3(moveFactor * 2, 0f, 0f);
                        break;
                    case Keys.S:
                        if (shiftKey)
                            viewMatrix.TranslationVector += new Vector3(0f, moveFactor * 2, 0f);
                        else
                            viewMatrix.TranslationVector += new Vector3(0f, 0f, 1) * moveFactor * 2;
                        break;
                    case Keys.W:
                        if (shiftKey)
                            viewMatrix.TranslationVector -= new Vector3(0f, moveFactor * 2, 0f);
                        else
                            viewMatrix.TranslationVector -= new Vector3(0f, 0f, 1) * moveFactor * 2;
                        break;
                    // Up/Down and Left/Right - rotates around X / Y respectively
                    // (Mouse wheel rotates around Z)
                    case Keys.Down:
                        worldMatrix *= Matrix.RotationX(-moveFactor);
                        rotation -= new Vector3(moveFactor, 0f, 0f);
                        break;
                    case Keys.Up:
                        worldMatrix *= Matrix.RotationX(moveFactor);
                        rotation += new Vector3(moveFactor, 0f, 0f);
                        break;
                    case Keys.Left:
                        worldMatrix *= Matrix.RotationY(-moveFactor);
                        rotation -= new Vector3(0f, moveFactor, 0f);
                        break;
                    case Keys.Right:
                        worldMatrix *= Matrix.RotationY(moveFactor);
                        rotation += new Vector3(0f, moveFactor, 0f);
                        break;

                    case Keys.X:
                        // To test for correct resource recreation
                        // Simulate device reset or lost.
                        System.Diagnostics.Debug.WriteLine(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());
                        DeviceManager.Initialize(DeviceManager.Dpi);
                        System.Diagnostics.Debug.WriteLine(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());
                        break;
                    case Keys.Z:
                        var context = DeviceManager.Direct3DContext;
                        useDepthShaders = !useDepthShaders;
                        if (useDepthShaders)
                        {
                            context.VertexShader.Set(depthVertexShader);
                            context.PixelShader.Set(depthPixelShader);
                        }
                        else
                        {
                            context.VertexShader.Set(vertexShader);
                            context.PixelShader.Set(pixelShader);
                        }
                        break;
                }

                updateText();
            };
            Window.KeyUp += (s, e) =>
            {
                // Clear the shift/ctrl keys so they aren't sticky
                if (e.KeyCode == Keys.ShiftKey)
                    shiftKey = false;
                if (e.KeyCode == Keys.ControlKey)
                    ctrlKey = false;
            };
            Window.MouseWheel += (s, e) =>
            {
                if (shiftKey)
                {
                    // Zoom in/out
                    viewMatrix.TranslationVector -= new Vector3(0f, 0f, (e.Delta / 120f) * moveFactor * 2);
                }
                else
                {
                    // rotate around Z-axis
                    viewMatrix *= Matrix.RotationZ((e.Delta / 120f) * moveFactor);
                    rotation += new Vector3(0f, 0f, (e.Delta / 120f) * moveFactor);
                }
                updateText();
            };

            var lastX = 0;
            var lastY = 0;

            Window.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    lastX = e.X;
                    lastY = e.Y;
                }
            };

            Window.MouseMove += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    var yRotate = lastX - e.X;
                    var xRotate = lastY - e.Y;
                    lastY = e.Y;
                    lastX = e.X;

                    // Mouse move changes 
                    viewMatrix *= Matrix.RotationX(xRotate * moveFactor);
                    viewMatrix *= Matrix.RotationY(yRotate * moveFactor);

                    updateText();
                }
            };

            // Display instructions with initial values
            updateText();

            #endregion

            var clock = new System.Diagnostics.Stopwatch();
            clock.Start();

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

                // Render FPS
                fps.Render();

                // Render instructions + position changes
                textRenderer.Render();

                Present();
            });
            #endregion
        }

        // Override / extend the default SwaoChainDescription1
        protected override SwapChainDescription1 CreateSwapChainDescription()
        {
            var description = base.CreateSwapChainDescription();
            description.SampleDescription.Count = 4;
            description.SampleDescription.Quality = 0;
            return description;
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

            RemoveAndDispose(ref depthVertexShader);
            RemoveAndDispose(ref depthVertexShaderBytecode);
            RemoveAndDispose(ref depthPixelShader);
            RemoveAndDispose(ref depthPixelShaderBytecode);

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

            depthVertexShaderBytecode = ToDispose(ShaderBytecode.CompileFromFile("Depth.hlsl", "VSMain", "vs_5_0", shaderFlags));
            depthVertexShader = ToDispose(new VertexShader(device, depthVertexShaderBytecode));
            depthPixelShaderBytecode = ToDispose(ShaderBytecode.CompileFromFile("Depth.hlsl", "PSMain", "ps_5_0", shaderFlags));
            depthPixelShader = ToDispose(new PixelShader(device, depthPixelShaderBytecode));

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
