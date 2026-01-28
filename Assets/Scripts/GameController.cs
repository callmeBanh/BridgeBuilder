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

    // Cho phép sai số: Gậy ngắn/dài hơn mép 0.2 đơn vị vẫn tính là thắng
    public float tolerance = 0.2f;

    [Header("Tinh chỉnh vị trí Gậy")]
    public float stickXOffset = 0f; // Chỉnh gậy sang trái/phải
    public float stickYOffset = 0f; // Chỉnh gậy lên/xuống

    [Header("Hệ thống Voucher")]
    public List<RewardData> availableRewards;

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
        // 1. Bấm chuột -> Reset gậy
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

        // 3. Nhả chuột -> Gậy đổ xuống
        if (Input.GetMouseButtonUp(0) && isHolding)
        {
            isHolding = false;
            StartCoroutine(PlayTurn());
        }
    }

    IEnumerator PlayTurn()
    {
        isInputLocked = true; // Khóa input

        // --- BƯỚC 1: GẬY RƠI ---
        Quaternion targetRot = Quaternion.Euler(0, 0, -90);
        while (Quaternion.Angle(stickBase.rotation, targetRot) > 0.1f)
        {
            stickBase.rotation = Quaternion.RotateTowards(stickBase.rotation, targetRot, 250 * Time.deltaTime);
            yield return null;
        }

        // --- BƯỚC 2: TÍNH TOÁN THẮNG THUA ---
        List<Transform> plats = levelGen.platforms;
        if (currentIdx >= plats.Count - 1) yield break;

        Transform targetPlat = plats[currentIdx + 1];

        // Tính vị trí đầu gậy dựa trên vị trí THỰC TẾ
        float stickLen = stickBase.localScale.y;
        float stickTipX = stickBase.position.x + stickLen;

        // Tính phạm vi an toàn của cột đích
        float targetLeft = targetPlat.position.x - (targetPlat.localScale.x / 2);
        float targetRight = targetPlat.position.x + (targetPlat.localScale.x / 2);

        // [SỬA LỖI] Dùng UnityEngine.Debug để tránh nhầm lẫn
        UnityEngine.Debug.Log($"Đầu gậy: {stickTipX} | Đích: {targetLeft} -> {targetRight} (Sai số: {tolerance})");

        // Kiểm tra thắng thua
        bool isSuccess = (stickTipX >= targetLeft - tolerance && stickTipX <= targetRight + tolerance);

        // --- BƯỚC 3: DI CHUYỂN & ANIMATION ---
        float moveTargetX = isSuccess ? targetPlat.position.x : stickTipX;

        if (playerAnimator != null) playerAnimator.SetBool("isWalking", true);

        while (Mathf.Abs(player.position.x - moveTargetX) > 0.05f)
        {
            player.position = Vector3.MoveTowards(player.position, new Vector3(moveTargetX, player.position.y, player.position.z), playerSpeed * Time.deltaTime);
            yield return null;
        }

        if (playerAnimator != null) playerAnimator.SetBool("isWalking", false);

        // --- BƯỚC 4: XỬ LÝ KẾT QUẢ ---
        if (isSuccess)
        {
            UnityEngine.Debug.Log("Thành công! Qua cột " + (currentIdx + 2));
            currentIdx++;

            // Kiểm tra cột cuối cùng (Cột 5)
            if (currentIdx == levelGen.totalPillars - 1)
            {
                if (availableRewards != null && availableRewards.Count > 0)
                {
                    // [SỬA LỖI] Dùng UnityEngine.Random để tránh nhầm lẫn
                    int randomIndex = UnityEngine.Random.Range(0, availableRewards.Count);

                    // Kiểm tra xem class GameSession có tồn tại không trước khi dùng
                    // Nếu bạn chưa có class GameSession thì tạm thời comment dòng dưới lại
                    if (typeof(GameSession) != null)
                    {
                        GameSession.WonReward = availableRewards[randomIndex];
                        UnityEngine.Debug.Log("Phần thưởng: " + GameSession.WonReward.rewardName);
                    }
                }

                yield return new WaitForSeconds(0.5f);
                SceneManager.LoadScene("Win");
            }
            else
            {
                StartCoroutine(MoveCameraAndReset());
            }
        }
        else
        {
            UnityEngine.Debug.Log("Thất bại!");
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;

            yield return new WaitForSeconds(1.5f);
            SceneManager.LoadScene("Lose");
        }
    }

    IEnumerator MoveCameraAndReset()
    {
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

        ResetStickPosition();
        isInputLocked = false;
    }

    void ResetStickPosition()
    {
        if (currentIdx >= levelGen.platforms.Count) return;

        Transform currentPlat = levelGen.platforms[currentIdx];

        float edgeX = currentPlat.position.x + (currentPlat.localScale.x / 2);
        float topY = currentPlat.position.y + (currentPlat.localScale.y / 2);

        stickBase.position = new Vector3(edgeX + stickXOffset, topY + stickYOffset, 0);

        stickBase.rotation = Quaternion.identity;
        stickBase.localScale = new Vector3(1, 0, 1);
    }
}