using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EElement
{
    FIRE = 1,
    WATER = 2,
    ELECTRICITY = 3
}
public interface IElemental
{
    EElement[] ElementSockets { get; }


}