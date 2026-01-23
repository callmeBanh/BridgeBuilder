using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.VisualScripting;

public class InputManager : MonoBehaviour
{
    // tham chiếu đến InputField
    public TMP_InputField inputMaDon;
    public TMP_InputField inputTen;
    public TMP_InputField inputSoDienThoai;
    public TextMeshProUGUI thongBaoText;

    //Danh sách các đầu số hợp lệ tại Việt Nam (Cập nhật 2024)
    private readonly string[] validPrefixes = new string[]
    {
        // Viettel (03x, 09x, 086)
        "032", "033", "034", "035", "036", "037", "038", "039",
        "096", "097", "098", "086",
        
        // VinaPhone (08x, 09x)
        "081", "082", "083", "084", "085", "088",
        "091", "094",

        // MobiFone (07x, 08x, 09x)
        "070", "076", "077", "078", "079",
        "089", "090", "093",

        // Vietnamobile (05x, 09x)
        "052", "056", "058", "092",

        // Gmobile (099, 059) & Itelecom (087) & Reddi (055)
        "099", "059", "087", "055"
    };

    public Button btnPlay;
    void Start()
    {
        btnPlay.onClick.AddListener(XuLiDangNhap);
        updateThongBao("", Color.white);
       
    }

    void updateThongBao(string message , Color color)
    {
        if(thongBaoText != null)
        {
            thongBaoText.text = message;
            thongBaoText.color = color;
        }
    }

    

    // Update is called once per frame
    void XuLiDangNhap()
    {
        String maDon = inputMaDon.text;
        String ten = inputTen.text;
        String soDienThoai = inputSoDienThoai.text;

        if(string.IsNullOrEmpty(maDon) || string.IsNullOrEmpty(ten) || string.IsNullOrEmpty(soDienThoai))
        {
           updateThongBao("vui lòng nhập đầy đủ thông tin", Color.red);
            return;
  
        }

        if(ten.Length < 2 || ten.Length > 50)
        {
            updateThongBao("Tên phải từ 2 đến 50 ký tự", Color.red);
            return;
        }

        if(!CheckPhoneNumber(soDienThoai))
        {
            
            return;
        }
        GameSession.PlayerName = ten;
        Debug.Log("Dang nhap thanh cong");
        SceneManager.LoadScene("GamePlay");

    }

 // hàm lấy đầu số
 

    bool CheckPhoneNumber(string phoneNumber)
    {
        //
        if(phoneNumber.Length != 10)
        {
            updateThongBao("Số điện thoại phải có 10 chữ số", Color.red);
            return false;
        }
        if(!long.TryParse(phoneNumber, out _))
        {
            updateThongBao("Số điện thoại chỉ chứa chữ số", Color.red);
            return false;
        }
        string dauSo = phoneNumber.Substring(0, 3);
        if(validPrefixes.Contains(dauSo))
        {
            return true;
        }
        else
        {
            updateThongBao("Đầu số " + dauSo + " không hợp lệ", Color.red);
            return false;
        }

        
    }

}
