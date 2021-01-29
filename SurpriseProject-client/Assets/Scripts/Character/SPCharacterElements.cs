using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.TopDownEngine;

namespace SP
{
    public class SPCharacterElements : MonoBehaviour
    {
        SPElemental OwnerElemental;
        ElementalTypes OrbElements;

        [SerializeField]
        ElementalTypes BornElements;
        //ElementalTypes 

        void Awake()
        {
            OwnerElemental.OnAttached += () => { };
            OwnerElemental.OnDetached += () => { };
            OwnerElemental.Attach(BornElements);
        }

        //1Frame동안 중첩되는 원소들을 수집한 이후에 한꺼번에 처리
        void LateUpdate()
        {
            SPElementalReactionFactory.LookUpPossibleReaction(OwnerElemental.ElementSockets);
        }
    }
}
