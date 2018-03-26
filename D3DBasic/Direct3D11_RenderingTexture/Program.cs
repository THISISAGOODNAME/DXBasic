using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Direct3D11_RenderingTexture
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
            // Enbale object tracking
            SharpDX.Configuration.EnableObjectTracking = true;
#endif

            // Create the surface
            var form = new Form1();
            form.Text = "Textures";
            form.ClientSize = new System.Drawing.Size(640, 480);
            form.Show();

            // Create and initializatize the D3DApp
            using (D3DApp app = new D3DApp(form))
            {
                app.VSync = true;
                app.Initialize();
                app.Run();
            }
        }
    }
}
