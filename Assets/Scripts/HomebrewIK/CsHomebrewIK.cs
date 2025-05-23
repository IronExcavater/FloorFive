/*
 * Created :    Spring 2022
 * Author :     SeungGeon Kim (keithrek@hanmail.net)
 * Project :    HomebrewIK
 * Filename :   CsHomebrewIK.cs (non-static monobehaviour module)
 * 
 * All Content (C) 2022 Unlimited Fischl Works, all rights reserved.
 */



using System;
using Animation;
using Audio;
using Level; // Convert
using UnityEngine;  // Monobehaviour
using UnityEditor;
using Random = UnityEngine.Random; // Handles



namespace HomebrewIK
{
    public class CsHomebrewIK : MonoBehaviour
    {
        public AudioSource leftFootAudioSource;
        public AudioSource rightFootAudioSource;

        private bool _mute;
        public bool Mute
        {
            get => _mute;
            set
            {
                _mute = value;
                leftFootAudioSource.mute = _mute;
                rightFootAudioSource.mute = _mute;
            }
        }
        
        public bool enableFootLocking = true;
        public Action<bool> OnStep; // bool => isRightFoot
        
        private enum SteppingFoot { None, Left, Right }
        private SteppingFoot stepping = SteppingFoot.None;
        private SteppingFoot prevStepping = SteppingFoot.None;
        
        private bool prevLeftGrounded;
        private bool prevRightGrounded;

        private float leftStepProgress;
        private float rightStepProgress;

        public float stepDuration = 0.2f;
        public float stepLiftHeight = 0.1f;

        private bool previousMoving;
        private float previousYaw;
        private SteppingFoot prevTurn = SteppingFoot.None;
        
        public float maxLockAngleDiff = 30;
        public float maxLockDistance = 1;

        private AnimatorCache aniCache;
        
        private Quaternion leftFootIKSourceRotationTarget;
        private Quaternion leftFootIKSourceRotationBuffer;
        
        private Quaternion rightFootIKSourceRotationTarget;
        private Quaternion rightFootIKSourceRotationBuffer;
        
        private Animator playerAnimator = null;

        private Transform leftFootTransform = null;
        private Transform rightFootTransform = null;

        private Transform leftFootOrientationReference = null;
        private Transform rightFootOrientationReference = null;

        private Vector3 initialForwardVector = new Vector3();

        private void StepSfx(bool isRightFoot)
        {
            var audioSource = isRightFoot ? rightFootAudioSource : leftFootAudioSource;
            Vector3 footPos = isRightFoot ? rightFootTransform.position : leftFootTransform.position;
            
            SurfaceType surfaceType = GetFootSurface(footPos);
            string sfxKey = surfaceType.ToString().ToLower() + "Step";
            
            var audioClip = AudioManager.AudioGroupDictionary.GetValue(sfxKey).GetRandomClip();
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(audioClip);
        }

        private SurfaceType GetFootSurface(Vector3 footPosition)
        {
            if (Physics.Raycast(footPosition + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 0.3f))
            {
                if (hit.collider.TryGetComponent(out Surface surface))
                {
                    return surface.surfaceType;
                }
            }
            return SurfaceType.Carpet;
        }

        private void OnEnable()
        {
            OnStep += StepSfx;
        }

        private void OnDisable()
        {
            OnStep -= StepSfx;
        }

        public float _LengthFromHeelToToes {
            get { return lengthFromHeelToToes; }
        }

        public float _RaySphereRadius {
            get { return raySphereRadius; }
        }

        public float _LeftFootProjectedAngle {
            get { return leftFootProjectedAngle; }
        }

        public float _RightFootProjectedAngle {
            get { return rightFootProjectedAngle; }
        }

        public Vector3 _LeftFootIKPositionTarget {
            get {
                if (Application.isPlaying == true)
                {
                    return leftFootIKPositionTarget;
                }
                else
                {
                    // This is being done because the IK target only gets updated during playmode
                    return new Vector3(0, GetAnkleHeight() + _WorldHeightOffset, 0);
                }
            }
        }

        public Vector3 _RightFootIKPositionTarget {
            get {
                if (Application.isPlaying == true)
                {
                    return rightFootIKPositionTarget;
                }
                else
                {
                    // This is being done because the IK target only gets updated during playmode
                    return new Vector3(0, GetAnkleHeight() + _WorldHeightOffset, 0);
                }
            }
        }

        public float _AnkleHeightOffset {
            get {
                return ankleHeightOffset;
            }
        }

