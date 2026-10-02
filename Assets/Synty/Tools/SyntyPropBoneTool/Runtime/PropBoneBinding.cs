<<<<<<< HEAD
=======
// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement

>>>>>>> origin/vicente
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

<<<<<<< HEAD
        public bool IsValid => bone != null && socket != null;

        public bool IsMatch(PropBoneDefinition definition)
        {
            if (definition == null || bone == null || socket == null || bone.parent == null)
=======
        public bool IsValid => bone != null && socket != null && bone.parent != null;

        public bool IsMatch(PropBoneDefinition definition)
        {
            if (definition == null || bone == null || socket == null)
>>>>>>> origin/vicente
            {
                return false;
            }

<<<<<<< HEAD
            return bone.name == definition.boneName
                && socket.name == definition.socketName
                && bone.parent.name == definition.parentBoneName;
=======
            return bone.name == definition.boneName &&
                   socket.name == definition.socketName &&
                   bone.parent != null &&
                   bone.parent.name == definition.parentBoneName;
>>>>>>> origin/vicente
        }
    }
}
