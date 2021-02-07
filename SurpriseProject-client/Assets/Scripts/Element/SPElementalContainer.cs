using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SP
{
    public class SPElementalContainer
    {
        public List<SPElemental> ElementSockets;

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

        public SPElementalContainer()
        {
            ElementSockets = new List<SPElemental>();
        }

        public virtual void Attach(SPElemental elemental)
        {
            ElementSockets.Add(elemental);
            _OnAttached?.Invoke();
        }

        public virtual void Detach(SPElemental elemental)
        {
            ElementSockets.Remove(elemental);
            _OnDetached?.Invoke();
        }

        public virtual void Transition()
        {
        }

        public void UpdateContainer()
        {
            for (int i = 0; i < ElementSockets.Count; ++i)
            {
                ElementSockets[i].UpdateElement();
                if (ElementSockets[i].IsExpired)
                {
                    ElementSockets.Remove(ElementSockets[i]);
                }
            }
        }

        public SPElemental GetElement (ElementalTypes elemType)
        {
            return ElementSockets.Where(T => T.ElemType == elemType).FirstOrDefault();
        }

        public bool HasElement(ElementalTypes elemType)
        {
            return GetElement(elemType) != null;
            //return ElementSockets.Include
        }
    }

}
