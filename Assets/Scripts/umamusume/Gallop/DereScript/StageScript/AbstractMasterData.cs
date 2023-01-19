using System;
using System.Collections;

namespace Stage
{
    public abstract class AbstractMasterData
    {
        public AbstractMasterData(AbstractMasterDatabase db)
        {
        }

        public AbstractMasterData(ArrayList list)
        {
        }

        public abstract void Unload();
    }
}
