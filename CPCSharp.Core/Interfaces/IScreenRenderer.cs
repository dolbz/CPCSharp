using System.Drawing;
namespace CPCSharp.Core.Interfaces {
    public interface IScreenRenderer {
        void SendPixels(Color[] pixels);
        void SendHsyncStart();
        void SendHsyncEnd();
        void SendVsyncStart();
        void SendVsyncEnd();
    }
}