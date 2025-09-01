using System;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    [Header("Controlls")] 
    public KeyCode slowDownKey = KeyCode.Minus;
    public KeyCode speedUpKey = KeyCode.Equals;
    public KeyCode resetKey = KeyCode.Alpha0;
    
    [Header("Settings")]
    public float timeChangeStep = 0.1f; // Bước nhảy mỗi lần thay đổi
    public float minTimeScale = 0.1f;   // Tốc độ chậm nhất
    public float maxTimeScale = 2f;     // Tốc độ nhanh nhất
    
    public TMP_Text timeScaleText;

    // void Start()
    // {
    //     Time.timeScale = minTimeScale;
    // }
    
    private void Update()
    {
        // Giảm tốc độ
        if (Input.GetKeyDown(slowDownKey))
        {
            // Time.timeScale ko dc nhỏ hơn 0
            Time.timeScale = Mathf.Max(minTimeScale, Time.timeScale - timeChangeStep);
        }
        
        // Tăng tốc độ
        if (Input.GetKeyDown(speedUpKey))
        {
            Time.timeScale = Mathf.Max(maxTimeScale, Time.timeScale + timeChangeStep);
        }
        
        // Reset về tốc độ bình thường
        if (Input.GetKeyDown(resetKey))
        {
            Time.timeScale = 1.0f;
        }
        
        // Update ân thanh và UI (optional)
        UpdateAudioPitch();
        UpdateTimeScaleText();
    }

    void UpdateAudioPitch()
    {
        // Làm cho âm thanh cũng chậm/nhanh theo game để đồng bộ
        AudioListener.pause = (Time.timeScale <= 0.01f);
        AudioListener.volume = Time.timeScale;
    }

    void UpdateTimeScaleText()
    {
        if (timeScaleText != null)
        {
            // làm tròn để hiện thị cho đẹp
            timeScaleText.text = "Time Scale: " + Time.timeScale.ToString("F2");
        }
    }
    
    // Đảm bảo khi scene bị hủy, timeScale trở về bth
    private void OnDestroy()
    {
        Time.timeScale = 1.0f;
    }
}
