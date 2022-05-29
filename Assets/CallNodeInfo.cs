using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FlowerType
{

}
    
public class CallNodeInfo : MonoBehaviour
{   
    public bool isCallNumber;

    public int nCallNumber;
    public Suit FlowerType;

    public GameObject SelectedBackground;
    public GameObject DisableLabel, EnableLabel;

    [HideInInspector]
    public bool isSelected = false;

    [HideInInspector]
    public bool isEnabled = true;

    // Start is called before the first frame update
    void Start()
    {
        isSelected = false;
        isEnabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSelected)
            SelectedBackground.SetActive(true);
        else
            SelectedBackground.SetActive(false);
    }

    public void Enable()
    {
        this.gameObject.SetActive(true);
        DisableLabel.SetActive(false);
        EnableLabel.SetActive(true);
        isEnabled = true;
    }

    public void Disable()
    {
        DisableLabel.SetActive(true);
        EnableLabel.SetActive(false);
        isEnabled = false;
    }

    public void HideBack()
    {
        isSelected = false;
    }

    public void ShowBack()
    {
        if(isEnabled)
            isSelected = true;
    }

}
