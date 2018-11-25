using System.Collections.Generic;
using System.Linq;

namespace xxoo.Common
{
    public class ListHelper
    {

        //将list按blockSize大小等分，最后多余的单独一份  
        public static List<List<T>> SubList<T>(List<T> list, int blockSize)
        {
            List<List<T>> listGroup = new List<List<T>>();
            int j = blockSize;
            for (int i = 0; i < list.Count; i += blockSize)
            {
                List<T> cList = new List<T>();
                cList = list.Take(j).Skip(i).ToList();
                j += blockSize;
                listGroup.Add(cList);
            }
            return listGroup;
        }

    }
}
