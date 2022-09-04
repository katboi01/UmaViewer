using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gallop
{
    public interface FaceLoadCallBack
    {
        public abstract void CallBack(FaceDrivenKeyTarget target);
    }
}
