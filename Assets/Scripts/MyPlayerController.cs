using UnityEngine;
using KinematicCharacterController; // 引入 KCC 核心

// 强制要求挂载 KCC 的核心引擎组件
[RequireComponent(typeof(KinematicCharacterMotor))]
public class PlayerController : MonoBehaviour, ICharacterController
{
    public KinematicCharacterMotor Motor;

    [Header("移动参数")]
    public float MaxSpeed = 8f;             // 最大奔跑速度
    public float GroundAcceleration = 50f;  // 地面起步加速度（值越大，推摇杆起步越快）
    public float GroundFriction = 8f;       // 地面摩擦力（值越大，松开键盘停得越快）
    public float JumpSpeed = 10f;
    public float Gravity = 30f;

    private Vector3 _moveInput;         // 缓存玩家的移动输入方向
    private bool _jumpRequested;        // 缓存玩家的跳跃请求

    private void Awake()
    {
        // 获取 Motor 组件，并告诉它：“我是你的大脑，请听我指挥”
        Motor = GetComponent<KinematicCharacterMotor>();
        Motor.CharacterController = this;
    }

    // 1. 在 Update 中捕获输入（满足快速实现的需求）
    private void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // 关键逻辑：获取摄像机的朝向，将 WASD 转换为相对于视角的移动方向
        if (Camera.main != null)
        {
            Vector3 cameraForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Motor.CharacterUp).normalized;
            Vector3 cameraRight = Vector3.ProjectOnPlane(Camera.main.transform.right, Motor.CharacterUp).normalized;

            _moveInput = (cameraForward * moveY + cameraRight * moveX).normalized;
        }

        // 捕获跳跃按键
        if (Input.GetButtonDown("Jump"))
        {
            _jumpRequested = true;
        }
    }

    #region KCC 核心接口实现

    // 2. KCC 引擎每一帧都会调用这里，询问“现在的速度应该是多少？”
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (Motor.GroundingStatus.IsStableOnGround)
        {
            // --- 处于地面 ---

            // 1. 应用地面摩擦力 (模拟松开按键后的滑行惯性)
            float currentSpeed = currentVelocity.magnitude;
            if (currentSpeed > 0.1f) // 只有在有速度时才计算摩擦力
            {
                // 计算这一帧应该衰减多少速度
                float frictionDrop = currentSpeed * GroundFriction * deltaTime;
                float newSpeed = Mathf.Max(currentSpeed - frictionDrop, 0f);
                // 按比例缩小当前速度向量
                currentVelocity *= (newSpeed / currentSpeed);
            }

            // 2. 处理地面加速 (模拟推摇杆/按WASD的起步发力)
            if (_moveInput.sqrMagnitude > 0f)
            {
                // 计算当前速度在我们想要移动的方向上的投影
                float currentSpeedInMoveDir = Vector3.Dot(currentVelocity, _moveInput);

                // 计算我们还能在这个方向上增加多少速度
                float speedToAdd = MaxSpeed - currentSpeedInMoveDir;

                if (speedToAdd > 0f)
                {
                    // 计算本帧理论上能提供的加速度
                    float accelSpeed = GroundAcceleration * deltaTime * MaxSpeed;
                    // 截断速度，确保不会超过最大速度限制
                    accelSpeed = Mathf.Min(accelSpeed, speedToAdd);

                    // 将算好的加速度施加到当前速度向量上
                    currentVelocity += _moveInput * accelSpeed;
                }
            }

            // 3. 处理跳跃
            if (_jumpRequested)
            {
                currentVelocity += Motor.CharacterUp * JumpSpeed;
                Motor.ForceUnground();
                _jumpRequested = false;
            }
        }
        else
        {
            // --- 处于空中 ---
            // 施加重力
            currentVelocity -= Motor.CharacterUp * Gravity * deltaTime;

            // 空中微调控制（暂时保留简单的实现）
            Vector3 targetVelocity = _moveInput * MaxSpeed;
            currentVelocity += targetVelocity * (GroundAcceleration * 0.05f * deltaTime);
        }
    }

    // 3. KCC 引擎询问“身体应该朝向哪里？”
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        // 第一人称游戏：角色的身体始终跟随摄像机的水平朝向（Y轴旋转）
        if (Camera.main != null)
        {
            Vector3 cameraForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Motor.CharacterUp).normalized;
            if (cameraForward.sqrMagnitude > 0f)
            {
                currentRotation = Quaternion.LookRotation(cameraForward, Motor.CharacterUp);
            }
        }
    }

    #endregion

    // --- 以下是 ICharacterController 要求实现的其他接口，原型阶段全部留空即可 ---
    public void BeforeCharacterUpdate(float deltaTime) { }
    public void PostGroundingUpdate(float deltaTime) { }
    public void AfterCharacterUpdate(float deltaTime) { }
    public bool IsColliderValidForCollisions(Collider coll) { return true; }
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
    public void OnDiscreteCollisionDetected(Collider hitCollider) { }
}