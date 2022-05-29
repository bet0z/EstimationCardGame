using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreDisplayControl : MonoBehaviour
{
    public TweenText tweenTextLabel;
    public UILabel labScore;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetText(int nResult)
    {
        int nFrom = int.Parse(labScore.text);
        tweenTextLabel.from = nFrom;
        tweenTextLabel.to = nResult;
        tweenTextLabel.ResetToBeginning(); 
        PlayTween();
    }

    public void PlayTween()
    {
        this.gameObject.SetActive(true);
        tweenTextLabel.PlayForward();
    }

    public void OnFinishTweenText()
    {
        StartCoroutine(StartTimer());
    }
    private IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(1.5f);
        this.gameObject.SetActive(false);
    }

}
