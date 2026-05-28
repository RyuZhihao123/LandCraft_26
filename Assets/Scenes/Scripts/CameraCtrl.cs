using UnityEngine;
using System.Collections.Generic;


public partial class CameraCtrl : MonoBehaviour
{
    // 相机移动相关
    private float moveVerticalSpeed = 50.0f;//水平移动速度
    private float moveHorizontalSpeed = 50.0f;//垂直移动速度
    private float moveJumpSpeed = 50.0f;//垂直移动速度
    private float angleX = 0.0f;
    private float angleY = 0.0f;

    void Start()
    {
        UpdateCameraTransform();
    }

    // QT端传输进来的交互动作。
    float camera_dist = 148.8f;
    float camera_angleX = -1.28f;
    float camera_angleY = 0.9817991f;


    void Update()
    {
        DoKey_MoveEvent();

        if (TCP_Qt_Server.InteractionInfo.interaction_type == "Rotate")  // 如果执行了旋转操作
        {
            camera_angleX -= TCP_Qt_Server.InteractionInfo.interaction_dx / 50.0f;
            camera_angleY += TCP_Qt_Server.InteractionInfo.interaction_dy / 50.0f;

            this.UpdateCameraTransform();

            TCP_Qt_Server.InteractionInfo.interaction_type = "none";

            TCP_Client.SendMessage(string.Format("A [Camera Info]" + camera_angleX.ToString() + " " + camera_angleY.ToString() + " " + camera_dist.ToString()));
        }
        if (TCP_Qt_Server.InteractionInfo.interaction_type == "Zoom")  // 如果执行了缩放操作
        {
            camera_dist -= TCP_Qt_Server.InteractionInfo.interaction_zoom / 25.0f;

            if (camera_dist < 0.1f)
                camera_dist = 0.1f;

            this.UpdateCameraTransform();

            TCP_Qt_Server.InteractionInfo.interaction_type = "none";

            TCP_Client.SendMessage(string.Format("A [Camera Info]" + camera_angleX.ToString() + " " + camera_angleY.ToString() + " " + camera_dist.ToString()));
        }

    }

    // 键盘事件
    bool m_camera_move_flag = false;  // 相机是否发生移动

    void DoKey_MoveEvent()
    {

        ///////////////// 移动相机 WASD /////////////////
        if (Input.GetKeyDown(KeyCode.LeftControl))  // control键用来开启/关闭
        {
            m_camera_move_flag = !m_camera_move_flag;
        }


        if (m_camera_move_flag)
        {
            float Ztranslation = Input.GetAxis("Vertical") * moveVerticalSpeed * Time.deltaTime;
            float Xtranslation = Input.GetAxis("Horizontal") * moveHorizontalSpeed * Time.deltaTime;
            float Ytranslation = Input.GetAxis("Jump") * moveJumpSpeed * Time.deltaTime;

            Debug.Log(string.Format("{0}, {1}, {2}", Ztranslation, moveVerticalSpeed, Time.deltaTime));

            //float Ztranslation = Input.GetAxis("Vertical") * moveVerticalSpeed;
            //float Xtranslation = Input.GetAxis("Horizontal") * moveHorizontalSpeed;
            //float Ytranslation = Input.GetAxis("Jump") * moveJumpSpeed;

            angleX += Input.GetAxis("Mouse X");
            angleY += Input.GetAxis("Mouse Y");

            if (angleX < 0)
                angleX += 360;
            if (angleY > 360)
                angleX -= 360;
            angleY = Mathf.Clamp(angleY, -89, 89);

            //----------------------旋转视角---------------------------
            transform.forward = new Vector3(
                1.0f * Mathf.Cos(Mathf.PI * angleY / 180.0f) * Mathf.Sin(Mathf.PI * angleX / 180.0f),
                1.0f * Mathf.Sin(Mathf.PI * angleY / 180.0f),
                1.0f * Mathf.Cos(Mathf.PI * angleY / 180.0f) * Mathf.Cos(Mathf.PI * angleX / 180.0f)
                ).normalized;  // 正向默认都是(0,0,1)

            // --------------WASD移动 & space上移------------------
            transform.Translate(0, 0, Ztranslation);
            transform.Translate(Xtranslation, 0, 0);
            transform.Translate(0, Ytranslation, 0);
        }
    }

