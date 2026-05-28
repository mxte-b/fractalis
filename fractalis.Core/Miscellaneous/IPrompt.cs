namespace fractalis.Core.Miscellaneous
{
    public interface IPromptPhase<out T>
    {
        T Run();
    }
}
