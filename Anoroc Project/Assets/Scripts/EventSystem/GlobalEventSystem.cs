using System;
using Actions;
using CharacterSystem;
using CombatSystem.SpellSystem;
using DialogSystem;
using InventorySystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;
using Utilities.Attributes;

namespace EventSystem
{
    public class GlobalEventSystem : MonoSingleton<GlobalEventSystem>
    {

        #region Fields

        [SerializeField] private MainActions _inputActions;

        
        
        [SerializeField, InspectorButton("StartGame")] private bool _startGame;
        [SerializeField, InspectorButton("EndGame")] private bool _endGame;
        [SerializeField, InspectorButton("OpenMenu")] private bool _openMenu;
        [SerializeField, InspectorButton("CloseMenu")] private bool _closeMenu;

        #endregion

        #region Properties

        public MainActions InputActions
        {
            get
            {
                if (_inputActions == null) _inputActions = new MainActions();
                return _inputActions;
            }
        }

        #endregion

        #region Events

        public event Action OnGameStart;
        public event Action OnGameEnd;

        public event Action OnGameMenuOpened;
        public event Action OnGameMenuClosed;

        public event Action<SpellArchetype, int> OnPlayerArchetypeLevelUp;
        
        public event Action OnPlayerDied;
        public event Action OnPlayerWon;
        
        public event Action<DialogObject> OnDialogStarted;
        public event Action OnDialogEnded;

        public event Action<ZoneScript> OnPlayerEnterZone;
        public event Action<ZoneScript> OnPlayerExitZone;

        public event Action<InventoryObject, Character> OnCharacterPickedItemUp;

        public event Action<Scene> OnLevelChanged;
        public event Action<Scene> OnLevelLoaded;
        public event Action<Scene> OnLevelUnloaded;

        public event Action<Scene> OnBeforeLevelLoad;
        public event Action<Scene> OnBeforeLevelUnLoad;

        #endregion

        #region Unity Events

        //public UnityEvent _onGameStart;
        //public UnityEvent _onGameEnd;

        #endregion

        #region Event Callers

        public void StartGame()
        {
            OnGameStart?.Invoke();
        }

        public void EndGame()
        {
            OnGameEnd?.Invoke();
        }

        public void OpenMenu()
        {
            OnGameMenuOpened?.Invoke();
        }

        public void CloseMenu()
        {
            OnGameMenuClosed?.Invoke();
        }

        public void LoadLevel(Scene s, LoadSceneMode mode = LoadSceneMode.Single)
        {
            OnBeforeLevelLoad?.Invoke(s);
            SceneManager.LoadSceneAsync(s.buildIndex, mode);
        }

        public void UnLoadLevel(Scene s)
        {
            OnBeforeLevelUnLoad?.Invoke(s);
            SceneManager.UnloadSceneAsync(s.buildIndex);
        }
        
        public void PlayerArchetypeHasLeveledUp(SpellArchetype arg1, int arg2)
        {
            OnPlayerArchetypeLevelUp?.Invoke(arg1, arg2);
        }
        
        public void PlayerEnteredZone(ZoneScript obj)
        {
            OnPlayerEnterZone?.Invoke(obj);
        }
        
        public void PlayerExitedZone(ZoneScript obj)
        {
            OnPlayerExitZone?.Invoke(obj);
        }
        
        public void DialogStarted(DialogObject dialogObject)
        {
            OnDialogStarted?.Invoke(dialogObject);
        }
        
        public void DialogEnded()
        {
            OnDialogEnded?.Invoke();
        }
        
        public void CharacterHasPickedItemUp(InventoryObject droppedItem, Character character)
        {
            OnCharacterPickedItemUp?.Invoke(droppedItem, character);
        }

        private void SceneManager_sceneLoaded(Scene sc, LoadSceneMode d)
        {
            OnLevelLoaded?.Invoke(sc);
            OnLevelChanged?.Invoke(sc);
        }

        private void SceneManager_sceneUnloaded(Scene sc)
        {
            OnLevelUnloaded?.Invoke(sc);
            OnLevelChanged?.Invoke(sc);
        }

        
        
        #endregion


        private new void Awake()
        {
            base.Awake();
            if (Instance != this)
                return;

            DontDestroyOnLoad(this.gameObject);

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
        }


        
    }

}