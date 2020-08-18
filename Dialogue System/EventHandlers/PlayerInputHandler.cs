// (c) Gijs Sickenga, 2018 //

using UnityEngine;

namespace ElMorro.DialogueSystem
{
    /// <summary>
    /// Advances a paragraph's dialogue through a specified player input.
    /// The first press fast-forwards the dialogue, the following press advances the dialogue.
    /// </summary>
    [System.Serializable]
    public class PlayerInputHandler : ParagraphEventHandler
    {
        [Tooltip("The button press that advances this paragraph.")]
        public KeyCode interactButton;

        public override void OnLoad()
        {
            base.OnLoad();
            MonoBehaviourSingleton.Instance.OnUpdate += Update;
        }

        public override void OnUnload()
        {
            MonoBehaviourSingleton.Instance.OnUpdate -= Update;
            base.OnUnload();
        }

        private void Update()
        {
            if (Input.GetKeyDown(interactButton))
            {
                if (_hasFinishedPrinting)
                {
                    NextParagraph();
                }
                else
                {
                    FastForward();
                }
            }
        }
    }
}
