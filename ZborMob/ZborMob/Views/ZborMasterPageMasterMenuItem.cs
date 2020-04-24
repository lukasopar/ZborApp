using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZborMob.Views
{

    public class ZborMasterPageMasterMenuItem
    {
        public ZborMasterPageMasterMenuItem()
        {
            TargetType = typeof(ZborMasterPageMasterMenuItem);
        }
        public int Id { get; set; }
        public string Title { get; set; }

        public Type TargetType { get; set; }
    }
}