        public float _WorldHeightOffset {
            get {
                if (giveWorldHeightOffset == true)
                {
                    return worldHeightOffset;
                }
                else
                {
                    return 0;
                }
            }
        }

        [BigHeader("Foot Properties")]

        [SerializeField]
        [Range(0, 0.25f)]
        private float lengthFromHeelToToes = 0.1f;
        [SerializeField]
        [Range(0, 60)]
        private float maxRotationAngle = 45;
        [SerializeField]
        [Range(-0.05f, 0.125f)]
        private float ankleHeightOffset = 0;

        [BigHeader("IK Properties")]

        [SerializeField]
        private bool enableIKPositioning = true;
        [SerializeField]
        private bool enableIKRotating = true;
        [SerializeField]
        [Range(0, 1)]
        private float globalWeight = 1;
        [SerializeField]
        [Range(0, 1)]
        private float leftFootWeight = 1;
        [SerializeField]
        [Range(0, 1)]
        private float rightFootWeight = 1;
        [SerializeField]
        [Range(0, 0.1f)]
        private float smoothTime = 0.075f;

        [BigHeader("Ray Properties")]

        [SerializeField]
        [Range(0.05f, 0.1f)]
        private float raySphereRadius = 0.05f;
        [SerializeField]
        [Range(0.1f, 2)]
        private float rayCastRange = 2;
        [SerializeField]
        private LayerMask groundLayers = new LayerMask();
        [SerializeField]
        private bool ignoreTriggers = true;

        [BigHeader("Raycast Start Heights")]

        [SerializeField]
        [Range(0.1f, 1)]
        private float leftFootRayStartHeight = 0.5f;
        [SerializeField]
        [Range(0.1f, 1)]
        private float rightFootRayStartHeight = 0.5f;

        [BigHeader("Advanced")]

        [SerializeField]
        private bool enableFootLifting = true;
        [ShowIf("enableFootLifting")]
        [SerializeField]
        private float floorRange = 0;
        [SerializeField]
        private bool enableBodyPositioning = true;
        [ShowIf("enableBodyPositioning")]
        [SerializeField]
        private float crouchRange = 0.25f;
        [ShowIf("enableBodyPositioning")]
        [SerializeField]
        private float stretchRange = 0;
        [SerializeField]
        private bool giveWorldHeightOffset = false;
        [ShowIf("giveWorldHeightOffset")]
        [SerializeField]
        private float worldHeightOffset = 0;

        private RaycastHit leftFootRayHitInfo = new RaycastHit();
        private RaycastHit rightFootRayHitInfo = new RaycastHit();

        private float leftFootRayHitHeight = 0;
        private float rightFootRayHitHeight = 0;

        private Vector3 leftFootRayStartPosition = new Vector3();
        private Vector3 rightFootRayStartPosition = new Vector3();

        private Vector3 leftFootDirectionVector = new Vector3();
        private Vector3 rightFootDirectionVector = new Vector3();

        private Vector3 leftFootProjectionVector = new Vector3();
        private Vector3 rightFootProjectionVector = new Vector3();

        private float leftFootProjectedAngle = 0;
        private float rightFootProjectedAngle = 0;

        private Vector3 leftFootRayHitProjectionVector = new Vector3();
        private Vector3 rightFootRayHitProjectionVector = new Vector3();

        private float leftFootRayHitProjectedAngle = 0;
        private float rightFootRayHitProjectedAngle = 0;

        private float leftFootHeightOffset = 0;
        private float rightFootHeightOffset = 0;

        private Vector3 leftFootIKPositionBuffer = new Vector3();
        private Vector3 rightFootIKPositionBuffer = new Vector3();

        private Vector3 leftFootIKPositionTarget = new Vector3();
        private Vector3 rightFootIKPositionTarget = new Vector3();

        private Vector3 leftFootHeightLerpVelocity;
        private Vector3 rightFootHeightLerpVelocity;

        private Vector3 leftFootIKRotationBuffer = new Vector3();
        private Vector3 rightFootIKRotationBuffer = new Vector3();

        private Vector3 leftFootIKRotationTarget = new Vector3();
        private Vector3 rightFootIKRotationTarget = new Vector3();

        private Vector3 leftFootRotationLerpVelocity = new Vector3();
        private Vector3 rightFootRotationLerpVelocity = new Vector3();
        
        private Vector3 leftFootSourceRotationLerpVelocity = new Vector3();
        private Vector3 rightFootSourceRotationLerpVelocity = new Vector3();

