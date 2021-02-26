using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SP{
    [RequireComponent(typeof(Animator))]
    public class SPAnimationEventReceiver : MonoBehaviour
    {
        void AnimEvent(string desc)
        {
            Debug.Log(desc);
        }
    }
}

