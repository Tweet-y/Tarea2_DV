using System;
using StarterAssets;
using UnityEngine;

// CJ's FBX has a segmented rig, not a valid Humanoid skin. Animate its original
// joints and deform only its unskinned sleeves, without retargeting the avatar.
[DisallowMultipleComponent]
public sealed class CJLocomotionAnimation : MonoBehaviour
{
    sealed class Joint
    {
        public Transform transform;
        public Quaternion rotation;
        public Joint(Transform t) { transform = t; rotation = t.localRotation; }
        public void Bend(float angle, Vector3 axis)
        {
            transform.localRotation = Quaternion.AngleAxis(angle,
                transform.parent.InverseTransformDirection(axis)) * rotation;
        }
    }

    sealed class Arm
    {
        public Vector3 shoulder, elbow, hand;
        public Transform handBone;
        public Quaternion handRotation;
    }

    ThirdPersonController controller;
    Joint leftLeg, rightLeg, leftKnee, rightKnee, leftFoot, rightFoot;
    Arm leftArm, rightArm;
    Transform torso, head, hips, player;
    Mesh torsoMesh;
    Vector3[] restVertices, vertices;
    Vector3 restPosition, headPosition, hipPosition, previousPosition;
    Vector3 leftSole, rightSole;
    float originalSoleHeight, speed, phase, airborne;
    bool initialized;

