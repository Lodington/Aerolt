using System.Collections.Generic;
using RoR2;
using RoR2.ContentManagement;

namespace Aerolt.Classes
{
    public class Items
    {
        public List<string> ItemStrings = new List<string>();

        public void getItemNames()
        {
            foreach (var def in ContentManager._itemDefs)
            {
                if (def == null || def.nameToken == null) continue;
                ItemStrings.Add(Language.GetString(def.nameToken));
            }
        }

    }
}
