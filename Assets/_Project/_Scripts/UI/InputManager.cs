// FILE: _Project/_Scripts/UI/InputManager.cs (SỬA LỖI HIỂN THỊ TRÊN PC)
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Project._Scripts.UI
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        // Các tham chiếu này sẽ được tự động tìm
        private GameObject mobileControlsCanvas;
        private Joystick movementJoystick;

        // Public Properties
        public Vector2 MoveInput { get; private set; }
        public bool DashTriggered { get; private set; }
        public bool ShootInput { get; private set; }
        public bool FocusInput { get; private set; }
        public bool BombTriggered { get; private set; }
        public bool InvincibilityTriggered { get; private set; }

        private bool isMobilePlatform;
        private bool dashButtonFlag = false;
        private bool invincibilityButtonFlag = false;
        private bool bombButtonFlag = false;

        #region Unity Lifecycle & Scene Management

        void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else { Destroy(gameObject); return; }
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Khi vào scene mới, thiết lập lại các control
            SetupControlsForCurrentScene();
        }

        #endregion

        #region Input Processing Loop

        void Update()
        {
            // Xử lý input dựa trên nền tảng
            if (isMobilePlatform) { HandleMobileInput(); }
            else { HandlePCInput(); }
            
            // Kích hoạt các cờ ...Triggered
            if (dashButtonFlag) DashTriggered = true;
            if (invincibilityButtonFlag) InvincibilityTriggered = true;
            if (bombButtonFlag) BombTriggered = true;
        }

        void LateUpdate()
        {
            // Reset các cờ ở cuối frame
            dashButtonFlag = false;
            invincibilityButtonFlag = false;
            bombButtonFlag = false;
            DashTriggered = false;
            InvincibilityTriggered = false;
            BombTriggered = false;
        }

        #endregion

        #region Platform & Control Setup

        // --- HÀM ĐÃ ĐƯỢC THIẾT KẾ LẠI HOÀN TOÀN ---
        private void SetupControlsForCurrentScene()
        {
            // Bước 1: Luôn luôn tìm Canvas trước
            mobileControlsCanvas = GameObject.FindGameObjectWithTag("MobileControlsCanvas");

            if (mobileControlsCanvas == null)
            {
                // Không tìm thấy canvas trong scene này, không cần làm gì thêm.
                return;
            }

            // Bước 2: Xác định nền tảng
            isMobilePlatform = false;
    #if UNITY_ANDROID || UNITY_IOS
            isMobilePlatform = true;
    #elif UNITY_WEBGL
            isMobilePlatform = Application.isMobilePlatform;
    #endif

            // Bước 3: Dựa vào nền tảng để Bật hoặc Tắt Canvas
            // Đây là dòng code sửa lỗi chính!
            mobileControlsCanvas.SetActive(isMobilePlatform);

            // Bước 4: Nếu là nền tảng di động, tiếp tục tìm các control con và gán sự kiện
            if (isMobilePlatform)
            {
                movementJoystick = mobileControlsCanvas.GetComponentInChildren<Joystick>();
                
                Button[] buttons = mobileControlsCanvas.GetComponentsInChildren<Button>(true);
                foreach (Button btn in buttons)
                {
                    btn.onClick.RemoveAllListeners(); // Luôn xóa listener cũ trước
                    if (btn.name.Contains("Dash"))
                    {
                        btn.onClick.AddListener(OnDashButtonPressed);
                    }
                    else if (btn.name.Contains("Invincibility"))
                    {
                        btn.onClick.AddListener(OnInvincibilityButtonPressed);
                    }
                    else if (btn.name.Contains("Bomb"))
                    {
                        btn.onClick.AddListener(OnBombButtonPressed);
                    }
                }
                Debug.Log("InputManager đã kết nối với các control di động.");
            }
        }

        #endregion

        #region Input Handlers

        private void HandlePCInput()
        {
            MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            ShootInput = Input.GetButton("Fire1");
            FocusInput = Input.GetButton("Fire3");
            if (Input.GetButtonDown("Jump")) DashTriggered = true;
            if (Input.GetButtonDown("Fire2")) BombTriggered = true;
            if (Input.GetKeyDown(KeyCode.E)) InvincibilityTriggered = true;
        }

        private void HandleMobileInput()
        {
            if (movementJoystick != null)
            {
                MoveInput = movementJoystick.Direction;
                ShootInput = movementJoystick.Direction.magnitude > 0.1f;
            }
            else
            {
                MoveInput = Vector2.zero;
                ShootInput = false;
            }
        }

        #endregion

        #region Public Methods for UI Events

        public void SetFocus(bool isFocusing) { FocusInput = isFocusing; }
        public void OnDashButtonPressed() { dashButtonFlag = true; }
        public void OnInvincibilityButtonPressed() { invincibilityButtonFlag = true; }
        public void OnBombButtonPressed() { bombButtonFlag = true; }

        #endregion
    }
}