        private GUIStyle helperTextStyle = null;



        // --- --- ---



        private void Start()
        {
            InitializeVariables();

            CreateOrientationReference();
        }



        private void Update()
        {
            UpdateFootProjection();

            UpdateRayHitInfo();
            
            UpdateIKPositionTarget();
            UpdateIKRotationTarget();
        }



        private void OnAnimatorIK(int layerIndex)
        {
            LerpIKBufferToTarget();

            ApplyFootIK();
            ApplyBodyIK();
        }



        // --- --- ---



        private void InitializeVariables()
        {
            playerAnimator = GetComponent<Animator>();
            aniCache = GetComponent<AnimatorCache>();
            
            previousYaw = transform.eulerAngles.y;

            leftFootTransform = playerAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFootTransform = playerAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
            
            leftFootIKSourceRotationTarget = leftFootTransform.rotation;
            leftFootIKSourceRotationBuffer = leftFootTransform.rotation;
            
            rightFootIKSourceRotationTarget = rightFootTransform.rotation;
            rightFootIKSourceRotationBuffer = rightFootTransform.rotation;
            
            leftFootIKPositionTarget = leftFootTransform.position;
            leftFootIKPositionBuffer = leftFootTransform.position;
            
            rightFootIKPositionTarget = rightFootTransform.position;
            rightFootIKPositionBuffer = rightFootTransform.position;

            // This is for faster development iteration purposes
            if (groundLayers.value == 0)
            {
                groundLayers = LayerMask.GetMask("Default");
            }

            // This is needed in order to wrangle with quaternions to get the final direction vector of each foot later
            initialForwardVector = transform.forward;

            // Initial value is given to make the first frames of lerping look natural, rotations should not need these
            leftFootIKPositionBuffer.y = transform.position.y + GetAnkleHeight();
            rightFootIKPositionBuffer.y = transform.position.y + GetAnkleHeight();

            // This is being done here due to internal unity reasons
            helperTextStyle = new GUIStyle()
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            helperTextStyle.normal.textColor = Color.yellow;
        }



        // This is being done to track bone orientation, since we cannot use footTransform's rotation in its own anyway
        private void CreateOrientationReference()
        {
            /* Just in case that this function gets called again... */

            if (leftFootOrientationReference != null)
            {
                Destroy(leftFootOrientationReference);
            }

            if (rightFootOrientationReference != null)
            {
                Destroy(rightFootOrientationReference);
            }

            /* These gameobjects hold different orientation values from footTransform.rotation, but the delta remains the same */

            leftFootOrientationReference = new GameObject("[RUNTIME] Normal_Orientation_Reference").transform;
            rightFootOrientationReference = new GameObject("[RUNTIME] Normal_Orientation_Reference").transform;

            leftFootOrientationReference.position = leftFootTransform.position;
            rightFootOrientationReference.position = rightFootTransform.position;

            leftFootOrientationReference.SetParent(leftFootTransform);
            rightFootOrientationReference.SetParent(rightFootTransform);
        }



        //This is being done because we want to know in what angle did the foot go underground
        private void UpdateFootProjection()
        {
            /* This is the only part in this script (except for those gizmos) that accesses footOrientationReference */

            leftFootDirectionVector = leftFootOrientationReference.rotation * initialForwardVector;
            rightFootDirectionVector = rightFootOrientationReference.rotation * initialForwardVector;

            /* World space based vector defines are used here for the representation of floor orientation */

            leftFootProjectionVector = Vector3.ProjectOnPlane(leftFootDirectionVector, Vector3.up);
            rightFootProjectionVector = Vector3.ProjectOnPlane(rightFootDirectionVector, Vector3.up);

            /* Cross is done in this order because we want the underground angle to be positive */

            leftFootProjectedAngle = Vector3.SignedAngle(
                leftFootProjectionVector,
                leftFootDirectionVector,
                Vector3.Cross(leftFootDirectionVector, leftFootProjectionVector) *
                // This is needed to cancel out the cross product's axis inverting behaviour
                Mathf.Sign(leftFootDirectionVector.y));

            rightFootProjectedAngle = Vector3.SignedAngle(
                rightFootProjectionVector,
                rightFootDirectionVector,
                Vector3.Cross(rightFootDirectionVector, rightFootProjectionVector) *
                // This is needed to cancel out the cross product's axis inverting behaviour
                Mathf.Sign(rightFootDirectionVector.y));
        }



