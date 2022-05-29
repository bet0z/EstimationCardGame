using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Common
{
    

    public static List<int> UniqueRandomFromList(int maxRange, int totalRandomnoCount,List<int> existList)
    {

        List<int> noList = new List<int>();
        int count = 0;
        Random r = new Random();
        List<int> listRange = new List<int>();
        for (int i = 0; i < totalRandomnoCount; i++)
        {
            listRange.Add(i);
        }
        while (listRange.Count > 0)
        {
            int item = r.Next(maxRange);// listRange[];    
            if (!existList.Contains(item) && !noList.Contains(item) && listRange.Count > 0)
            {
                noList.Add(item);
                listRange.Remove(count);
                count++;
            }
        }
        return noList;
    }

    public static List<int> UniqueRandomNoList(int maxRange, int totalRandomnoCount)
    {

        List<int> noList = new List<int>();
        int count = 0;
        Random r = new Random();
        List<int> listRange = new List<int>();
        for (int i = 0; i < totalRandomnoCount; i++)
        {
            listRange.Add(i);
        }
        while (listRange.Count > 0)
        {
            int item = r.Next(maxRange);// listRange[];    
            if (!noList.Contains(item) && listRange.Count > 0)
            {
                noList.Add(item);
                listRange.Remove(count);
                count++;
            }
        }
        return noList;
    }
}
