using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VictoryManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI txtUserName;    // Gắn UserGreeting vào đây
    public TextMeshProUGUI txtVoucherCode; // Gắn VoucherCode vào đây
    public Image imgVoucher;               // Gắn VoucherIcon vào đây
    public GameObject dialogPanel;         // Gắn đối tượng Dialog vào đây

    void Start()
    {
        dialogPanel.SetActive(true);

    // Kiểm tra xem có phần thưởng nào được lưu không
        if (GameSession.WonReward != null)
        {
            txtUserName.text = "Chúc mừng: " + GameSession.PlayerName;
            txtVoucherCode.text = "Mã nhận thưởng: " + GameSession.WonReward.rewardCode;
            imgVoucher.sprite = GameSession.WonReward.rewardIcon;
            
            // (Tùy chọn) Hiện thêm tên Voucher nếu muốn
            // Debug.Log("Voucher: " + GameSession.WonReward.rewardName);
        }
    }

    // Hàm này sẽ được gọi khi người chơi đi qua 5 cột
    public void DisplayWin(string name, string code, Sprite icon)
    {
        dialogPanel.SetActive(true);
        txtUserName.text = "Chúc mừng: " + name;
        txtVoucherCode.text = "Mã nhận thưởng: " + code;
        imgVoucher.sprite = icon;
    }

    // Hàm này gắn vào sự kiện OnClick của nút X
    public void ClaimRewardAndExit()
    {
        // 1. Thực hiện lưu dữ liệu (Gọi sang một hệ thống quản lý Database hoặc API)
        //SaveDataToSystem();

        // 2. Thông báo nhận quà thành công (Optional)
        Debug.Log("Đã nhận thưởng thành công!");

        // quay về Scene Đăng nhập/Menu chính:
        SceneManager.LoadScene("StartGame"); 
    }

}