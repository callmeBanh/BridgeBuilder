using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Linq;
using Unity.VisualScripting;
public class InputManager : MonoBehaviour
{
    // tham chiếu đến InputField
    public TMP_InputField inputMaDon;
    public TMP_InputField inputTen;
    public TMP_InputField inputSoDienThoai;

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
    }

    // Update is called once per frame
    void XuLiDangNhap()
    {
        String maDon = inputMaDon.text;
        String ten = inputTen.text;
        String soDienThoai = inputSoDienThoai.text;

        if(string.IsNullOrEmpty(maDon) || string.IsNullOrEmpty(ten) || string.IsNullOrEmpty(soDienThoai))
        {
            Debug.Log("Vui long nhap day du thong tin");
            return;
  
        }

        if(ten.Length < 2 || ten.Length > 50)
        {
            Debug.Log("Ten phai tu 2 den 50 ky tu");
            return;
        }

        if(!CheckPhoneNumber(soDienThoai))
        {
            return;
        }

        Debug.Log("Dang nhap thanh cong");
        SceneManager.LoadScene("GamePlay");

    }

 // hàm lấy đầu số
 

    bool CheckPhoneNumber(string phoneNumber)
    {
        //
        if(phoneNumber.Length != 10)
        {
            Debug.Log("So dien thoai phai co 10 chu so");
            return false;
        }
        if(!long.TryParse(phoneNumber, out _))
        {
            Debug.Log("So dien thoai chi chua chu so");
            return false;
        }
        string dauSo = phoneNumber.Substring(0, 3);
        if(validPrefixes.Contains(dauSo))
        {
            return true;
        }
        else
        {
            Debug.Log("Dau so khong hop le");
            return false;
        }

        
    }
}
