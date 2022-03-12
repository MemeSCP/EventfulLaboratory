using System.Collections;
using System.Collections.Generic;
using EventfulLaboratory.Extension;
using EventfulLaboratory.structs;

namespace EventfulLaboratory.SLEvents
{
    internal sealed class Deathrun : AEvent
    {
        #region Events
        public override void OnNewRound()
        {
            SpawnStartingPlatform();
        }
        
        #endregion
        
        #region Helpers

        private void SpawnStartingPlatform()
        {
            //KDQxLjIsIDEuMywgNTMuNCklSENaIEJyZWFrYWJsZURvb3IoQ2xvbmUpfCgtMTMuMiwgMTEyNy41LCAtNDkuMSl8KDAuNSwgMC41LCAtMC41LCAwLjUpfCgxMC4wLCA1LjAsIDEuMCklSENaIEJyZWFrYWJsZURvb3IoQ2xvbmUpfCgtMTMuMiwgMTEyOS42LCAtNTguOSl8KDAuMCwgMC4wLCAtMC43LCAwLjcpfCgzLjAsIDUuMCwgMS4wKSVIQ1ogQnJlYWthYmxlRG9vcihDbG9uZSl8KDMuMCwgMTEyOS42LCAtMzkuNCl8KDAuMCwgMC4wLCAwLjcsIDAuNyl8KDMuMCwgNS4wLCAxLjAp
            var str = "KDQxLjIsIDEuMywgNTMuNCklSENaIEJyZWFrYWJsZURvb3IoQ2xvbmUpfCgtMTIuOCwgMTEyNi43LCAtNTQuMil8KDAuMCwgMC4wLCAwLjAsIDEuMCl8KDcuMCwgMS4wLCAxLjAp".FromBase64();
            var parts = str.Split('%');
            for (int i = 1; i < parts.Length; i++)
            {
                var part = parts[i].Split('|');
                Util.BuilderUtil.HandleSpawning(
                    part[0],
                    part[1].ParseVec3(),
                    part[2].ParseQuat(),
                    part[3].ParseVec3()
                );
            }
        }
        
        #endregion
    }
}