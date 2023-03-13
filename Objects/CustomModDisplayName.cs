using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static GameDefinitionBase;

namespace FTKAPI.Objects
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class CustomModDisplayName : ModDisplayName
    {
        public CustomModDisplayName(string _display, string _toolTip, ModType _modtype, bool _percent = false) : base(_display, _toolTip, _modtype, _percent)
        {
            m_DisplayName = _display;
            m_ToolTip = _toolTip;
        }
    }
}
