using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Google2u;
using FTKAPI.Utils;
namespace FTKAPI.APIs.BattleAPI
{
    public class APIuiToolTipFocusable : uiToolTipFocusable, IToolTipInfo
    {
        
        public override string GetToolTip()
        {
            if (m_ReturnRawInfo)
            {
                return m_Info;
            }
            return FTKHub.LocalizeString(m_LocalizationTable.ToString(), m_Info);
        }

        public override Sprite GetSprite()
        {
            return gCanFocus ? FTKInput.Instance.m_FocusGraphics.GetSprite() : null;
        }

        public override Sprite GetMouseSprite()
        {
            FTKInput.RemapKeyInfo remapKeyInfo = FTKInput.Instance.GetRemapKeyInfo("Focus");
            if (gCanFocus && remapKeyInfo != null && remapKeyInfo.IsAnyPosKey(KeyCode.Mouse1))
            {
                return FTKInput.Instance.m_FocusGraphics.m_MouseSprite;
            }
            return null;
        }

        public override bool IsShowToolTip()
        {
            return true;
        }

        public override bool IsFocus()
        {
            return true;
        }

        public override uiPoiButton GetPoiButton()
        {
            return GetComponent<uiPoiButton>();
        }

        public override uiBattleButton GetBattleButton()
        {
            return GetComponent<uiBattleButton>();
        }
    }
}
