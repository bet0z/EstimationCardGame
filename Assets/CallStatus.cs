using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallStatus : MonoBehaviour
{
    public UILabel labCallNumber;
    public List<GameObject> lstCardType;

    public void Disable()
    {
        this.gameObject.GetComponent<TweenTransform>().ResetToBeginning();
        this.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        this.gameObject.GetComponent<TweenTransform>().ResetToBeginning();
        this.gameObject.GetComponent<TweenTransform>().enabled = true;
    }
    public void SetValue(BidValue value)
    {
        labCallNumber.text = value.value.ToString();
        for(int nI = 0; nI < lstCardType.Count; nI++)
        {
            if(nI == (int)(value.suit))
                lstCardType[nI].SetActive(true);
            else
                lstCardType[nI].SetActive(false);
        }
        this.gameObject.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
