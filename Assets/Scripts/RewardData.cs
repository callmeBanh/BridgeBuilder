using UnityEngine;

[CreateAssetMenu(fileName = "NewReward", menuName = "Game/Reward")]
public class RewardData : ScriptableObject
{
    public string rewardName;    // Tên voucher (vd: Giảm 20k)
    public string rewardCode;    // Mã code thực tế (vd: SALE20)
    public Sprite rewardIcon;    // Hình ảnh minh họa
    [Range(0, 100)]
    public float dropRate;       // Tỷ lệ trúng (0-100%)
}