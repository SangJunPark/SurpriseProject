using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using MoreMountains.TopDownEngine;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.UI;

namespace SP
{
    public class SPGUIManager : GUIManager
    {
        public Text CounterText;
        public PlayerInfoBar PlayerInfoUI;

        protected override void Start()
        {
            base.Start();
            CounterText.enabled = false;
        }

        public void SetCounterText(string text)
        {
            CounterText.enabled = !String.IsNullOrEmpty(text);
            CounterText.text = text;
        }

        public void VisiblePlayerInfoUI(bool visible = false)
        {
            PlayerInfoUI.enabled = visible;
        }
    }
}


