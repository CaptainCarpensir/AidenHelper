using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.AidenHelper.Module
{
    public class AidenHelperSession : EverestModuleSession
    {
        public bool InvertStaminaOnDashEnabled { get; set; } = false;

        public int RunAndGunMoveX { get; set; } = 0;
        public bool RunAndGunEnabled { get; set; } = false;
    }
}
