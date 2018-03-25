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

namespace D3DBasic
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
            form.Text = "D3DRendering";
            form.Width = 800;
            form.Height = 600;

            // device & swapChain
            Device device;
            SwapChain swapChain;

            // create device & swapChain
            Device.CreateWithSwapChain(
                SharpDX.Direct3D.DriverType.Hardware,
                DeviceCreationFlags.None,
                new[]
                {
                    SharpDX.Direct3D.FeatureLevel.Level_11_1,
                    SharpDX.Direct3D.FeatureLevel.Level_11_0
                },
                new SwapChainDescription()
                {
                    ModeDescription = new ModeDescription(form.ClientSize.Width, form.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = SharpDX.DXGI.Usage.BackBuffer | SharpDX.DXGI.Usage.RenderTargetOutput,
                    BufferCount = 1,
                    Flags = SwapChainFlags.None,
                    IsWindowed = true,
                    OutputHandle = form.Handle,
                    SwapEffect = SwapEffect.Discard,
                },
                out device, out swapChain
            );

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
                    form.Text = string.Format("D3DRendering - FPS: {0:F2} ({1:F2}ms/frame)", fps, (float)fpsTimer.ElapsedMilliseconds / fpsFrames);

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
                swapChain.Present(0, PresentFlags.None);

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
