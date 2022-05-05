using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    public static UIHealthBar instance { get; private set; }

    public Image healthMask;
    public Image magicMask;
    public Image portrait;

    float originalHealthSize;
    float originalMagicSize;

    private void Awake()
    {
        instance = this;
        
        originalHealthSize = healthMask.rectTransform.rect.width;
        originalMagicSize = magicMask.rectTransform.rect.width;
    }    

    public void SetHealthValue(float value)
    {
        healthMask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalHealthSize * value);
    }

    public void SetMagicValue(float value)
    {
        magicMask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalMagicSize * value);
    }

    public void EnableMagicBar()
    {
        magicMask.gameObject.SetActive(true);
    }

    public void DisableMagicBar()
    {
        magicMask.gameObject.SetActive(false);
    }

    public void UpdatePortrait(Sprite image)
    {
        portrait.sprite = image;
    }
}
