using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTKAPI.Objects.SkillHooks
{
    internal class BaseModule
    {
        public virtual void Initialize() { }
        public virtual void Unload() { }
    }
}
