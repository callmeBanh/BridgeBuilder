using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [Header("Cài đặt chung")]
    public LevelGenerator levelGen; // Kéo script LevelManager vào đây
    public Transform player;        // Kéo nhân vật vào
    public Transform stickBase;     // Kéo StickBase (Object cha) vào
    public Transform mainCamera;    // Kéo Main Camera vào

    [Header("Thông số Game")]
    public float growSpeed = 5f;    // Tốc độ mọc gậy
    public float playerSpeed = 4f;  // Tốc độ chạy của nhân vật

    [Header("Tinh chỉnh vị trí Gậy (QUAN TRỌNG)")]
    // Chỉnh 2 số này để gậy khớp với mép cột
    public float stickXOffset = 0f; // Dịch sang trái (số âm) hoặc phải (số dương)
    public float stickYOffset = 0f; // Dịch lên (số dương) hoặc xuống (số âm)

    // Biến nội bộ (Không cần chỉnh)
    private int currentIdx = 0;     // Đang đứng ở cột số 0
    private bool isInputLocked = false; // Khóa nút bấm khi nhân vật đang chạy
    private bool isHolding = false;     // Kiểm tra có đang giữ chuột không

    void Start()
    {
        // Đặt gậy vào vị trí cột đầu tiên ngay khi vào game
        StartCoroutine(WaitAndInit());
    }

    // Đợi 1 chút để LevelGenerator tạo xong cột rồi mới đặt gậy
    IEnumerator WaitAndInit()
    {
        yield return new WaitForEndOfFrame();
        ResetStickPosition();
    }

    void Update()
    {
        // Nếu đang bị khóa (nhân vật đang chạy/rơi) thì không cho bấm
        if (isInputLocked) return;

        HandleInput();
    }

    void HandleInput()
    {
        // 1. Bắt đầu giữ chuột
        if (Input.GetMouseButtonDown(0))
        {
            isHolding = true;
            // Đảm bảo gậy dựng đứng và chiều dài về 0
            stickBase.rotation = Quaternion.identity;
            stickBase.localScale = new Vector3(1, 0, 1);
        }

        // 2. Đang giữ chuột -> Gậy dài ra
        if (Input.GetMouseButton(0) && isHolding)
        {
            stickBase.localScale += new Vector3(0, growSpeed * Time.deltaTime, 0);
        }

        // 3. Nhả chuột -> Gậy đổ xuống
        if (Input.GetMouseButtonUp(0) && isHolding)
        {
            isHolding = false;
            StartCoroutine(PlayTurn());
        }
    }

    IEnumerator PlayTurn()
    {
        isInputLocked = true; // Khóa không cho bấm chuột nữa

        // --- BƯỚC 1: CHO GẬY RƠI (XOAY -90 ĐỘ) ---
        Quaternion targetRot = Quaternion.Euler(0, 0, -90);
        while (Quaternion.Angle(stickBase.rotation, targetRot) > 0.1f)
        {
            stickBase.rotation = Quaternion.RotateTowards(stickBase.rotation, targetRot, 250 * Time.deltaTime);
            yield return null;
        }

        // --- BƯỚC 2: TÍNH TOÁN THẮNG THUA ---
        List<Transform> plats = levelGen.platforms;
        
        // Nếu đã hết cột để đi -> Dừng
        if (currentIdx >= plats.Count - 1) yield break;

        Transform currentPlat = plats[currentIdx];     // Cột đang đứng
        Transform targetPlat = plats[currentIdx + 1];  // Cột đích

        // Tính vị trí đầu gậy
        float stickLen = stickBase.localScale.y;
        // Mép phải cột hiện tại (nơi đặt chân gậy)
        float startEdge = currentPlat.position.x + (currentPlat.localScale.x / 2); 
        // Vị trí đầu gậy chạm đất = Mép phải + Chiều dài gậy
        float stickTipX = startEdge + stickLen; // (Không cộng Offset vào đây để tính cho chuẩn xác với logic)

        // Tính phạm vi an toàn của cột đích
        float targetLeft = targetPlat.position.x - (targetPlat.localScale.x / 2);
        float targetRight = targetPlat.position.x + (targetPlat.localScale.x / 2);

        // Kiểm tra trúng đích
        bool isSuccess = (stickTipX >= targetLeft && stickTipX <= targetRight);

        // --- BƯỚC 3: DI CHUYỂN NHÂN VẬT ---
        // Nếu thắng: Đi đến giữa cột tiếp theo. Nếu thua: Đi hết cây gậy rồi rớt.
        float moveTargetX = isSuccess ? targetPlat.position.x : stickTipX;

        // Code chạy nhân vật
        while (Mathf.Abs(player.position.x - moveTargetX) > 0.05f)
        {
            // Chỉ di chuyển trục X, giữ nguyên Y
            player.position = Vector3.MoveTowards(player.position, new Vector3(moveTargetX, player.position.y, player.position.z), playerSpeed * Time.deltaTime);
            yield return null;
        }

        // --- BƯỚC 4: XỬ LÝ KẾT QUẢ ---
        if (isSuccess)
        {
            Debug.Log("Thành công! Qua cột " + (currentIdx + 2));
            currentIdx++; // Tăng chỉ số cột lên

            // Nếu là cột cuối cùng (Cột 5)
            if (currentIdx == levelGen.totalPillars - 1)
            {
                Debug.Log("VICTORY! BẠN ĐÃ CHIẾN THẮNG!");
                SceneManager.LoadScene("Win");
                // Có thể hiện bảng Win UI ở đây
            }
            else
            {
                // Chưa hết game -> Dời camera và Reset gậy
                StartCoroutine(MoveCameraAndReset());
            }
        }
        else
        {
            Debug.Log("Thất bại! Rớt xuống vực.");
            // Bật vật lý để nhân vật rơi tự do
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;
            SceneManager.LoadScene("Lose");
        }
    }

    IEnumerator MoveCameraAndReset()
    {
        // 1. Di chuyển Camera sang phải
        float targetCamX = player.position.x + 2f; // Camera luôn đi trước nhân vật 1 đoạn
        Vector3 camStart = mainCamera.position;
        Vector3 camEnd = new Vector3(targetCamX, camStart.y, camStart.z);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            mainCamera.position = Vector3.Lerp(camStart, camEnd, t);
            yield return null;
        }

        // 2. Đặt gậy sang cột mới (Reset)
        ResetStickPosition();
        
        // 3. Mở khóa để chơi tiếp
        isInputLocked = false;
    }

    // --- HÀM QUAN TRỌNG: ĐẶT VỊ TRÍ GẬY ---
    void ResetStickPosition()
    {
        // Lấy cột hiện tại nhân vật đang đứng
        Transform currentPlat = levelGen.platforms[currentIdx];

        // Tính mép phải của cột: Vị trí tâm + (Nửa độ rộng)
        float edgeX = currentPlat.position.x + (currentPlat.localScale.x / 2);
        
        // Tính mép trên của cột: Vị trí tâm + (Nửa chiều cao)
        float topY = currentPlat.position.y + (currentPlat.localScale.y / 2);

        // Đặt vị trí gậy + CỘNG THÊM OFFSET CỦA BẠN
        stickBase.position = new Vector3(edgeX + stickXOffset, topY + stickYOffset, 0);

        // Reset trạng thái gậy
        stickBase.rotation = Quaternion.identity;    // Dựng đứng
        stickBase.localScale = new Vector3(1, 0, 1); // Chiều dài về 0
    }
}