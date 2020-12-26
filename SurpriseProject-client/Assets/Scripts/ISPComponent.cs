using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SPLevelObject
{

    abstract public void Notify();
    virtual public void AddCallback(){

    }
    virtual public void RemoveCallback(){

    }
}