        private void UpdateRayHitInfo()
        {
            /* Rays will be casted from above each foot, in the downward orientation of the world */

            leftFootRayStartPosition = leftFootTransform.position;
            leftFootRayStartPosition.y += leftFootRayStartHeight;

            rightFootRayStartPosition = rightFootTransform.position;
            rightFootRayStartPosition.y += rightFootRayStartHeight;

            /* SphereCast is used here just because we need a normal vector to rotate our foot towards */

            // Vector3.up is used here instead of transform.up to get normal vector in world orientation
            Physics.SphereCast(
                leftFootRayStartPosition,
                raySphereRadius,
                Vector3.up * -1,
                out leftFootRayHitInfo, rayCastRange, groundLayers,
                (QueryTriggerInteraction)(2 - Convert.ToInt32(ignoreTriggers)));

            // Vector3.up is used here instead of transform.up to get normal vector in world orientation
            Physics.SphereCast(
                rightFootRayStartPosition,
                raySphereRadius,
                Vector3.up * -1,
                out rightFootRayHitInfo, rayCastRange, groundLayers,
                (QueryTriggerInteraction)(2 - Convert.ToInt32(ignoreTriggers)));

            // Left Foot Ray Handling
            if (leftFootRayHitInfo.collider != null)
            {
                leftFootRayHitHeight = leftFootRayHitInfo.point.y;

                /* Angle from the floor is also calculated to isolate the rotation caused by the animation */

                // We are doing this crazy operation because we only want to count rotations that are parallel to the foot
                leftFootRayHitProjectionVector = Vector3.ProjectOnPlane(
                    leftFootRayHitInfo.normal,
                    Vector3.Cross(leftFootDirectionVector, leftFootProjectionVector));

                leftFootRayHitProjectedAngle = Vector3.Angle(
                    leftFootRayHitProjectionVector,
                    Vector3.up);
            }
            else
            {
                leftFootRayHitHeight = transform.position.y;
            }

            // Right Foot Ray Handling
            if (rightFootRayHitInfo.collider != null)
            {
                rightFootRayHitHeight = rightFootRayHitInfo.point.y;

                /* Angle from the floor is also calculated to isolate the rotation caused by the animation */

                // We are doing this crazy operation because we only want to count rotations that are parallel to the foot
                rightFootRayHitProjectionVector = Vector3.ProjectOnPlane(
                    rightFootRayHitInfo.normal,
                    Vector3.Cross(rightFootDirectionVector, rightFootProjectionVector));

                rightFootRayHitProjectedAngle = Vector3.Angle(
                    rightFootRayHitProjectionVector,
                    Vector3.up);
            }
            else
            {
                rightFootRayHitHeight = transform.position.y;
            }
        }



        private void UpdateIKPositionTarget()
        {
            /* We reset the offset values here instead of declaring them as local variables, since other functions reference it */

            leftFootHeightOffset = 0;
            rightFootHeightOffset = 0;

            /* Foot height correction based on the projected angle */

            float trueLeftFootProjectedAngle = leftFootProjectedAngle - leftFootRayHitProjectedAngle;

            if (trueLeftFootProjectedAngle > 0)
            {
                leftFootHeightOffset += Mathf.Abs(Mathf.Sin(
                    Mathf.Deg2Rad * trueLeftFootProjectedAngle) *
                    lengthFromHeelToToes);

                // There's no Abs here to support negative manual height offset
                leftFootHeightOffset += Mathf.Cos(
                    Mathf.Deg2Rad * trueLeftFootProjectedAngle) *
                    GetAnkleHeight();
            }
            else
            {
                leftFootHeightOffset += GetAnkleHeight();
            }

            /* Foot height correction based on the projected angle */

            float trueRightFootProjectedAngle = rightFootProjectedAngle - rightFootRayHitProjectedAngle;

            if (trueRightFootProjectedAngle > 0)
            {
                rightFootHeightOffset += Mathf.Abs(Mathf.Sin(
                    Mathf.Deg2Rad * trueRightFootProjectedAngle) *
                    lengthFromHeelToToes);

                // There's no Abs here to support negative manual height offset
                rightFootHeightOffset += Mathf.Cos(
                    Mathf.Deg2Rad * trueRightFootProjectedAngle) *
                    GetAnkleHeight();
            }
            else
            {
                rightFootHeightOffset += GetAnkleHeight();
            }

            /* Application of calculated position */
            
            leftFootIKPositionTarget.y = leftFootRayHitHeight + leftFootHeightOffset + _WorldHeightOffset;
            rightFootIKPositionTarget.y = rightFootRayHitHeight + rightFootHeightOffset + _WorldHeightOffset;
        }



