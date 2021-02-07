using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;

namespace SP
{
    public struct SPElementalCreationDesc
    {
        public float CoolTime { get; private set; }
        public float Duration { get; private set; }

        public SPElementalCreationDesc(float coolTime, float duration)
        {
            CoolTime = coolTime;
            Duration = duration;
        }
    }

    public abstract class SPElemental : IElemental
    {
        private float CurrentTime;
        public virtual bool IsActive { get; set; }
        public virtual bool IsExpired { get; set; }


        public abstract bool CanTransition { get; }
        public abstract float CoolTime { get; }
        public abstract float Duration { get; }
        public abstract ElementalTypes ElemType { get; }

        public SPElemental()
        {
            CurrentTime = 0;
            IsExpired = false;

        }

        public virtual void TransitionTo(SPElementalContainer to)
        {
            //to.Attach(this);
            //SPElementalReaction
        }

        public virtual void TransitionFrom(SPElementalContainer from)
        {

            //from.Attach(this);
        }
        public void UpdateElement()
        {
            if (IsActive)
            {
                CurrentTime += Time.fixedDeltaTime;
                if (CurrentTime >= Duration)
                {
                    IsActive = false;
                    IsExpired = true;
                }
            }
        }
    }
}