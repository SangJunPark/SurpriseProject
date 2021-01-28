using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SP
{
    public abstract class SPElemental : IElemental
    {
        public ElementalTypes[] ElementSockets => null;

        public delegate void OnAttachedDelegate();
        public delegate void OnDetachedDelegate();

        private OnAttachedDelegate _OnAttached;
        public event OnAttachedDelegate OnAttached
        {
            add
            {
                _OnAttached += value;
            }
            remove
            {
                _OnAttached -= value;
            }
        }

        private OnDetachedDelegate _OnDetached;
        public event OnDetachedDelegate OnDetached
        {
            add
            {
                _OnDetached += value;
            }
            remove
            {
                _OnDetached -= value;
            }
        }

        public virtual void Attach(ElementalTypes elemType)
        {
            _OnAttached?.Invoke();
        }

        public virtual void Detach(ElementalTypes elemType)
        {
            _OnDetached?.Invoke();
        }

        public virtual void Transition()
        {

        }
    }
}