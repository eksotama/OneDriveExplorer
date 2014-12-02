using System;

namespace OneDriveExplorer
{
    public class ViewModel
    {
        public enum State
        {
            None,
            Unauthenticated,
            Authenticating,
            Authenticated,
            Mounting,
            Mounted,
            Unmounting
        }

        public EventHandler StateChanged;
        private State _currentState;

        public State CurrentState
        {
            get { return _currentState; }
            set
            {
                PreviousState = _currentState;
                _currentState = value;
                if (StateChanged != null)
                {
                    StateChanged(this, EventArgs.Empty);
                }
            }
        }

        public State PreviousState { get; private set; }
    }
}