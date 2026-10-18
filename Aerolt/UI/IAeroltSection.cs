using UnityEngine;

namespace Aerolt.UI
{
    public interface IAeroltSection
    {
        string Title { get; }
        GameObject Root { get; }
        void Build(GameObject contentParent);
        void OnShown();
    }
}
