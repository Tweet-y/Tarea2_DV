using System;
using UnityEngine;

namespace Synty.Tools.SyntyPropBoneTool
{
    [Serializable]
    public class PropBoneBinding
    {
        public Transform bone;
        public Transform socket;
        public Vector3 rotationOffset;
        public float scale = 1f;

        public bool IsValid => bone != null && socket != null;

        public bool IsMatch(PropBoneDefinition definition)
        {
            if (definition == null || bone == null || socket == null || bone.parent == null)
            {
                return false;
            }

            return bone.name == definition.boneName
                && socket.name == definition.socketName
                && bone.parent.name == definition.parentBoneName;
        }
    }
}
