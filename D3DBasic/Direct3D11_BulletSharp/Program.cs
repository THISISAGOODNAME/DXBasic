using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Direct3D11_BulletSharp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if DEBUG
            // Enable object tracking
            SharpDX.Configuration.EnableObjectTracking = true;
#endif
            // Create the form to render to
            var form = new Form1();
            form.Text = "D3DRendering - Physics Simulation";
            form.ClientSize = new System.Drawing.Size(640, 480);
            form.Show();
            // Create and initialize the new D3D application
            // Then run the application.
            using (D3DApp app = new D3DApp(form))
            {
                // Only render frames at the maximum rate the
                // display device can handle.
                app.VSync = true;

                // Initialize the framework (creates Direct3D device etc)
                app.Initialize();

                // Run the application
                app.Run();
            }
        }
    }
}
