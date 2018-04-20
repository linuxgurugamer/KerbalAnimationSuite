using UnityEngine;
using ToolbarControl_NS;

namespace KerbalAnimation
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(KerbalAnimationSuite.MODID, KerbalAnimationSuite.MODNAME);
        }
    }
}