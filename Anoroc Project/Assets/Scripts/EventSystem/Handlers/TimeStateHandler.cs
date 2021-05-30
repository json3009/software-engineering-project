using System;
using DialogSystem;
using UnityEngine;
using Utilities;

namespace EventSystem.Handlers
{
    public class TimeStateHandler : MonoSingleton<TimeStateHandler>
    {
        private void OnEnable()
        {
            GlobalEventSystem.Instance.OnDialogStarted += DialogStarted;
            GlobalEventSystem.Instance.OnDialogEnded += DialogEnded;
        }

        private void OnDisable()
        {
            GlobalEventSystem.Instance.OnDialogStarted -= DialogStarted;
            GlobalEventSystem.Instance.OnDialogEnded -= DialogEnded;
        }

        private void DialogEnded()
        {
            Time.timeScale = 1;
        }
        
        private void DialogStarted(DialogObject obj)
        {
            Time.timeScale = 0;
        }
        
    }
}