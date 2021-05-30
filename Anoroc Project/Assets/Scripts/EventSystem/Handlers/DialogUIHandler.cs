using System;
using DialogSystem;
using JetBrains.Annotations;
using PlayFab.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace EventSystem.Handlers
{
    public class DialogUIHandler : MonoSingleton<DialogUIHandler>
    {
        private DialogObject _currentObj;
        
        [SerializeField] private Button _dialogBox;
        [SerializeField] private TextMeshProUGUI _textField;

        public TextMeshProUGUI TextField => _textField;

        private void OnEnable()
        {
            if (!_dialogBox || !_textField)
            {
                this.enabled = false;
                throw new ArgumentNullException($"Dialog UI Handler is missing critical information!");
            }

            _dialogBox.onClick.AddListener(ContinueDialog);
            
            GlobalEventSystem.Instance.OnDialogStarted += DialogStarted;
            GlobalEventSystem.Instance.OnDialogEnded += DialogEnded;
        }

        private void OnDisable()
        {
            GlobalEventSystem.Instance.OnDialogStarted -= DialogStarted;
            GlobalEventSystem.Instance.OnDialogEnded -= DialogEnded;
        }

        private void DialogStarted(DialogObject dialogObject)
        {
            _currentObj = dialogObject;
            _textField.SetText("");
            _dialogBox.gameObject.SetActive(true);

            ContinueDialog();
        }

        private void ContinueDialog()
        {
            _currentObj.RefreshView();
        }
        
        private void DialogEnded()
        {
            _currentObj = null;
            _dialogBox.gameObject.SetActive(false);
        }
    }
}