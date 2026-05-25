namespace fractalis.Core.Miscellaneous.Phases
{
    public interface IPromptPhase<out T>
    {
        T Run();
    }
}
