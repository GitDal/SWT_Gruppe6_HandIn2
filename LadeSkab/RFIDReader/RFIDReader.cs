using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LadeSkab
{
    public class RFIDReader : IIdentificationKeyReader<int>
    {
        public event EventHandler<int> IdDetectedEvent;

        public void OnIdDetected(int id)
        {
            IdDetectedEvent?.Invoke(this, id);
        }

        public void DetectId(int id)
        {
            if(IdIsValid((id)))
                OnIdDetected(id);
        }

        private bool IdIsValid(int id)
        {
            if (id > 0)
                return true;
            else
                return false;
        }

    }
}
