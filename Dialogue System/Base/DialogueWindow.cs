// (c) Gijs Sickenga, 2018 //

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ElMorro.DialogueSystem
{
    /// <summary>
    /// Represents a single UI dialogue window.
    /// Contains references to the elements that make up a dialogue window.
    /// Handles the drawing of dialogue inside the window.
    /// </summary>
    [DisallowMultipleComponent]
    public class DialogueWindow : MonoBehaviour, IParagraphAdvanceEventListener
    {
        [SerializeField]
        [Tooltip("The image element where a character's portrait should appear. Null if the window has none.")]
        private Image _portraitImage;
        
        [SerializeField]
        [Tooltip("The title text element of the window, where a character's name might appear. Null if the window has none.")]
        private Text _titleText;

        [SerializeField]
        [Tooltip("The body text element of the window, where dialogue should appear.")]
        private Text _bodyText;

        private Coroutine _printRoutine = null;

        // This is a nested class so the events are drawn in a foldout menu in the inspector.
        [System.Serializable]
        public class WindowStateEvents
        {
            /// <summary>
            /// Raised when the dialogue window is opened.
            /// </summary>
            public UnityEvent OnWindowOpen = new UnityEvent();
            /// <summary>
            /// Raised when the dialogue window is closed.
            /// Will pass a bool indicating whether all dialogue was exhausted.
            /// </summary>
            public BoolEvent OnWindowClose = new BoolEvent();

            ~WindowStateEvents()
            {
                // Remove UnityEvent listeners.
                OnWindowOpen.RemoveAllListeners();
                OnWindowClose.RemoveAllListeners();
            }
        }

        public WindowStateEvents WindowEvents
        {
            get
            {
                return Dialogue.WindowEvents;
            }
        }

        private DialogueSequence _dialogue;
        public DialogueSequence Dialogue
        {
            get
            {
                return _dialogue;
            }

            private set
            {
                bool onWindowOpen = false;
                if (_dialogue == null)
                {
                    // Set dialogue for the first time, so the window has just opened.
                    onWindowOpen = true;
                }
                _dialogue = value;

                if (onWindowOpen)
                {
                    WindowEvents.OnWindowOpen.Invoke();
                }

                CurrentParagraphNode = _dialogue.Paragraphs.First;
            }
        }

        private LinkedListNode<DialogueParagraph> _currentParagraphNode;
        public LinkedListNode<DialogueParagraph> CurrentParagraphNode
        {
            get
            {
                return _currentParagraphNode;
            }

            private set
            {
                // Unsubscribe from current paragraph if it was set.
                if (CurrentParagraph != null)
                {
                    UnsubscribeFromEventHandler(CurrentParagraph);
                }

                // Assign new paragraph.
                _currentParagraphNode = value;

                if (CurrentParagraph == null)
                {
                    // End of LinkedList reached, so DialogueSequence is exhausted.
                    Close(true);
                }
                else
                {
                    // Subscribe to new paragraph.
                    SubscribeToEventHandler(CurrentParagraph);
                    StartPrinting(CurrentParagraph);
                }
            }
        }
        
        public DialogueParagraph CurrentParagraph
        {
            get
            {
                if (_currentParagraphNode != null)
                {
                    return _currentParagraphNode.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Should be called manually when the DialogueWindow is attached to a GameObject.
        /// </summary>
        public void Initialize(DialogueSequence dialogue)
        {
            this.Dialogue = dialogue;
        }

        /// <summary>
        /// Subscribe to EventHandler's dialogue advancement callbacks.
        /// </summary>
        private void SubscribeToEventHandler(DialogueParagraph paragraph)
        {
            // Subscribe to callbacks.
            paragraph.EventHandler.AddListener(this);

            // Notify EventHandler that it is currently being loaded.
            paragraph.EventHandler.OnLoad();
        }

        /// <summary>
        /// Unsubscribe from EventHandler's dialogue advancement callbacks.
        /// </summary>
        private void UnsubscribeFromEventHandler(DialogueParagraph paragraph)
        {
            // Unsubscribe from callbacks.
            paragraph.EventHandler.RemoveListener(this);

            // Notify EventHandler that it is currently being unloaded.
            paragraph.EventHandler.OnUnload();
        }

        /// <summary>
        /// Changes the title for this window if it has a title element.
        /// </summary>
        private void SetTitle(string titleString, Color titleColor)
        {
            if (_titleText != null)
            {
                // Unhide UI.
                _titleText.enabled = true;
                _titleText.text = titleString;
                _titleText.color = titleColor;
            }
        }

        /// <summary>
        /// Sets the character portrait sprite for this window if it has a portrait element.
        /// </summary>
        private void SetPortrait(Sprite portraitSprite)
        {
            if (_portraitImage != null)
            {
                // Unhide UI.
                _portraitImage.enabled = true;
                _portraitImage.sprite = portraitSprite;
            }
        }

        /// <summary>
        /// Displays the given NPC in the window's title and portrait elements.
        /// If passed a null NPC, the title and portrait will be disabled.
        /// </summary>
        private void SetSpeaker(NPC speaker)
        {
            if (speaker != null)
            {
                SetTitle(speaker.name, speaker.NameColor);
                SetPortrait(speaker.Portrait);
            }
            else
            {
                // Hide UI.
                _titleText.enabled = false;
                _portraitImage.enabled = false;
            }
        }

        /// <summary>
        /// Fast-forwards the current paragraph, instantly printing it on the DialogueWindow in its entirety.
        /// </summary>
        public void FastForward()
        {
            StopPrinting();
            _bodyText.text = CurrentParagraph.Text;

            CurrentParagraph.EventHandler.OnFinishPrinting();
        }

        /// <summary>
        /// Starts printing the next paragraph in the DialogueSequence.
        /// </summary>
        public void NextParagraph()
        {
            // TODO (Gijs): Maybe call FastForward() here?
            CurrentParagraphNode = CurrentParagraphNode.Next;
        }

        /// <summary>
        /// Closes the DialogueWindow.
        /// </summary>
        /// <param name="markDialogueCompleted">Whether the OnWindowClose event should say that all dialogue was exhausted.</param>
        public void Close(bool markDialogueCompleted)
        {
            WindowEvents.OnWindowClose.Invoke(markDialogueCompleted);
            Destroy(gameObject);
        }

        /// <summary>
        /// Starts a coroutine that prints the current paragraph.
        /// </summary>
        private void StartPrinting(DialogueParagraph paragraph)
        {
            SetSpeaker(paragraph.Speaker);
            paragraph.EventHandler.OnStartPrinting();
            _printRoutine = StartCoroutine(PrintRoutine(paragraph));
        }

        /// <summary>
        /// Stops the coroutine that is printing the current paragraph.
        /// </summary>
        private void StopPrinting()
        {
            if (_printRoutine != null)
            {
                StopCoroutine(_printRoutine);
                _printRoutine = null;
            }
        }

        private IEnumerator PrintRoutine(DialogueParagraph paragraph)
        {
            // Print the first character instantly.
            float charactersToPrint = 1;
            int charactersPrinted = 0;

            // Clear existing text.
            _bodyText.text = string.Empty;

            while (charactersPrinted < paragraph.Text.Length)
            {
                charactersToPrint += Time.deltaTime * paragraph.PrintSpeed;
                for (int i = Mathf.FloorToInt(charactersToPrint); i > 0; i--)
                {
                    if (charactersPrinted < paragraph.Text.Length)
                    {
                        _bodyText.text += paragraph.Text[charactersPrinted];
                        charactersPrinted++;
                        charactersToPrint--;
                    }
                    else
                    {
                        // All characters printed, so wait default delay and then finish.
                        yield return new WaitForSecondsRealtime(1f / paragraph.PrintSpeed);
                        CurrentParagraph.EventHandler.OnFinishPrinting();
                        yield break;
                    }
                }
                
                yield return new WaitForSecondsRealtime(1f / paragraph.PrintSpeed);
            }

            CurrentParagraph.EventHandler.OnFinishPrinting();
            yield break;
        }
    }
}