        private void UpdateIKRotationTarget()
        {
            if (leftFootRayHitInfo.collider != null)
            {
                var upAngleDiff = Vector3.Angle(transform.up, leftFootRayHitInfo.normal);

                leftFootIKRotationTarget = Vector3.Slerp(
                    transform.up,
                    leftFootRayHitInfo.normal,
                    Mathf.Clamp(upAngleDiff, 0, maxRotationAngle) /
                    // Addition of 1 is to prevent division by zero, not a perfect solution but somehow works
                    (upAngleDiff + 1));
            }
            else
            {
                leftFootIKRotationTarget = transform.up;
            }

            if (rightFootRayHitInfo.collider != null)
            {
                var upAngleDiff = Vector3.Angle(transform.up, rightFootRayHitInfo.normal);

                rightFootIKRotationTarget = Vector3.Slerp(
                    transform.up,
                    rightFootRayHitInfo.normal,
                    Mathf.Clamp(upAngleDiff, 0, maxRotationAngle) /
                    // Addition of 1 is to prevent division by zero, not a perfect solution but somehow works
                    (upAngleDiff + 1));
            }
            else
            {
                rightFootIKRotationTarget = transform.up;
            }
        }



        private void LerpIKBufferToTarget()
        {
            /* Instead of wrangling with weights, we switch the lerp targets to make movement smooth */
            
            if (enableFootLifting == true &&
                playerAnimator.GetIKPosition(AvatarIKGoal.LeftFoot).y >=
                leftFootIKPositionTarget.y + floorRange)
            {
                leftFootIKPositionBuffer = Vector3.SmoothDamp(
                    leftFootIKPositionBuffer,
                    playerAnimator.GetIKPosition(AvatarIKGoal.LeftFoot),
                    ref leftFootHeightLerpVelocity,
                    smoothTime);
            }
            else 
            {
                leftFootIKPositionBuffer = Vector3.SmoothDamp(
                    leftFootIKPositionBuffer,
                    leftFootIKPositionTarget,
                    ref leftFootHeightLerpVelocity,
                    smoothTime);
            }
            
            if (enableFootLifting == true &&
                playerAnimator.GetIKPosition(AvatarIKGoal.RightFoot).y >=
                rightFootIKPositionTarget.y + floorRange)
            {
                rightFootIKPositionBuffer = Vector3.SmoothDamp(
                    rightFootIKPositionBuffer,
                    playerAnimator.GetIKPosition(AvatarIKGoal.RightFoot),
                    ref rightFootHeightLerpVelocity,
                    smoothTime);
            }
            else 
            {
                rightFootIKPositionBuffer = Vector3.SmoothDamp(
                    rightFootIKPositionBuffer,
                    rightFootIKPositionTarget,
                    ref rightFootHeightLerpVelocity,
                    smoothTime);
            }

            leftFootIKRotationBuffer = Vector3.SmoothDamp(
                leftFootIKRotationBuffer,
                leftFootIKRotationTarget,
                ref leftFootRotationLerpVelocity,
                smoothTime);

            rightFootIKRotationBuffer = Vector3.SmoothDamp(
                rightFootIKRotationBuffer,
                rightFootIKRotationTarget,
                ref rightFootRotationLerpVelocity,
                smoothTime);
            
            leftFootIKSourceRotationBuffer = SmoothDampQuaternion(
                leftFootIKSourceRotationBuffer,
                leftFootIKSourceRotationTarget,
                ref leftFootSourceRotationLerpVelocity,
                smoothTime);
            
            rightFootIKSourceRotationBuffer = SmoothDampQuaternion(
                rightFootIKSourceRotationBuffer,
                rightFootIKSourceRotationTarget,
                ref rightFootSourceRotationLerpVelocity,
                smoothTime);
        }
        
        public static Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref Vector3 currentVelocity, float smoothTime)
        {
            var c = current.eulerAngles;
            var t = target.eulerAngles;
            return Quaternion.Euler(
                Mathf.SmoothDampAngle(c.x, t.x, ref currentVelocity.x, smoothTime),
                Mathf.SmoothDampAngle(c.y, t.y, ref currentVelocity.y, smoothTime),
                Mathf.SmoothDampAngle(c.z, t.z, ref currentVelocity.z, smoothTime)
            );
        }
        
