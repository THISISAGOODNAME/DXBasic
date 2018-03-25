using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

// Resolve class name conflicts by explicitly stating which class they refer to:
using Device = SharpDX.Direct3D11.Device;
using Device2 = SharpDX.Direct3D11.Device2;

namespace Direct3D11_2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            #region d3d initialization
            // create winform
            Form1 form = new Form1();
            form.Text = "D3D11.2Rendering";
            form.Width = 800;
            form.Height = 600;

            // device & swapChain
            Device2 device;
            SwapChain1 swapChain;

            // First create a regular D3D11 device
            using (
                var device11 = new Device(
                    SharpDX.Direct3D.DriverType.Hardware,
                    DeviceCreationFlags.None,
                    new[] {
                        SharpDX.Direct3D.FeatureLevel.Level_11_1,
                        SharpDX.Direct3D.FeatureLevel.Level_11_0,
                    }))
            {
                // Query device for the Device1 interface (ID3D11Device1)
                device = device11.QueryInterfaceOrNull<Device2>();

                if (device == null)
                    throw new NotSupportedException("SharpDX.Direct3D11.Device2 is not supported");
            }


            // Rather than create a new DXGI Factory we should reuse
            // the one that has been used internally to create the device
            using (var dxgi = device.QueryInterface<SharpDX.DXGI.Device2>())
            using (var adapter = dxgi.Adapter)
            using (var factory = adapter.GetParent<Factory2>())
            {
                var desc1 = new SwapChainDescription1()
                {
                    Width = form.ClientSize.Width,
                    Height = form.ClientSize.Height,
                    Format = Format.R8G8B8A8_UNorm,
                    Stereo = false,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = Usage.BackBuffer | Usage.RenderTargetOutput,
                    BufferCount = 1,
                    Scaling = Scaling.Stretch,
                    SwapEffect = SwapEffect.Discard,
                };

                swapChain = new SwapChain1(factory,
                    device,
                    form.Handle,
                    ref desc1,
                    new SwapChainFullScreenDescription()
                    {
                        RefreshRate = new Rational(60, 1),
                        Scaling = DisplayModeScaling.Centered,
                        Windowed = true
                    },
                    // Restrict output to specific Output (monitor)
                    null);

                swapChain.QueryInterfaceOrNull<SwapChain2>();
            }

            // Create references to backBuffer and renderTargetView
            var backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            var renderTargetView = new RenderTargetView(device, backBuffer);
            #endregion

            #region render loop

            // Create Clock and FPS counters
            var clock = new System.Diagnostics.Stopwatch();
            var clockFrequency = (double)System.Diagnostics.Stopwatch.Frequency;
            clock.Start();
            var deltaTime = 0.0;
            var fpsTimer = new System.Diagnostics.Stopwatch();
            fpsTimer.Start();
            var fps = 0.0;
            int fpsFrames = 0;

            RenderLoop.Run(form, () =>
            {
                // Time in seconds
                var totalSeconds = clock.ElapsedTicks / clockFrequency;

                #region FPS and title update
                fpsFrames++;
                if (fpsTimer.ElapsedMilliseconds > 1000)
                {
                    fps = 1000.0 * fpsFrames / fpsTimer.ElapsedMilliseconds;

                    // Update window title with FPS once every second
                    form.Text = string.Format("D3D11.2Rendering - FPS: {0:F2} ({1:F2}ms/frame)", fps, (float)fpsTimer.ElapsedMilliseconds / fpsFrames);

                    // Restart the FPS counter
                    fpsTimer.Reset();
                    fpsTimer.Start();
                    fpsFrames = 0;
                }
                #endregion

                // clearcolor
                var lerpColor = SharpDX.Color.Lerp(Color.LightBlue, Color.DarkBlue, (float)(Math.Sin(totalSeconds) / 2.0 + 0.5));

                device.ImmediateContext.ClearRenderTargetView(renderTargetView, lerpColor);

                // rendering commands

                // Present the frams
                swapChain.Present(0, PresentFlags.None, new PresentParameters());

                // Determine the time it took to render the frame
                deltaTime = (clock.ElapsedTicks / clockFrequency) - totalSeconds;
            });
            #endregion

            #region d3d cleanup
            // release
            renderTargetView.Dispose();
            backBuffer.Dispose();
            device.Dispose();
            swapChain.Dispose();
            #endregion
        }
    }
}
