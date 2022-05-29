using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatusWindowCtrl : MonoBehaviour
{
    public GameObject objScrollItem;
    public Transform parentGrid;
    public UIScrollView scrollView;

    [HideInInspector]
    public List<ScoreStatusListItem> currentDisplayedItem;

    public void InitItems()
    {
        if (currentDisplayedItem != null) {
            foreach (ScoreStatusListItem item in currentDisplayedItem)
            {
                GameObject.Destroy(item.gameObject);
            }
        }
        
        currentDisplayedItem = new List<ScoreStatusListItem>();
    }

    public void SetData(List<ScoreStatusListItem> lstStatus)
    {
        if(currentDisplayedItem == null)
            currentDisplayedItem = new List<ScoreStatusListItem>();
        InitItems();
        for (int nI = lstStatus.Count-1; nI >=0; nI--)
        {
            currentDisplayedItem.Add(CreateItem(lstStatus[nI]));
        }
        parentGrid.GetComponent<UIGrid>().repositionNow = true;
        parentGrid.GetComponent<UIGrid>().Reposition();
        scrollView.ResetPosition();
    }


    public ScoreStatusListItem CreateItem(ScoreStatusListItem item)
    {
        GameObject obj = parentGrid.AddChild(objScrollItem);
        parentGrid.GetComponent<UIGrid>().repositionNow = true;
        parentGrid.GetComponent<UIGrid>().Reposition();
        ScoreStatusListItem itemInfo = obj.GetComponent<ScoreStatusListItem>();
        itemInfo.SetInfo(item);
        return itemInfo;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
