using System;
using System.Collections.Generic;
using System.Text;

namespace Lavender.FurnitureLib
{
    public class FurnitureAssetData
    {
        /// <summary>
        /// Furniture Image PNG/JPG filepath relativ to the json file
        /// </summary>
        public string imagePath;

        /// <summary>
        /// OBJ filepath relativ to the json file
        /// </summary>
        public string objPath;

        /// <summary>
        /// PNG/JPG filepath(s) relativ to the json file
        /// </summary>
        public string[] texturePaths;
    }
}
