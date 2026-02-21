using UnityEngine;

/// <summary>
/// 第一人称摄像机控制器：独立于玩家层级之外，防止旋转死循环
/// </summary>
public class FirstPersonCamera : MonoBehaviour
{
    [Header("绑定目标")]
    [Tooltip("摄像机需要跟随的玩家眼睛位置")]
    public Transform EyePosition;

    [Header("视角参数")]
    public float MouseSensitivity = 2f; // 鼠标灵敏度
    public float MinPitch = -89f;       // 向上看的极限角度（防止脖子往后折断）
    public float MaxPitch = 89f;        // 向下看的极限角度

    private float _pitch = 0f;          // 当前的上下旋转角度 (X轴)
    private float _yaw = 0f;            // 当前的左右旋转角度 (Y轴)

    private void Start()
    {
        // 锁定并隐藏鼠标指针，这是第一人称游戏的标配
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 初始化时获取当前的旋转角度
        Vector3 euler = transform.eulerAngles;
        _pitch = euler.x;
        _yaw = euler.y;
    }

    // 使用 LateUpdate 是为了确保在 PlayerController 计算完位置（甚至播放完动画）之后，摄像机再跟上去，消除画面抖动
    private void LateUpdate()
    {
        if (EyePosition == null) return;

        // 1. 捕获鼠标移动
        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity;

        // 2. 累加旋转角度
        _yaw += mouseX;
        _pitch -= mouseY; // 注意这里是减号，否则上下视角会反转

        // 3. 限制上下视角的范围
        _pitch = Mathf.Clamp(_pitch, MinPitch, MaxPitch);

        // 4. 应用旋转
        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

        // 5. 让摄像机位置瞬间移动到目标点（解耦的核心）
        transform.position = EyePosition.position;
    }
}