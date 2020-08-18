// (c) Gijs Sickenga, 2018 //

namespace ElMorro.DialogueSystem
{
    /// <summary>
    /// Interface for receiving paragraph advancement callbacks from a ParagraphEventHandler.
    /// </summary>
    public interface IParagraphAdvanceEventListener
    {
        void FastForward();
        void NextParagraph();
    }
}
