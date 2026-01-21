using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Cài đặt")]
    public GameObject platformPrefab; // File mẫu cột
    public Transform startPlatform;   // Cột đầu tiên trên màn hình
    public int totalPillars = 5;      // Tổng số cột muốn chơi

    [Header("Độ khó")]
    public float baseDistance = 2.0f;       // Khoảng cách cơ bản
    public float distanceMultiplier = 0.8f; // Mỗi level xa thêm bao nhiêu
    public float baseWidth = 2.0f;          // Độ rộng cột thứ 2
    public float widthShrink = 0.3f;        // Mỗi level bé đi bao nhiêu

    // Danh sách chứa các cột để GameController biết
    [HideInInspector]
    public List<Transform> platforms = new List<Transform>();

    void Awake()
    {
        CreateLevel();
    }

    void CreateLevel()
    {
        // 1. Thêm cột xuất phát vào danh sách
        platforms.Add(startPlatform);
        
        Vector3 currentPos = startPlatform.position;
        float currentWidth = startPlatform.localScale.x;

        // 2. Tạo các cột tiếp theo
        for (int i = 1; i < totalPillars; i++)
        {
            // TÍNH TOÁN ĐỘ KHÓ
            float gap = baseDistance + (i * distanceMultiplier); // Xa dần
            float newWidth = Mathf.Max(0.4f, baseWidth - (i * widthShrink)); // Bé dần (tối thiểu 0.4)

            // TÍNH VỊ TRÍ (Mép phải cột cũ + khoảng cách + nửa cột mới)
            float newX = currentPos.x + (currentWidth / 2) + gap + (newWidth / 2);
            Vector3 spawnPos = new Vector3(newX, startPlatform.position.y, startPlatform.position.z);

            // TẠO RA CỘT MỚI
            GameObject newPlat = Instantiate(platformPrefab, spawnPos, Quaternion.identity);
            newPlat.transform.localScale = new Vector3(newWidth, startPlatform.localScale.y, 1);
            newPlat.name = "Platform_" + (i + 1);

            // LƯU VÀO DANH SÁCH
            platforms.Add(newPlat.transform);

            // Cập nhật cho vòng lặp sau
            currentPos = spawnPos;
            currentWidth = newWidth;
        }
    }
}