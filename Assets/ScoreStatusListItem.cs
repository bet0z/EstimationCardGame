using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreStatusListItem : MonoBehaviour
{
    public UILabel labRoundNo, labRoundStatus;
    public GameObject objEmptyLine;
    public List<UserRoundStatus> lstUserInfos;
    public Dictionary<int, UserRoundStatus> dicUserInfos;

    [HideInInspector]
    public int nRoundNo, nRoundStatus;
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {

    }
    public ScoreStatusListItem(int nRoundNo, int nRoundStatus)
    {
        this.nRoundNo = nRoundNo;
        this.nRoundStatus = nRoundStatus;
        dicUserInfos = new Dictionary<int, UserRoundStatus>();
    }

    public void AddUserRoundStatus(int nUserIndex, int scoreValue, int orderNum, bool isWon, bool isRisk, bool isCaller, BidValue bidValue)
    {
        UserRoundStatus status = new UserRoundStatus(scoreValue, orderNum, isWon, isRisk, isCaller, bidValue);
        dicUserInfos.Add(nUserIndex, status);
        //lstUserInfos[nUserIndex].DisplayData(status);
    }

    public void SetInfo(ScoreStatusListItem item)
    {
        this.nRoundNo = item.nRoundNo;
        this.nRoundStatus = item.nRoundStatus;

        string part = this.nRoundStatus > 0 ? "+" : "";
        labRoundStatus.text = part + this.nRoundStatus.ToString();

        labRoundNo.text = this.nRoundNo.ToString();

        for (int nI = 0; nI < lstUserInfos.Count; nI++)
        {
            lstUserInfos[nI].DisplayData(item.dicUserInfos[nI]);
        }
    }

}
