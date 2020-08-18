// (c) Gijs Sickenga, 2018 //

using UnityEngine;

namespace ElMorro.DialogueSystem
{
    /// <summary>
    /// Advances a paragraph's dialogue when a given game event occurs.
    /// </summary>
    [System.Serializable]
    public class GameEventHandler : ParagraphEventHandler, IGameEventListener
    {
        [Tooltip("The game event that advances this paragraph.")]
        public GameEvent trigger;

        public override void OnLoad()
        {
            base.OnLoad();
            if (trigger != null)
            {
                trigger.RegisterListener(this);
            }
            else
            {
                Debug.LogError("Trigger unset for a dialogue's GameEventHandler, cannot subscribe.");
            }
        }

        public override void OnUnload()
        {
            if (trigger != null)
            {
                trigger.UnregisterListener(this);
            }
            else
            {
                Debug.LogError("Trigger unset for a dialogue's GameEventHandler, cannot unsubscribe.");
            }
            base.OnUnload();
        }

        public void OnEventRaised()
        {
            NextParagraph();
        }
    }
}
