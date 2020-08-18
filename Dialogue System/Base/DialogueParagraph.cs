// (c) Gijs Sickenga, 2018 //

using UnityEngine;
using UnityEngine.Events;

namespace ElMorro.DialogueSystem
{
    /// <summary>
    /// Represents a skippable string of dialogue shown in a single dialogue window screen.
    /// </summary>
    [System.Serializable]
    public class DialogueParagraph
    {
        [SerializeField]
        private ParagraphEventHandler.Types _skipMethod = ParagraphEventHandler.Types.PlayerInput;
        /// <summary>
        /// The manner in which the paragraph's dialogue is advanced.
        /// </summary>
        public ParagraphEventHandler.Types SkipMethod
        {
            get
            {
                return _skipMethod;
            }
        }

        [SerializeField]
        [Tooltip("Advances the paragraph's dialogue when a game event is fired.")]
        private GameEventHandler _gameEventHandler;

        [SerializeField]
        [Tooltip("Advances the paragraph's dialogue when a button is pressed.")]
        private PlayerInputHandler _playerInputHandler;

        [SerializeField]
        [Tooltip("Advances the paragraph's dialogue a given amount of seconds after it is fully printed.")]
        private TimerHandler _timerHandler;

        /// <summary>
        /// Handles all events pertaining to the paragraph,
        /// including the manner in which the paragraph's dialogue is advanced.
        /// </summary>
        public ParagraphEventHandler EventHandler
        {
            get
            {
                ParagraphEventHandler eventHandler = null;
                switch (SkipMethod)
                {
                    case ParagraphEventHandler.Types.GameEvent:
                        eventHandler = _gameEventHandler;
                        break;
                    case ParagraphEventHandler.Types.PlayerInput:
                        eventHandler = _playerInputHandler;
                        break;
                    case ParagraphEventHandler.Types.Timer:
                        eventHandler = _timerHandler;
                        break;
                }
                eventHandler.AssignParagraph(this);
                return eventHandler;
            }
        }

        [SerializeField]
        [Tooltip("The NPC speaking this dialogue. If left empty, the dialogue will not be spoken by an NPC.")]
        private NPC _speaker;
        /// <summary>
        /// The NPC speaking this dialogue. If left empty, the dialogue will not be spoken by an NPC.
        /// </summary>
        public NPC Speaker
        {
            get
            {
                return _speaker;
            }
        }

        [SerializeField]
        [Tooltip("The amount of characters to print per second when the dialogue is spoken.")]
        private FloatReference _printSpeed;
        /// <summary>
        /// The amount of characters to print per second when the dialogue is spoken.
        /// </summary>
        public float PrintSpeed
        {
            get
            {
                return _printSpeed.Value;
            }
        }

        [SerializeField] [MaxLength(140)]
        private string _text;
        public string Text
        {
            get
            {
                return _text;
            }
        }

        // This is a nested class so the events are drawn in a foldout menu in the inspector.
        [System.Serializable]
        public class PrintEvents
        {
            /// <summary>
            /// Raised when the paragraph starts being printed in a dialogue window.
            /// </summary>
            public UnityEvent OnStartPrinting = new UnityEvent();
            /// <summary>
            /// Raised when the paragraph is fully printed in a dialogue window.
            /// </summary>
            public UnityEvent OnFinishPrinting = new UnityEvent();

            ~PrintEvents()
            {
                // Remove UnityEvent listeners.
                OnStartPrinting.RemoveAllListeners();
                OnFinishPrinting.RemoveAllListeners();
            }
        }

        [SerializeField]
        [Tooltip("UnityEvents that can be used to trigger in-game events when dialogue starts or finishes.\n\n" +
                 "OnStartPrinting: Raised when the paragraph starts being printed in a dialogue window.\n\n" +
                 "OnFinishPrinting: Raised when the paragraph is fully printed in a dialogue window.")]
        private PrintEvents _events;
        /// <summary>
        /// UnityEvents that can be used to trigger in-game events when dialogue starts or finishes.
        /// </summary>
        public PrintEvents Events
        {
            get
            {
                return _events;
            }
        }
    }
}