    // 根据Qt端口的信息，来更新相机位置
    public void UpdateCameraTransform()
    {
        Vector3 target_center_pos = new Vector3(40, 0, 40);
        Transform transf = this.GetComponent<Transform>();

        if (camera_angleY >= Mathf.PI / 2.0f)
            camera_angleY = Mathf.PI / 2.0f - 0.01f;
        if (camera_angleY <= -Mathf.PI / 2.0f)
            camera_angleY = Mathf.PI / 2.0f + 0.01f;

        transf.position = new Vector3(
            camera_dist * Mathf.Cos(camera_angleY) * Mathf.Cos(camera_angleX),
            camera_dist * Mathf.Sin(camera_angleY),
            camera_dist * Mathf.Cos(camera_angleY) * Mathf.Sin(camera_angleX)
            );

        transf.position += target_center_pos;
        transf.forward = (target_center_pos - transf.position).normalized;
    }

}



//public partial class CameraCtrl : MonoBehaviour
//{
//    // 相机移动相关
//    private float moveVerticalSpeed = 18.0f;
//    private float moveHorizontalSpeed = 18.0f;
//    private float moveJumpSpeed = 18.0f;
//    private float angleX = 0.0f;
//    private float angleY = 0.0f;
//    private bool freeView = true;

//    private bool m_camera_move_flag = false;

//    // private MainScript m_main_script;
//    // private Camera m_camera;
    
//    public static readonly int MOVE_W = 0b000001;
//    public static readonly int MOVE_S = 0b000010;
//    public static readonly int MOVE_A = 0b000100;
//    public static readonly int MOVE_D = 0b001000;
//    public static readonly int MOVE_Q = 0b010000;
//    public static readonly int MOVE_E = 0b100000;
//    public static readonly int MOVING = 0b111111;
//    public static readonly int ROTATE = 0b1000000;
//    public static readonly int ZOOM = 0b10000000;

//    void Start() {
//        // transform.rotation = Quaternion.Euler(30f, 180f, 0f);
//        // Vector3 angles = transform.rotation.eulerAngles;
//        // angleX = angles.y;
//        // angleY = angles.x;
//    }

//    void Update()
//    {
//        if (TCP_Qt_Server.InteractionInfo.shouldResetView)
//        {
//            ResetViewToFix();
//            TCP_Qt_Server.InteractionInfo.shouldResetView = false;
//        }

//        DoKey_MoveEvent();
//        DoQt_MoveEvent();
//    }

//    // unity side directly control the camera move
//    void DoKey_MoveEvent()
//    {
//        ///////////////// 移动相机 WASD /////////////////
//        if (Input.GetMouseButtonDown(0))  // 左键用来开启/关闭
//        {
//            m_camera_move_flag = true;
//        } else if (Input.GetMouseButtonUp(0)) {
//            m_camera_move_flag = false;
//        }

//        if (m_camera_move_flag)
//        {
//            float Ztranslation = Input.GetAxis("Vertical") * moveVerticalSpeed * Time.deltaTime;
//            float Xtranslation = Input.GetAxis("Horizontal") * moveHorizontalSpeed * Time.deltaTime;
//            float Ytranslation = 0;
            
//            if (Input.GetKey(KeyCode.Q)) {
//                Ytranslation = moveJumpSpeed * Time.deltaTime; // 往上移动
//            }
//            if (Input.GetKey(KeyCode.E)) {
//                Ytranslation = -moveJumpSpeed * Time.deltaTime; // 往下移动
//            }

//            angleX = Input.GetAxis("Mouse X"); 
//            angleY = Input.GetAxis("Mouse Y"); 

