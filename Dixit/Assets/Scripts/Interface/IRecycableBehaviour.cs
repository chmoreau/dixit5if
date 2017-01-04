using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IRecycableBehaviour
{
    void Init(params object[] args);
    void Reset();
}
