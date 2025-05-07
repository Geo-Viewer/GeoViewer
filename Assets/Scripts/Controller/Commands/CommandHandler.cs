using System.Collections.Generic;

namespace GeoViewer.Controller.Commands
{
    /// <summary>
    /// A class for handling command execution, as well as undo and redo.
    /// </summary>
    public class CommandHandler
    {
        private readonly List<ICommand> _history = new();
        private int _index;

        /// <summary>
        /// Executes the given <see cref="ICommand"/> and adds it to the history.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public void Execute(ICommand command)
        {
            if (_index < _history.Count)
            {
                _history.RemoveRange(_index, _history.Count - _index);
            }

            command.Execute();
            _history.Add(command);
            _index++;
        }

        /// <summary>
        /// Adds the given <see cref="ICommand"/> to the history without executing it.
        /// </summary>
        /// <param name="command">The command to add.</param>
        public void AddWithoutExecute(ICommand command)
        {
            if (_index < _history.Count)
            {
                _history.RemoveRange(_index, _history.Count - _index);
            }

            _history.Add(command);
            _index++;
        }

        /// <summary>
        /// Undoes the last <see cref="ICommand"/>.
        /// </summary>
        public void Undo()
        {
            if (_index > 0)
            {
                _history[_index - 1].Undo();
                _index--;
            }
        }

        /// <summary>
        /// Redoes the last undone <see cref="ICommand"/>.
        /// </summary>
        public void Redo()
        {
            if (_index < _history.Count)
            {
                _history[_index].Execute();
                _index++;
            }
        }

        /// <summary>
        /// Clears the command history.
        /// </summary>
        public void Clear()
        {
            _history.Clear();
            _index = 0;
        }
    }
}