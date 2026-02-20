using UnityEngine;
using KinematicCharacterController;

public class MyPlayerController : MonoBehaviour, ICharacterController
{
    public KinematicCharacterMotor Motor;

    [Header("移动参数")]
    public float MaxSpeed = 10f;
    public float Acceleration = 50f; // 加速度，数值越大起步越快

    private void Start()
    {
        // 建立连接：让 Motor 知道该问谁要移动逻辑
        Motor.CharacterController = this;
    }

    // 1. 处理旋转的回调
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        // 这里暂时保持不动，或者对接你的鼠标输入
    }

    // 2. 处理速度的回调（核心逻辑）
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        // 我们在这里写代码
    }

    // 以下是必须实现但暂时可以留空的接口方法
    public void BeforeCharacterUpdate(float deltaTime) { }
    public void PostCharacterUpdate(float deltaTime) { }
    public void AfterCharacterUpdate(float deltaTime) { }
    public bool IsRelevantUpdate(float deltaTime) => true;
    public void OnDiscreteCollisionDetected(Collider hitCollider) { }
    // 注意：根据版本不同，可能还有其他接口方法需要留空实现
}