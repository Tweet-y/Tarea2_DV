using System;
using UnityEngine;

namespace Synty.Tools.SyntyPropBoneTool
{
    [Serializable]
    public class PropBoneDefinition
    {
        public string parentBoneName;
        public string boneName;
        public string socketName;
        public Vector3 rotationOffset;
        public float scale = 1f;
        public string scaleCalculationBone1;
        public string scaleCalculationBone2;

        public override string ToString()
        {
            return string.IsNullOrEmpty(boneName) ? base.ToString() : boneName;
        }
    }
}
