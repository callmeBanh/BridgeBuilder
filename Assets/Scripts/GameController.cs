using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [Header("Cài đặt chung")]
    public LevelGenerator levelGen; // Kéo script LevelManager vào
    public Transform player;        // Kéo nhân vật vào
    public Transform stickBase;     // Kéo StickBase (Object cha) vào
    public Transform mainCamera;    // Kéo Main Camera vào
    public Animator playerAnimator; // Kéo Animator của Player vào

    [Header("Thông số Game")]
    public float growSpeed = 5f;    // Tốc độ mọc gậy
    public float playerSpeed = 4f;  // Tốc độ chạy của nhân vật

    [Header("Tinh chỉnh vị trí Gậy")]
    // Chỉnh 2 số này nếu gậy bị lệch so với cột
    public float stickXOffset = 0f; 
    public float stickYOffset = 0f; 

    // Biến nội bộ
    private int currentIdx = 0;     
    private bool isInputLocked = false; 
    private bool isHolding = false;     

    void Start()
    {
        // Đợi tạo cột xong mới đặt gậy
        StartCoroutine(WaitAndInit());
    }

    IEnumerator WaitAndInit()
    {
        yield return new WaitForEndOfFrame();
        ResetStickPosition();
    }

    void Update()
    {
        if (isInputLocked) return;
        HandleInput();
    }

    void HandleInput()
    {
        // 1. Bấm chuột -> Reset gậy về vị trí chuẩn bị
        if (Input.GetMouseButtonDown(0))
        {
            isHolding = true;
            stickBase.rotation = Quaternion.identity;
            stickBase.localScale = new Vector3(1, 0, 1);
        }

        // 2. Giữ chuột -> Gậy dài ra
        if (Input.GetMouseButton(0) && isHolding)
        {
            stickBase.localScale += new Vector3(0, growSpeed * Time.deltaTime, 0);
        }

        // 3. Nhả chuột -> Gậy đổ xuống và bắt đầu đi
        if (Input.GetMouseButtonUp(0) && isHolding)
        {
            isHolding = false;
            StartCoroutine(PlayTurn());
        }
    }

    IEnumerator PlayTurn()
    {
        isInputLocked = true; // Khóa không cho bấm nữa

        // --- GIAI ĐOẠN 1: GẬY RƠI ---
        Quaternion targetRot = Quaternion.Euler(0, 0, -90);
        while (Quaternion.Angle(stickBase.rotation, targetRot) > 0.1f)
        {
            stickBase.rotation = Quaternion.RotateTowards(stickBase.rotation, targetRot, 250 * Time.deltaTime);
            yield return null;
        }

        // --- GIAI ĐOẠN 2: TÍNH TOÁN ---
        List<Transform> plats = levelGen.platforms;
        if (currentIdx >= plats.Count - 1) yield break;

        Transform currentPlat = plats[currentIdx];
        Transform targetPlat = plats[currentIdx + 1];

        // Tính toán độ dài gậy và điểm chạm
        float stickLen = stickBase.localScale.y;
        float startEdge = currentPlat.position.x + (currentPlat.localScale.x / 2);
        float stickTipX = startEdge + stickLen;

        // Tính phạm vi an toàn của cột đích
        float targetLeft = targetPlat.position.x - (targetPlat.localScale.x / 2);
        float targetRight = targetPlat.position.x + (targetPlat.localScale.x / 2);

        // Kiểm tra xem gậy có rơi trúng cột đích không
        bool isSuccess = (stickTipX >= targetLeft && stickTipX <= targetRight);

        // --- GIAI ĐOẠN 3: DI CHUYỂN & ANIMATION ---
        // Nếu thắng: Đi đến đích. Nếu thua: Đi hết gậy rồi rớt.
        float moveTargetX = isSuccess ? targetPlat.position.x : stickTipX;

        // Bật Animation đi bộ
        if (playerAnimator != null) playerAnimator.SetBool("isWalking", true);

        // Code di chuyển nhân vật
        while (Mathf.Abs(player.position.x - moveTargetX) > 0.05f)
        {
            player.position = Vector3.MoveTowards(player.position, new Vector3(moveTargetX, player.position.y, player.position.z), playerSpeed * Time.deltaTime);
            yield return null;
        }

        // Tắt Animation đi bộ (về đứng yên)
        if (playerAnimator != null) playerAnimator.SetBool("isWalking", false);

        // --- GIAI ĐOẠN 4: XỬ LÝ KẾT QUẢ (THẮNG/THUA) ---
        if (isSuccess)
        {
            Debug.Log("Thành công! Qua cột " + (currentIdx + 2));
            currentIdx++;

            // Kiểm tra nếu đã đến cột cuối cùng (Cột 5)
            if (currentIdx == levelGen.totalPillars - 1)
            {
                Debug.Log("VICTORY! Chuyển sang màn Win...");
                yield return new WaitForSeconds(0.5f); // Chờ xíu cho mượt
                
                // [QUAN TRỌNG] Chuyển sang Scene tên là "Win"
                SceneManager.LoadScene("Win");
            }
            else
            {
                // Chưa hết game -> Dời Camera đi tiếp
                StartCoroutine(MoveCameraAndReset());
            }
        }
        else
        {
            Debug.Log("Thất bại! Chuyển sang màn Lose...");
            
            // Cho nhân vật rơi xuống vực (Bật vật lý)
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;

            // Chờ 1.5 giây để người chơi thấy mình rớt rồi mới chuyển cảnh
            yield return new WaitForSeconds(1.5f);

            // [QUAN TRỌNG] Chuyển sang Scene tên là "Lose"
            SceneManager.LoadScene("Lose");
        }
    }

    IEnumerator MoveCameraAndReset()
    {
        // Di chuyển Camera sang phải
        float targetCamX = player.position.x + 2f;
        Vector3 camStart = mainCamera.position;
        Vector3 camEnd = new Vector3(targetCamX, camStart.y, camStart.z);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            mainCamera.position = Vector3.Lerp(camStart, camEnd, t);
            yield return null;
        }

        ResetStickPosition(); // Đặt gậy sang cột mới
        isInputLocked = false; // Mở khóa cho chơi tiếp
    }

    void ResetStickPosition()
    {
        if (currentIdx >= levelGen.platforms.Count) return;

        Transform currentPlat = levelGen.platforms[currentIdx];
        
        // Tính mép phải cột
        float edgeX = currentPlat.position.x + (currentPlat.localScale.x / 2);
        // Tính mép trên cột
        float topY = currentPlat.position.y + (currentPlat.localScale.y / 2);

        // Đặt vị trí gậy + Offset tùy chỉnh
        stickBase.position = new Vector3(edgeX + stickXOffset, topY + stickYOffset, 0);
        
        stickBase.rotation = Quaternion.identity;
        stickBase.localScale = new Vector3(1, 0, 1);
    }
}