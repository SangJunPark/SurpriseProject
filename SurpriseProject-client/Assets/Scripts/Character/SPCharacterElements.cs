using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.TopDownEngine;

namespace SP
{
    public class SPCharacterElements : MonoBehaviour
    {
        SPElementalContainer ElemContainer;
        
        void Awake()
        {
            ElemContainer = new SPElementalContainer();
            ElemContainer.OnAttached += OnAttachedElem;
            ElemContainer.OnAttached += OnDetachedElem;

            SPElementalFire fireElem = new SPElementalFire(new SPElementalCreationDesc(5,5));
            ElemContainer.Attach(fireElem);
            fireElem.IsActive = true;
        }

        //1Frame동안 중첩되는 원소들을 수집한 이후에 한꺼번에 처리
        void LateUpdate()
        {
            //SPElementalReactionFactory.LookUpPossibleReaction(OwnerElemental.ElementSockets);
        }

        void Update()
        {
            ElemContainer.UpdateContainer();
            if (ElemContainer.HasElement(ElementalTypes.FIRE))
            {
                var spgui = (SPGUIManager)GUIManager.Instance;
                spgui.VisiblePlayerInfoUI(true);
            }
            else
            {
                var spgui = (SPGUIManager)GUIManager.Instance;
                spgui.VisiblePlayerInfoUI(false);
            }
        }

        void OnAttachedElem()
        {

        }

        void OnDetachedElem()
        {

        }
    }
}
