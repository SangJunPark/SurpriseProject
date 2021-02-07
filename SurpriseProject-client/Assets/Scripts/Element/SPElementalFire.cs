﻿using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using SP;
using UnityEngine;

namespace SP
{
    public struct SPElementalCreationDesc
    {
        public float CoolTime { get; private set; }
        public  float Duration { get; private set; }

        public SPElementalCreationDesc(float coolTime, float duration)
        {
            CoolTime = coolTime;
            Duration = duration;
        }
    }

    public class SPElementalFire : SPElemental
    {
        public override bool CanTransition => true;
        public override float CoolTime => ElemDesc.CoolTime;
        public override float Duration => ElemDesc.Duration;
        public override ElementalTypes ElemType => ElementalTypes.FIRE;

        private SPElementalCreationDesc ElemDesc;

        public SPElementalFire(SPElementalCreationDesc desc)
        {
            ElemDesc = desc;
        }

    }
}