         private void ApplyFootIK()
         {
             var velocity = new Vector2(
                 playerAnimator.GetFloat(aniCache.GetHash("xVel")),
                 playerAnimator.GetFloat(aniCache.GetHash("yVel")));
            
             var moving = velocity.magnitude > 0.1f;

             var leftGrounding = playerAnimator.GetFloat(aniCache.GetHash("leftFootGrounded"));
             var rightGrounding = playerAnimator.GetFloat(aniCache.GetHash("rightFootGrounded"));

             playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, globalWeight * leftFootWeight * leftGrounding);
             playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot, globalWeight * rightFootWeight * rightGrounding);
             
             playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, globalWeight * leftFootWeight * leftGrounding);
             playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightFoot, globalWeight * rightFootWeight * rightGrounding);

             if (leftGrounding > 0.9 && !prevLeftGrounded)
             {
                 OnStep?.Invoke(false);
                 prevLeftGrounded = true;
             }
             if (leftGrounding < 0.2 || (moving && !previousMoving)) prevLeftGrounded = false;

             if (rightGrounding > 0.9 && !prevRightGrounded)
             {
                 OnStep?.Invoke(true);
                 prevRightGrounded = true;
             }
             if (rightGrounding < 0.2 || (moving && !previousMoving)) prevRightGrounded = false;
                 
             var leftDistanceDelta = Vector3.Distance(leftFootIKPositionTarget,
                 playerAnimator.GetIKPosition(AvatarIKGoal.LeftFoot));
             var leftAngleDelta = Mathf.DeltaAngle(leftFootIKSourceRotationTarget.eulerAngles.y,
                 playerAnimator.GetIKRotation(AvatarIKGoal.LeftFoot).eulerAngles.y);
                 
             var rightDistanceDelta = Vector3.Distance(rightFootIKPositionTarget,
                 playerAnimator.GetIKPosition(AvatarIKGoal.RightFoot));
             var rightAngleDelta = Mathf.DeltaAngle(rightFootIKSourceRotationTarget.eulerAngles.y,
                 playerAnimator.GetIKRotation(AvatarIKGoal.RightFoot).eulerAngles.y);
             
             var yaw = transform.eulerAngles.y;
             var yawDelta = Mathf.DeltaAngle(previousYaw, yaw);

             var isTurningRight = yawDelta > 1f;
             var isTurningLeft = yawDelta < -1f;
             
             if (!moving)
             {
                 if (stepping == SteppingFoot.None && (isTurningRight || isTurningLeft))
                 {
                     var turn = isTurningRight ? SteppingFoot.Right : SteppingFoot.Left;
                     
                     if (prevStepping != SteppingFoot.None && prevTurn == turn)
                     {
                         stepping = prevStepping == SteppingFoot.Left ? SteppingFoot.Right : SteppingFoot.Left;
                     }
                     else stepping = isTurningRight ? SteppingFoot.Right : SteppingFoot.Left;
                     
                     prevTurn = turn;
                 }
                 
                 if (leftDistanceDelta > maxLockDistance || Mathf.Abs(leftAngleDelta) > maxLockAngleDiff)
                 {
                     if (stepping == SteppingFoot.None) stepping = SteppingFoot.Left;
                     if (stepping == SteppingFoot.Left)
                     {
                         leftStepProgress = 0.01f;
                         OnStep?.Invoke(false);
                     }
                 }

                 if (rightDistanceDelta > maxLockDistance || Mathf.Abs(rightAngleDelta) > maxLockAngleDiff)
                 {
                     if (stepping == SteppingFoot.None) stepping = SteppingFoot.Right;
                     if (stepping == SteppingFoot.Right)
                     {
                         rightStepProgress = 0.01f;
                         OnStep?.Invoke(true);
                     }
                 }
             }
             
             if (leftStepProgress > 0) leftStepProgress += Time.deltaTime / stepDuration;
             if (leftStepProgress >= 2)
             {
                 leftStepProgress = 0;
                 prevStepping = stepping;
                 stepping = SteppingFoot.None;
             }
             
             if (rightStepProgress > 0) rightStepProgress += Time.deltaTime / stepDuration;
             if (rightStepProgress >= 2)
             {
                 rightStepProgress = 0;
                 prevStepping = stepping;
                 stepping = SteppingFoot.None;
             }
             
             if (!enableFootLocking || moving || stepping == SteppingFoot.Left && leftStepProgress > 0)
             {
                 CopyByAxis(ref leftFootIKPositionTarget, playerAnimator.GetIKPosition(AvatarIKGoal.LeftFoot),
                     true, false, true);
                 leftFootIKSourceRotationTarget = Quaternion.FromToRotation(transform.up, leftFootIKRotationTarget)
                                                  * playerAnimator.GetIKRotation(AvatarIKGoal.LeftFoot);
             }
             
             if (!enableFootLocking || moving || stepping == SteppingFoot.Right && rightStepProgress > 0)
             {
                 CopyByAxis(ref rightFootIKPositionTarget, playerAnimator.GetIKPosition(AvatarIKGoal.RightFoot),
                     true, false, true); 
                 rightFootIKSourceRotationTarget = Quaternion.FromToRotation(transform.up, rightFootIKRotationTarget)
                                                   * playerAnimator.GetIKRotation(AvatarIKGoal.RightFoot);
             }
             
             var leftArc = new Vector3(0, enableFootLocking ? Mathf.Sin(Mathf.Min(leftStepProgress, 1) * Mathf.PI) * stepLiftHeight : 0, 0);
             var rightArc = new Vector3(0, enableFootLocking ? Mathf.Sin(Mathf.Min(rightStepProgress, 1) * Mathf.PI) * stepLiftHeight : 0, 0);
             
             
             Quaternion leftFootRotation, rightFootRotation;
             if (moving || !enableFootLocking)
             {
                 // FromToRotation is used because we need the delta, not the final target orientation
                 leftFootRotation =
                     Quaternion.FromToRotation(transform.up, leftFootIKRotationBuffer) *
                     playerAnimator.GetIKRotation(AvatarIKGoal.LeftFoot);

                 // FromToRotation is used because we need the delta, not the final target orientation
                 rightFootRotation =
                     Quaternion.FromToRotation(transform.up, rightFootIKRotationBuffer) *
                     playerAnimator.GetIKRotation(AvatarIKGoal.RightFoot);
             }
             else
             {
                 leftFootRotation = leftFootIKSourceRotationBuffer;
                 rightFootRotation = rightFootIKSourceRotationBuffer;
             }
             
             previousYaw = yaw;
             previousMoving = moving;
             
             if (enableIKPositioning)
             {
                 playerAnimator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootIKPositionBuffer + leftArc);
                 playerAnimator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootIKPositionBuffer + rightArc);
             }
             
             if (enableIKRotating)
             {
                 playerAnimator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRotation);
                 playerAnimator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootRotation);
             }
         }



        private void ApplyBodyIK()
        {
            if (enableBodyPositioning == false)
            {
                return;
            }

            float minFootHeight = Mathf.Min(
                    playerAnimator.GetIKPosition(AvatarIKGoal.LeftFoot).y,
                    playerAnimator.GetIKPosition(AvatarIKGoal.RightFoot).y);

            /* This part moves the body 'downwards' by the root gameobject's height */

            playerAnimator.bodyPosition = new Vector3(
            playerAnimator.bodyPosition.x,
            playerAnimator.bodyPosition.y +
            LimitValueByRange(minFootHeight - transform.position.y, 0),
            playerAnimator.bodyPosition.z);
        }



        private float GetAnkleHeight()
        {
            return raySphereRadius + _AnkleHeightOffset;
        }



        private void CopyByAxis(ref Vector3 target, Vector3 source, bool copyX, bool copyY, bool copyZ)
        {
            target = new Vector3(
                Mathf.Lerp(
                    target.x,
                    source.x,
                    Convert.ToInt32(copyX)),
                Mathf.Lerp(
                    target.y,
                    source.y,
                    Convert.ToInt32(copyY)),
                Mathf.Lerp(
                    target.z,
                    source.z,
                    Convert.ToInt32(copyZ)));
        }



        private float LimitValueByRange(float value, float floor)
        {
            if (value < floor - stretchRange)
            {
                return value + stretchRange;
            }
            else if (value > floor + crouchRange)
            {
                return value - crouchRange;
            }
            else
            {
                return floor;
            }
        }



