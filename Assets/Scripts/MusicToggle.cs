using UnityEngine;
using UnityEngine.UI; // Bắt buộc để chỉnh sửa UI

public class MusicToggle : MonoBehaviour
{
    [Header("Cài đặt hình ảnh")]
    public Sprite btnNoi;   // Hình nút khi đang Bật (Lồi lên)
    public Sprite btnLom;   // Hình nút khi đang Tắt (Lõm xuống/Tối đi)

    private Image buttonImage; // Biến để lấy cái ảnh của nút
    private bool isSoundOn = true; // Trạng thái đang bật hay tắt

    void Start()
    {
        // Tự động lấy component Image trên chính nút này
        buttonImage = GetComponent<Image>();
        
        // Mặc định khi vào game cho nó Nổi lên (Bật)
        // Nếu bạn muốn lưu trạng thái cũ thì dùng PlayerPrefs ở đây
        UpdateUI();
    }

    public void OnClickButton()
    {
        // 1. Đảo ngược trạng thái (đang bật -> tắt, đang tắt -> bật)
        isSoundOn = !isSoundOn;

        // 2. Cập nhật hình ảnh
        UpdateUI();

        // 3. Xử lý âm thanh thực tế
        if (isSoundOn)
        {
            AudioListener.volume = 1; // Mở tiếng
            Debug.Log("Đã Bật - Nút Nổi lên");
        }
        else
        {
            AudioListener.volume = 0; // Tắt tiếng
            Debug.Log("Đã Tắt - Nút Lõm xuống");
        }
    }

    // Hàm phụ để đổi hình
    void UpdateUI()
    {
        if (isSoundOn)
        {
            buttonImage.sprite = btnNoi; // Gán hình Nổi
        }
        else
        {
            buttonImage.sprite = btnLom; // Gán hình Lõm
        }
    }
}