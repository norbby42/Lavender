using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Lavender.RecipeLib
{
    public class ModifierInfo
    {
        public int id;

        public string TooltipTitel;

        public string TooltipDetails;

        public Sprite Image;

        public ModifierInfo(int ID, string tooltipTitel, string tooltipDetails, Sprite image)
        {
            id = ID;
            TooltipTitel = tooltipTitel;
            TooltipDetails = tooltipDetails;
            Image = image;
        }
    }
}
