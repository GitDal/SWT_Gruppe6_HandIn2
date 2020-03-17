using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LadeSkab
{
    public interface IIdentificationKeyReader<TIdType>
    {
        event EventHandler<TIdType> IdDetectedEvent;
        void OnIdRead(TIdType id);
    }
}
