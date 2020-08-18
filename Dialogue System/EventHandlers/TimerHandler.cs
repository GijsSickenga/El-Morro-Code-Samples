// (c) Gijs Sickenga, 2018 //

using System.Collections;
using UnityEngine;

namespace ElMorro.DialogueSystem
{
    /// <summary>
    /// Advances a paragraph's dialogue a given amount of seconds after it has finished printing.
    /// </summary>
    [System.Serializable]
    public class TimerHandler : ParagraphEventHandler
    {
        [Tooltip("The time to wait in seconds before printing the next paragraph after this paragraph is fully printed.")]
        public FloatReference timer;

        public override void OnFinishPrinting()
        {
            MonoBehaviourSingleton.Instance.StartCoroutine(DelayedNextParagraph());
            base.OnFinishPrinting();
        }

        private IEnumerator DelayedNextParagraph()
        {
            yield return new WaitForSecondsRealtime(timer.Value);
            NextParagraph();
            yield break;
        }
    }
}
