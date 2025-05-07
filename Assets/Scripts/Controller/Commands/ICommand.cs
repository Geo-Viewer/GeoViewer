namespace GeoViewer.Controller.Commands
{
    /// <summary>
    /// An interface for commands. Commands can be executed and undone.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        public void Execute();

        /// <summary>
        /// Undoes the command.
        /// </summary>
        public void Undo();
    }
}