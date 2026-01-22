using UnityEngine;

public class MusicDontDestroy : MonoBehaviour
{
    // Biến static để kiểm tra xem đã có nhạc chưa
    public static MusicDontDestroy instance;

    void Awake()
    {
        // Nếu chưa có MusicManager nào tồn tại -> giữ cái này lại
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Lệnh quan trọng: Không hủy object này khi chuyển scene
        }
        // Nếu đã có rồi (khi quay lại màn Start) -> hủy cái mới đi để tránh bị trùng 2 bài nhạc
        else
        {
            Destroy(gameObject);
        }
    }
}