//            if (angleX < 0)
//                angleX += 360;
//            if (angleY > 360)
//                angleX -= 360;
//            angleY = Mathf.Clamp(angleY, -89, 89);

//            //----------------------旋转视角---------------------------
//            // transform.forward = new Vector3(
//            //     1.0f * Mathf.Cos(Mathf.PI* angleY / 180.0f) * Mathf.Sin(Mathf.PI * angleX / 180.0f),
//            //     1.0f * Mathf.Sin(Mathf.PI * angleY / 180.0f),
//            //     1.0f * Mathf.Cos(Mathf.PI * angleY / 180.0f) * Mathf.Cos(Mathf.PI * angleX / 180.0f)
//            //     ).normalized;  // 正向默认都是(0,0,1)
//            transform.Rotate(Vector3.up, angleX, Space.World);
//            transform.Rotate(Vector3.right, -angleY, Space.Self);

//            // --------------WASD移动 & space上移------------------
//            transform.Translate(0, 0, Ztranslation);
//            transform.Translate(Xtranslation, 0, 0);
//            transform.Translate(0, Ytranslation, 0, Space.World);
//        }
//    }
    
//    // Qt side remote control the camera move
//    void DoQt_MoveEvent() {
//        /*
//         * WSAD: 分别对应第1，2，3，4 bit
//         * 鼠标操作：对应第5bit
//         */
//        if (!TCP_Qt_Server.InteractionInfo.enable)
//        {
//            return;
//        }

//        // 一个是自由视角观察一个是中心固定中心点观察
//        if (TCP_Qt_Server.InteractionInfo.freeControl)
//        {
//            HandleQtFreeControl();
//        }
//        else
//        {
//            HandleQtFixControl();
//        }

//    }

//    private void HandleQtFreeControl()
//    {
//        if ((TCP_Qt_Server.InteractionInfo.interaction_type & MOVING) != 0)
//        {
//            float Ztranslation = 0;
//            float Xtranslation = 0;
//            float Ytranslation = 0;
        
//            if ((TCP_Qt_Server.InteractionInfo.interaction_type & MOVE_W) != 0) {
//                Ztranslation = moveVerticalSpeed * Time.deltaTime;
//            }
//            if ((TCP_Qt_Server.InteractionInfo.interaction_type & MOVE_S) != 0) {
//                Ztranslation = -moveVerticalSpeed * Time.deltaTime;
//            }
//            if ((TCP_Qt_Server.InteractionInfo.interaction_type & MOVE_A) != 0) {
//                Xtranslation = -moveVerticalSpeed * Time.deltaTime;
//            }
//            if ((TCP_Qt_Server.InteractionInfo.interaction_type & MOVE_D) != 0) {
//                Xtranslation = moveVerticalSpeed * Time.deltaTime;
//            }
//            if ((TCP_Qt_Server.InteractionInfo.interaction_type & MOVE_Q) != 0) {
//                Ytranslation = moveVerticalSpeed * Time.deltaTime;
//            }
//            if ((TCP_Qt_Server.InteractionInfo.interaction_type & MOVE_E) != 0) {
//                Ytranslation = -moveVerticalSpeed * Time.deltaTime;
//            }
        
//            // --------------WASD QE移动 & space上移------------------
//            transform.Translate(0, 0, Ztranslation);
//            transform.Translate(Xtranslation, 0, 0);
//            transform.Translate(0, Ytranslation, 0, Space.World);
//        }
        
//        if ((TCP_Qt_Server.InteractionInfo.interaction_type & ROTATE) != 0) {
//            angleX = TCP_Qt_Server.InteractionInfo.interaction_dx * 3.0f; 
//            angleY = TCP_Qt_Server.InteractionInfo.interaction_dy * 3.0f;

//            if (angleX < 0)
//                angleX += 360;
//            if (angleY > 360)
//                angleX -= 360;
//            angleY = Mathf.Clamp(angleY, -89, 89);
            
