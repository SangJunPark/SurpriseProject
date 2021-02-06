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
        float CoolTime { get; }
        float Duration { get; }
        bool IsActive { get; set; }

        bool IsExpired { get; set; }
        ElementalTypes ElemType { get; }
        //ElementalTypes[] ElementSockets { get; }
    }
}