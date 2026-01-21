using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PrimeTween;
public class ButtonAnimation : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // tăng tỉ lệ phóng to khi đưa chột vào
    public float scaleUpFactor = 1.2f;

    // thời gian thực hiện hiệu ứng
    public float animationDuration = 0.5f;

   

    // cấu hình sprite ban đầu
    public Sprite fisrtSprite;
    // cấu hình sprite thư 2
    public Sprite secondSprite;

    private Vector3 originalScale;
    private Image image;
    private Sprite originalSprite;
    private bool isToggled = false;

    private void Awake()
    {
        originalScale = transform.localScale;
        image = GetComponent<Image>();
        if(image != null)
        {
            originalSprite = image.sprite;
        }
    }
  
  public void OnPointerEnter(PointerEventData eventData)
    {
        Tween.Scale(transform , originalScale * scaleUpFactor , animationDuration , Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Tween.Scale(transform , originalScale , animationDuration , Ease.OutQuad);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Tạo hiệu ứng "nhún" nhẹ khi bấm (Punch)
        // strength: độ mạnh, duration: thời gian, frequency: độ rung
        Tween.PunchScale(transform, new Vector3(-0.1f, -0.1f, 0), 0.15f, 5);

        if(image != null && secondSprite != null)
        {
            isToggled = !isToggled;
            if(isToggled)
            {
                image.sprite = secondSprite;
            }
            else
            {
                image.sprite = fisrtSprite;
            }
    }



    
     }
 }
