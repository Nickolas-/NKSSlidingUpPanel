using System;
namespace MvxNKSSlidingUpPanel
{
	public class VisibilityViewStateEventArgs : EventArgs
	{
		public VisibilityViewStateEventArgs (VisibilityViewState state)
		{
			VisibilityViewState = state;
		}

		public VisibilityViewState VisibilityViewState { get; private set; }
	}
}

