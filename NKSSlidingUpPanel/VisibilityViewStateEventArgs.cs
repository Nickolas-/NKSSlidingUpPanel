using System;
namespace NKSSlidingUpPanel
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