    public void Initialize()
    {
        if (initialized) return;
        player = transform.parent;
        controller = GetComponentInParent<ThirdPersonController>();
        var animator = GetComponent<Animator>();
        if (animator != null) animator.enabled = false;
        Transform Find(string name)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
            throw new InvalidOperationException("Falta un hueso de CJ: " + name);
        }
        leftLeg = new Joint(Find("PIERNA_IZQ"));
        rightLeg = new Joint(Find("PIERNA_DER"));
        leftKnee = new Joint(Find("Junta_1_3"));
        rightKnee = new Joint(Find("Junta_1_2"));
        leftFoot = new Joint(Find("PIE_IZQ"));
        rightFoot = new Joint(Find("PIE_DER"));
        torso = Find("TORSO");
        head = Find("CABEZA");
        hips = Find("Cadera");
        restPosition = transform.localPosition;
        headPosition = head.localPosition;
        hipPosition = hips.localPosition;
        previousPosition = player.position;
        Arm MakeArm(string shoulder, string elbow, string hand)
        {
            var bone = Find(hand);
            return new Arm { shoulder = torso.InverseTransformPoint(Find(shoulder).position),
                elbow = torso.InverseTransformPoint(Find(elbow).position),
                hand = torso.InverseTransformPoint(bone.position), handBone = bone,
                handRotation = Quaternion.Inverse(player.rotation) * bone.rotation };
        }
        leftArm = MakeArm("Junta_1_5", "Junta_2_3", "MANO_IZQ");
        rightArm = MakeArm("Junta_1_4", "Junta_2_2", "MANO_DER");
        torsoMesh = Instantiate(torso.GetComponent<MeshFilter>().sharedMesh);
        torsoMesh.name = "CJ animated torso (instance)";
        torsoMesh.MarkDynamic();
        torso.GetComponent<MeshFilter>().sharedMesh = torsoMesh;
        restVertices = torsoMesh.vertices;
        vertices = new Vector3[restVertices.Length];
        leftSole = leftFoot.transform.InverseTransformPoint(leftFoot.transform.position - player.up * 0.1f);
        rightSole = rightFoot.transform.InverseTransformPoint(rightFoot.transform.position - player.up * 0.1f);
        originalSoleHeight = Mathf.Min(player.InverseTransformPoint(leftFoot.transform.TransformPoint(leftSole)).y,
            player.InverseTransformPoint(rightFoot.transform.TransformPoint(rightSole)).y);
        initialized = true;
    }

    void Awake() { Initialize(); }

    void LateUpdate()
    {
        if (!initialized) return;
        var delta = player.position - previousPosition;
        previousPosition = player.position;
        var targetSpeed = new Vector2(delta.x, delta.z).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        speed = Mathf.Lerp(speed, Mathf.Min(targetSpeed, 7f), 1f - Mathf.Exp(-12f * Time.deltaTime));
        var grounded = controller == null || controller.Grounded;
        airborne = Mathf.MoveTowards(airborne, grounded ? 0f : 1f, Time.deltaTime * 8f);
        if (speed > 0.05f) phase += Time.deltaTime * Mathf.Lerp(5f, 11f, speed / 5.335f);
        ApplyPose(speed, phase, airborne);
    }

    // Also used by the editor preview to verify the exact gameplay poses.
    public void ApplyPose(float movementSpeed, float cycle, float jumpBlend)
    {
        Initialize();
        transform.localPosition = restPosition;
        var motion = Mathf.Clamp01(movementSpeed / 2f) * (1f - jumpBlend);
        var run = Mathf.Clamp01((movementSpeed - 2f) / 3.335f);
        var swing = Mathf.Sin(cycle) * Mathf.Lerp(18f, 30f, run) * motion;
        var leftBend = Mathf.Max(0f, -Mathf.Cos(cycle)) * Mathf.Lerp(22f, 40f, run) * motion;
        var rightBend = Mathf.Max(0f, Mathf.Cos(cycle)) * Mathf.Lerp(22f, 40f, run) * motion;
        var axis = player.right;
        leftLeg.Bend(swing - jumpBlend * 12f, axis);
        rightLeg.Bend(-swing - jumpBlend * 12f, axis);
        leftKnee.Bend(-leftBend - jumpBlend * 25f, axis);
        rightKnee.Bend(-rightBend - jumpBlend * 25f, axis);
        // Keep the shoes in the original ankle pose so their skin stays joined
        // to the trouser cuffs while the knee and hip joints move.
        leftFoot.Bend(0f, axis);
        rightFoot.Bend(0f, axis);
        var breath = Mathf.Sin(Time.time * 2f) * 0.002f * (1f - motion);
        hips.localPosition = hipPosition + Vector3.up * breath;
        head.localPosition = headPosition + Vector3.up * breath;
        var armSwing = swing * 0.7f;
        var leftRotation = Quaternion.AngleAxis(-armSwing - jumpBlend * 18f, Vector3.right);
        var rightRotation = Quaternion.AngleAxis(armSwing - jumpBlend * 18f, Vector3.right);
        var elbowRotation = Quaternion.AngleAxis(run * 12f * motion + jumpBlend * 12f, Vector3.right);
        Vector3 Map(Arm arm, Quaternion rotation, Vector3 p, float bendWeight)
        {
            var upper = arm.shoulder + rotation * (p - arm.shoulder);
            var lower = arm.shoulder + rotation * (arm.elbow - arm.shoulder + elbowRotation * (p - arm.elbow));
            return Vector3.Lerp(upper, lower, bendWeight);
        }
        for (var i = 0; i < restVertices.Length; i++)
        {
            var p = restVertices[i];
            var arm = p.x >= 0f ? leftArm : rightArm;
            var rotation = p.x >= 0f ? leftRotation : rightRotation;
            var weight = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.15f, 0.22f, Mathf.Abs(p.x)));
            weight *= 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(arm.shoulder.y, arm.shoulder.y + 0.12f, p.y));
            var lowerWeight = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(arm.elbow.y + 0.05f, arm.elbow.y - 0.05f, p.y));
            vertices[i] = Vector3.Lerp(p, Map(arm, rotation, p, lowerWeight), weight);
        }
        torsoMesh.vertices = vertices;
        torsoMesh.RecalculateNormals();
        torsoMesh.RecalculateBounds();
        void MoveHand(Arm arm, Quaternion rotation)
        {
            arm.handBone.position = torso.TransformPoint(Map(arm, rotation, arm.hand, 1f));
            arm.handBone.rotation = player.rotation * rotation * elbowRotation * arm.handRotation;
        }
        MoveHand(leftArm, leftRotation);
        MoveHand(rightArm, rightRotation);
        if (jumpBlend < 0.1f)
        {
            var height = Mathf.Min(player.InverseTransformPoint(leftFoot.transform.TransformPoint(leftSole)).y,
                player.InverseTransformPoint(rightFoot.transform.TransformPoint(rightSole)).y);
            transform.localPosition += Vector3.up * (originalSoleHeight - height);
        }
    }

    void OnDestroy()
    {
        if (torsoMesh != null)
        {
            if (Application.isPlaying) Destroy(torsoMesh);
            else DestroyImmediate(torsoMesh);
        }
    }
}
