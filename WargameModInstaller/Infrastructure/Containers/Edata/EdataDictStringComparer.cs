using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Infrastructure.Containers.Edata
{
    public class EdataDictStringComparer : IComparer<String>
    {
        private static Dictionary<int, int> weights = CreateWeights();

        private static Dictionary<int, int> CreateWeights()
        {
            var weights = new Dictionary<int, int>();

            //'\' has to be before digits and chars. 
            //new adjusted code overwriets "*", 
            //so  all thoses should be pushed one up to make free space for it...
            weights.Add('\\', -50); 

            //weights.Add('_', -50);

            return weights;
        }

        public int Compare(String x, String y)
        {
            if (x == y)
            {
                return 0;
            }

            if (String.IsNullOrEmpty(x))
            {
                return -1;
            }

            if (String.IsNullOrEmpty(y))
            {
                return 1;
            }

            int index = 0;
            while (true)
            {
                if(index >= x.Length || index >= y.Length)
                {
                    break;
                }

                int x1 = x[index];
                int y1 = y[index];

                int xWeight;
                int yWeight;

                weights.TryGetValue(x1, out xWeight);
                weights.TryGetValue(y1, out yWeight);

                x1 += xWeight;
                y1 += yWeight;

                if (x1 < y1)
                {
                    return -1;
                }
                else if (x1 > y1)
                {
                    return 1;
                }

                index++;
            }

            return x.Length < y.Length ? -1 : 1;

        }
    }
}
