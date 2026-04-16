namespace Shizounu.Library.Dialogue
{
    public enum DialogueExecutionState
    {
        Idle,
        RunningNode,
        WaitingForContinue,
        ReadyToAdvance,
        Cancelled
    }
}