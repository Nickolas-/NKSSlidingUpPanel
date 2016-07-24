namespace MvxNKSSlidingUpPanel
{
	public enum VisibilityViewState
	{
		/**
  		 Panel View Controller is not visible on screen
  		 */
		VisibilityStateClosed = 0,
		/**
		 Panel View Controller is minimized - only top panel that defines by Visible Height Zone is visible on screen
		 */
		VisibilityStateMinimized,
		/**
		 Panel View Controller is maximizing
		 */
		VisibilityStateIsMaximizing,
		/**
		 Panel View Controller is minimizing
		 */
		VisibilityStateIsMinimizing,
		/**
		 Panel View Controller is closing
		 */
		VisibilityStateIsClosing,
		/**
		 Panel View Controller is dragging by user
		 */
		VisibilityStateIsDragging,
		/**
		 Panel View Controller is maximized - it's in fullscreen mode
		 */
		VisibilityStateMaximized
	}
}

