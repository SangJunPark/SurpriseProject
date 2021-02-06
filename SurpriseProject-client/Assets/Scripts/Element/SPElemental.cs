using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SP
{
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
                }
            }
        }
    }
}