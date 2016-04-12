
namespace Vlc.DotNet.Forms
{
    public partial class VlcControl
    {
        public string VlcVersion
        {
            get
            {
                var vlcVersion = "Unknown Version";
                if (myVlcMediaPlayer != null && myVlcMediaPlayer.Manager != null)
                    vlcVersion = myVlcMediaPlayer.Manager.VlcVersion;
                return vlcVersion;
            }
        }
    }
}
