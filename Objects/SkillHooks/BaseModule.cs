using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTKAPI.Objects.SkillHooks
{
    public class BaseModule
    {
        internal int prof;
        public virtual void Initialize() { }
        public virtual void Unload() { }

        public int Prof {
            get => prof;
            set => prof = value;
        }
    }
}