#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Debug draw function relies on objects that are dynamically located during runtime
            if (Application.isPlaying == false)
            {
                return;
            }

            /* Left Foot */

            if (leftFootRayHitInfo.collider != null)
            {
                Handles.color = Color.yellow;

                // Just note that the normal vector of RayCastHit object is used here
                Handles.DrawWireDisc(
                    leftFootRayHitInfo.point,
                    leftFootRayHitInfo.normal,
                    0.1f);
                Handles.DrawDottedLine(
                    leftFootTransform.position,
                    leftFootTransform.position + leftFootRayHitInfo.normal,
                    2);

                // Just note that the orientation of the parent transform is used here
                Handles.color = Color.green;
                Handles.DrawWireDisc(
                    leftFootRayHitInfo.point,
                    transform.up, 0.25f);

                Gizmos.color = Color.green;

                Gizmos.DrawSphere(
                    leftFootRayHitInfo.point + transform.up * raySphereRadius,
                    raySphereRadius);
            }
            else
            {
                Gizmos.color = Color.red;
            }

            if (leftFootProjectedAngle > 0)
            {
                Handles.color = Color.blue;
            }
            else
            {
                Handles.color = Color.red;
            }

            // Foot height correction related debug draws
            Handles.DrawWireDisc(
                leftFootTransform.position,
                leftFootOrientationReference.rotation * transform.up,
                0.15f);
            Handles.DrawSolidArc(
                leftFootTransform.position,
                Vector3.Cross(leftFootDirectionVector, leftFootProjectionVector) * -1,
                leftFootProjectionVector,
                // Abs is needed here because the cross product will deal with axis direction
                Mathf.Abs(leftFootProjectedAngle),
                0.25f);
            Handles.DrawDottedLine(
                leftFootTransform.position,
                leftFootTransform.position + leftFootDirectionVector.normalized,
                2);

            // SphereCast related debug draws
            Gizmos.DrawWireSphere(
                leftFootRayStartPosition,
                0.1f);
            Gizmos.DrawLine(
                leftFootRayStartPosition,
                leftFootRayStartPosition - rayCastRange * Vector3.up);

            // Indicator text
            Handles.Label(leftFootTransform.position, "L", helperTextStyle);

            /* Right foot */

            if (rightFootRayHitInfo.collider != null)
            {
                Handles.color = Color.yellow;

                // Just note that the normal vector of RayCastHit object is used here
                Handles.DrawWireDisc(
                    rightFootRayHitInfo.point,
                    rightFootRayHitInfo.normal,
                    0.1f);
                Handles.DrawDottedLine(
                    rightFootTransform.position,
                    rightFootTransform.position + rightFootRayHitInfo.normal,
                    2);

                // Just note that the orientation of the parent transform is used here
                Handles.color = Color.green;
                Handles.DrawWireDisc(
                    rightFootRayHitInfo.point,
                    transform.up, 0.25f);

                Gizmos.color = Color.green;

                Gizmos.DrawSphere(
                    rightFootRayHitInfo.point + transform.up * raySphereRadius,
                    raySphereRadius);
            }
            else
            {
                Gizmos.color = Color.red;
            }

            if (rightFootProjectedAngle > 0)
            {
                Handles.color = Color.blue;
            }
            else
            {
                Handles.color = Color.red;
            }

            // Foot height correction related debug draws
            Handles.DrawWireDisc(
                rightFootTransform.position,
                rightFootOrientationReference.rotation * transform.up,
                0.15f);
            Handles.DrawSolidArc(
                rightFootTransform.position,
                Vector3.Cross(rightFootDirectionVector, rightFootProjectionVector) * -1,
                rightFootProjectionVector,
                // Abs is needed here because the cross product will deal with axis direction
                Mathf.Abs(rightFootProjectedAngle),
                0.25f);
            Handles.DrawDottedLine(
                rightFootTransform.position,
                rightFootTransform.position + rightFootDirectionVector.normalized,
                2);

            // SphereCast related debug draws
            Gizmos.DrawWireSphere(
                rightFootRayStartPosition,
                0.1f);
            Gizmos.DrawLine(
                rightFootRayStartPosition,
                rightFootRayStartPosition - rayCastRange * Vector3.up);

            // Indicator text
            Handles.Label(rightFootTransform.position, "R", helperTextStyle);
        }
#endif
    }



    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string _BaseCondition {
            get { return mBaseCondition; }
        }

        private string mBaseCondition = String.Empty;

        public ShowIfAttribute(string baseCondition)
        {
            mBaseCondition = baseCondition;
        }
    }



    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class BigHeaderAttribute : PropertyAttribute
    {
        public string _Text {
            get { return mText; }
        }

        private string mText = String.Empty;

        public BigHeaderAttribute(string text)
        {
            mText = text;
        }
    }



}