using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Direct3D11_VertexSkinning
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
            form.Text = "D3DRendering - Vertex Skinning";
            form.ClientSize = new System.Drawing.Size(640, 480);
            form.Show();
            // Create and initialize the new D3D application
            // Then run the application.
            using (D3DApp app = new D3DApp(form))
            {
                app.VSync = false;
                app.Initialize();
                app.Run();
            }
        }
    }
}