//            //----------------------旋转视角---------------------------
//            // transform.forward = new Vector3(
//            //     1.0f * Mathf.Cos(Mathf.PI* angleY / 180.0f) * Mathf.Sin(Mathf.PI * angleX / 180.0f),
//            //     1.0f * Mathf.Sin(Mathf.PI * angleY / 180.0f),
//            //     1.0f * Mathf.Cos(Mathf.PI * angleY / 180.0f) * Mathf.Cos(Mathf.PI * angleX / 180.0f)
//            // ).normalized;  // 正向默认都是(0,0,1)
//            transform.Rotate(Vector3.up, angleX, Space.World);
//            transform.Rotate(Vector3.right, angleY, Space.Self);

//            TCP_Qt_Server.InteractionInfo.interaction_dx = 0;
//            TCP_Qt_Server.InteractionInfo.interaction_dy = 0;
//        }
        
//    }
    
//    // 下面是用于fix视角的变量
//    private bool lockPitch = false;
    
//    private float rotationSpeed = 1.0f;   // 旋转速度
//    private float zoomSpeed = 0.05f;      // 缩放速度
//    private Vector3 lookAtPoint = new Vector3(30.0f, 5.0f, 30.0f);  // 刚开始的观察中心
//    private float translationSpeed = 1.0f;   // 垂直移动观察中心速度

//    private Vector3 initialOffset;
//    private Vector3 currentOffset;
//    private Vector3 initialDirection;
//    private float distanceToLookAtPoint;

//    private float yaw = .0f;
//    private float pitch = .0f;

//    public void ResetViewToFix()
//    {
//        transform.position = new Vector3(30, 25, 70);
//        transform.rotation = Quaternion.Euler(40,180,0);

//        initialOffset = transform.position - lookAtPoint;
//        currentOffset = initialOffset;
//        initialDirection = initialOffset.normalized;

//        var eulerAngles = transform.eulerAngles;
//        yaw = eulerAngles.y;
//        pitch = eulerAngles.x;

//        distanceToLookAtPoint = initialOffset.magnitude;
//    }

//    private void HandleQtFixControl()
//    {
//        // 旋转
//        if ((TCP_Qt_Server.InteractionInfo.interaction_type & ROTATE) != 0)
//        {
//            float horizontalRotation = TCP_Qt_Server.InteractionInfo.interaction_dx * rotationSpeed;
//            float verticalRotation = TCP_Qt_Server.InteractionInfo.interaction_dy * rotationSpeed;

//            yaw += horizontalRotation;

//            if (!lockPitch)
//            {
//                pitch -= verticalRotation;
//                pitch = Mathf.Clamp(pitch, -80.0f, 80.0f);
//            }
            
//            currentOffset = Quaternion.Euler(pitch, yaw, 0) *
//                            (initialDirection * distanceToLookAtPoint);
            
//            TCP_Qt_Server.InteractionInfo.interaction_dx = 0;
//            TCP_Qt_Server.InteractionInfo.interaction_dy = 0;
//        }
        
//        // 中心点上升下降
//        if ((TCP_Qt_Server.InteractionInfo.interaction_type & (MOVE_Q)) != 0)
//        {
//            lookAtPoint += Vector3.up * (translationSpeed * Time.deltaTime);
//        } else if ((TCP_Qt_Server.InteractionInfo.interaction_type & (MOVE_E)) != 0)
//        {
//            lookAtPoint += Vector3.down * (translationSpeed * Time.deltaTime);
//        }

//        // 缩放
//        if ((TCP_Qt_Server.InteractionInfo.interaction_type & ZOOM) != 0)
//        {
//            float zoomAmount = TCP_Qt_Server.InteractionInfo.interaction_zoom * zoomSpeed;
//            currentOffset = Vector3.MoveTowards(currentOffset, Vector3.zero, zoomAmount);

//            distanceToLookAtPoint = currentOffset.magnitude;
            
//            // 清除zoom的比特位
//            TCP_Qt_Server.InteractionInfo.interaction_type &= ~ZOOM;
//        }

//        transform.position = lookAtPoint + currentOffset;
//        transform.LookAt(lookAtPoint);
//    }
//}

