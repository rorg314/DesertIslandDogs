using System.Collections.Generic;

namespace UnityEngine.Animations.Rigging
{
    public interface IRigEffectorHolder
    {
#if UNITY_EDITOR
        IEnumerable<RigEffectorData> effectors { get; }

        void AddEffector(Transform transform, RigEffectorData.Style style);
        void RemoveEffector(Transform transform);
        bool ContainsEffector(Transform transform);
#endif
    }
}
