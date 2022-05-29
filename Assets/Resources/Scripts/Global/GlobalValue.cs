using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalValue : MonoBehaviour
{
    [HideInInspector]
    public delegate void DashCall(int userIndex,bool isDashCall);

    [HideInInspector]
    public delegate void BidCall(int userIndex, BidValue bidValue);

    [HideInInspector]
    public delegate void BidConfirm(int userIndex, int confirmValue);

    [HideInInspector]
    public delegate void TurnPlayed(int userIndex, CardBaseInfo info);

    [HideInInspector]
    public delegate void WaitingEnded();

    public static GlobalValue Instance;

    public static int m_nNormalAITime = 500;
    public static int m_nCallerAITime = 1000;

    public Sprite[] m_arrCardFrontSprites;
    public Sprite[] m_arrCardBackSprites;
    public UIRoot m_Root;

    public int CardNumber;
    public int TotalCardNumber;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
