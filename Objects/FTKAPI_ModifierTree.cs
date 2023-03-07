using GridEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTKAPI.Objects
{
    public class FTKAPI_ModifierTree
    {
        internal Dictionary<FTKAPI_ModifierTree.Milestone, FTK_characterModifier.ID> m_Dictionary;

        public enum Milestone
        {
            None = -1
        }
        
        public Dictionary<FTKAPI_ModifierTree.Milestone, FTK_characterModifier.ID> Levels
        {
            get => m_Dictionary;
            set => m_Dictionary = value;
        }
    }
}
