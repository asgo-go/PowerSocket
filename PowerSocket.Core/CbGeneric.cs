using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerSocket.Core
{
    public delegate void CbGeneric();
    public delegate void CbGeneric<T>(T obj);
    public delegate void CbGeneric<T1, T2>(T1 obj1, T2 obj2);
    public delegate void CbGeneric<T1, T2, T3>(T1 obj1, T2 obj2, T3 obj3);
    public delegate void CbGeneric<T1, T2, T3, T4>(T1 obj1, T2 obj2, T3 obj3, T4 obj4);
    public delegate void CbGeneric<T1, T2, T3, T4, T5>(T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5);
    public delegate void CbGeneric<T1, T2, T3, T4, T5, T6>(T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6);
    public delegate void CbGeneric<T1, T2, T3, T4, T5, T6, T7>(T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6, T7 obj7);
    public delegate void CbGeneric<T1, T2, T3, T4, T5, T6, T7, T8>(T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6, T7 obj7, T8 obj8);
    public delegate void CbGeneric<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5, T6 obj6, T7 obj7, T8 obj8, T9 obj9);

}
