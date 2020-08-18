// (c) Gijs Sickenga, 2018 //

using System;
using UnityEngine;

namespace ElMorro.DialogueSystem
{
    /// <summary>
    /// Handles all events pertaining to a DialogueParagraph,
    /// including the manner in which the paragraph's dialogue is advanced.
    /// </summary>
    public abstract class ParagraphEventHandler
    {
        public enum Types
        {
            GameEvent,
            PlayerInput,
            Timer
        }

        [NonSerialized]
        private DialogueParagraph _targetParagraph;
        /// <summary>
        /// The paragraph to which this event handler is attached.
        /// Null until the event handler is first referenced through its paragraph.
        /// </summary>
        public DialogueParagraph TargetParagraph
        {
            get
            {
                return _targetParagraph;
            }
        }

        protected bool _hasFinishedPrinting = false;
        protected bool _nextParagraphCalled = false;

        private delegate void AdvanceEvent();
        // Called when the paragraph's text should be fast-forwarded.
        private AdvanceEvent FastForwardEvent;
        // Called when the next paragraph should appear.
        private AdvanceEvent NextParagraphEvent;

        /// <summary>
        /// Informs the ParagraphEventHandler to what DialogueParagraph it is attached.
        /// </summary>
        public void AssignParagraph(DialogueParagraph targetParagraph)
        {
            _targetParagraph = targetParagraph;
        }

        /// <summary>
        /// Resets the event handler to its default state after it has been unloaded from a DialogueWindow.
        /// </summary>
        private void Reset()
        {
            _hasFinishedPrinting = false;
            _nextParagraphCalled = false;
        }

        /// <summary>
        /// Adds the given listener to the text advancement events.
        /// </summary>
        public void AddListener(IParagraphAdvanceEventListener listener)
        {
            FastForwardEvent += listener.FastForward;
            NextParagraphEvent += listener.NextParagraph;
        }

        /// <summary>
        /// Removes the given listener from the text advancement events.
        /// </summary>
        public void RemoveListener(IParagraphAdvanceEventListener listener)
        {
            FastForwardEvent -= listener.FastForward;
            NextParagraphEvent -= listener.NextParagraph;
        }

        /// <summary>
        /// Invokes FastForwardCallback.
        /// Should be called when the paragraph's text is fast-forwarded in some way.
        /// </summary>
        protected void FastForward()
        {
            if (FastForwardEvent != null)
            {
                FastForwardEvent();
            }
        }

        /// <summary>
        /// Invokes NextParagraphCallback if it hasn't been invoked yet.
        /// Should be called when the next paragraph should appear.
        /// </summary>
        protected void NextParagraph()
        {
            if (!_nextParagraphCalled)
            {
                if (NextParagraphEvent != null)
                {
                    NextParagraphEvent();
                }
                _nextParagraphCalled = true;
            }
        }

        /// <summary> Called when the paragraph is loaded into a DialogueWindow. </summary>
        public virtual void OnLoad()
        {
            Reset();
        }
        /// <summary> Called when the paragraph's text starts appearing in a DialogueWindow. </summary>
        public virtual void OnStartPrinting()
        {
            _targetParagraph.Events.OnStartPrinting.Invoke();
        }
        /// <summary> Called when the paragraph's text is fully printed in a DialogueWindow. </summary>
        public virtual void OnFinishPrinting()
        {
            if (!_hasFinishedPrinting)
            {
                _targetParagraph.Events.OnFinishPrinting.Invoke();
                _hasFinishedPrinting = true;
            }
        }
        /// <summary> Called when the paragraph is unloaded from a DialogueWindow. </summary>
        public virtual void OnUnload()
        {
            Reset();
        }
    }
}
