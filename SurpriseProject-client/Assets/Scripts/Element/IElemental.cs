using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SP
{
    public enum ElementalTypes
    {
        FIRE = 1,
        WATER = 2,
        ELECTRICITY = 3
    }
    public interface IElemental
    {
        ElementalTypes[] ElementSockets { get; }
    }
}