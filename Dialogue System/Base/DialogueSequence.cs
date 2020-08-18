// (c) Gijs Sickenga, 2018 //

using System.Collections.Generic;
using UnityEngine;

// TODO (Gijs):
// - Check if all events work: window events, paragraph events, etc.
// - Add Cinemachine camera control to DialogueSequence and DialogueParagraph.
// - Add easy way to pause game during dialogue.
// - Add preview of button to press under dialogue for PlayerInput dialogue.

// Nice-to-have:
// - Write custom PropertyDrawer for DialogueParagraphLinkedList to manually adjust list order in inspector.
// - Add a way to resume dialogue from a previously left-off point.
// - Add a way to move back in dialogue, or provide optional ways to traverse a dialogue tree, rather than strictly linearly.
// - Add a way to hide a dialogue window, pausing the dialogue.
// - Fancy text visualisation in dialogue window (bold, italics, underlined, colored, etc.).
// - Emotions.
// - Animated portraits.
// - Hub-and-Spokes dialogue.

namespace ElMorro.DialogueSystem
{
    /// <summary>
    /// Represents a specific dialogue sequence, made up of DialogueParagraphs.
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Dialogue Sequence", menuName = "Dialogue/Sequence", order = 0)]
    public class DialogueSequence : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The UI prefab for the dialogue window.")]
        private GameObject _windowPrefab = null;

        [SerializeField]
        [Tooltip("UnityEvents that can be used to trigger in-game events when the window the dialogue is displayed in changes states.\n\n" +
                 "OnWindowOpen: Raised when the dialogue window is opened.\n\n" +
                 "OnWindowClose: Raised when the dialogue window is closed. Will pass a bool indicating whether all dialogue was exhausted.")]
        private DialogueWindow.WindowStateEvents _windowEvents;
        /// <summary>
        /// UnityEvents that can be used to trigger in-game events when the window the dialogue is displayed in changes states.
        /// </summary>
        public DialogueWindow.WindowStateEvents WindowEvents
        {
            get
            {
                return _windowEvents;
            }
        }

        [SerializeField]
        [Tooltip("The ordered list of dialogue paragraphs in this sequence.")]
        private DialogueParagraphLinkedList _paragraphs = new DialogueParagraphLinkedList();
        public LinkedList<DialogueParagraph> Paragraphs
        {
            get
            {
                return _paragraphs.LinkedList;
            }
        }

        /// <summary>
        /// Opens a DialogueWindow and plays the DialogueSequence in it.
        /// Returns the DialogueWindow that was opened, null if no window could be opened.
        /// </summary>
        /// <param name="uiParent">The UI object to parent the DialogueWindow in.</param>
        public DialogueWindow PlaySequence(RectTransform uiParent)
        {
            if (_windowPrefab == null)
            {
                Debug.LogError("Window prefab unset for " + name + ", cannot play dialogue.");
                return null;
            }

            var windowInstance = Instantiate(_windowPrefab, uiParent);
            var dialogueWindow = windowInstance.GetComponent<DialogueWindow>();
            dialogueWindow.Initialize(this);

            return dialogueWindow;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_windowPrefab != null)
            {
                // Make sure _windowPrefab has a DialogueWindow script attached.
                if (_windowPrefab.GetComponent<DialogueWindow>() == null)
                {
                    _windowPrefab = null;
                }
            }
        }
#endif
    }
}
