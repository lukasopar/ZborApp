using System;
using System.Collections.Generic;
using System.Text;

namespace ZborMob
{
    public interface INotification
    {
        void CreateNotification(Guid id, String title, String message);
        void DeleteNotification(List<int> ids);

    }
}
