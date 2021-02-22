using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SP;
using MoreMountains.Tools;

namespace SP
{
    public enum InputType
    {
        LCLICK,
        LHOLD,
        RCLICK,
        SHIFT
    }
    public class SPInputManager : MMSingleton<SPInputManager>
    {
        public bool InputEnabled = true;
        Stack<InputType> KeyStacks;
        public Vector2 InputMovement;
        // Start is called before the first frame update

        protected override void Awake()
        {
            base.Awake();
            Initialization();
        }

        void Initialization()
        {
            KeyStacks = new Stack<InputType>();
        }


        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            UpdateInternal();
        }

        void UpdateInternal()
        {
            if (!InputEnabled)
                return;

            InputMovement.x = Input.GetAxis("Horizontal");
            InputMovement.y = Input.GetAxis("Vertical");

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                KeyStacks.Push(InputType.SHIFT);
            }

            if (Input.GetMouseButtonDown(0))
            {
                KeyStacks.Push(InputType.LCLICK);
            }

            if (Input.GetMouseButtonDown(2))
            {
                KeyStacks.Push(InputType.RCLICK);
            }

            while(KeyStacks.Count > 0)
            {
                InputType iType = KeyStacks.Pop();
            }
            KeyStacks.Clear();
        }
    }
}
