using System;
using EventSystem;
using EventSystem.Handlers;
using Ink.Runtime;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Attributes;

namespace DialogSystem
{
    public class DialogObject : MonoBehaviour
    {
        [InspectorButton("ManuallyStartChain", ButtonWidth = 200)]
        public bool _manuallyStartChainBtn;
        
        [SerializeField] private TextAsset _inkJsonAsset = null;

        private Story _story;
        
        public static event Action<Story> OnCreateStory;

        private Story Story => _story;

        // Creates a new Story object with the compiled story which we can then play!
        public void StartStory()
        {
            _story = new Story(_inkJsonAsset.text);
            OnCreateStory?.Invoke(Story);

            GlobalEventSystem.Instance.DialogStarted(this);
        }

        // This is the main function called every time the story changes. It does a few things:
        // Destroys all the old content and choices.
        // Continues over all the lines of text, then displays all the choices. If there are no choices, the story is finished!
        public void RefreshView()
        {
            if (!Story.canContinue)
            {
                GlobalEventSystem.Instance.DialogEnded();
                return;
            }

           
            string text = Story
                .Continue()
                .Trim();
            
            CreateContentView(text);
        }
        
        // Creates a textbox showing the the line of text
        private void CreateContentView(string text)
        {
            DialogUIHandler.Instance.TextField.SetText($"{text}");
        }
        

        private void ManuallyStartChain()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Can only start dialog in play mode");    
                return;
            }
            
            StartStory();
        }
    }
}