using UnityEngine;

namespace Aerolt.Helpers
{
    public static class Extentions
    {
        public static void SetZ(this Transform tran, float z)
        {
            var vec = tran.position;
            vec.z = z;
            tran.position = vec;
        }
    }
}
