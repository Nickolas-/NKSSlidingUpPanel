using System;
using Foundation;
using UIKit;

namespace NKSSlidingUpPanel
{
	public class SlidingUpPanelController : UIViewController, IUIGestureRecognizerDelegate
	{
		private bool _isCleanedUp;
		private bool _isInitedTouch;
		private bool _dropShadow;
		private bool _isDraggingEnabled;
		private nfloat _shadowRadius;
		private float _shadowOpacity;

		private bool _shouldDrag;

		private nfloat _visibleZoneHeight;

		private bool _shouldShiftMainView;
		private bool _shouldOverlapMainView;

		private UIView _mainView;
		private UIView _panelView;
		private NSLayoutConstraint _mainViewTopConstraint;
		private NSLayoutConstraint _mainViewHeightConstraint;
		private NSLayoutConstraint _panelViewTopConstraint;
		private UIGestureRecognizer.Token _panGestureToken;
		private CoreGraphics.CGPoint _touchPointOffset;

		public event EventHandler<VisibilityViewStateEventArgs> OnVisibilityViewStateChanged;

		public VisibilityViewState VisibilityViewState { get; protected set; }
		public UIPanGestureRecognizer PanGestureRecognizer { get; protected set; }

		public nfloat AnimationDuration { get; set; }

		private bool IsPanelViewScrollView { get { return _panelView is UIScrollView; } }

		public virtual bool ShouldShiftMainView {
			get { return _shouldShiftMainView; }
			set { UpdateShouldShiftMainView (value); }
		}

		public virtual bool ShouldOverlapMainView {
			get { return _shouldOverlapMainView; }
			set { UpdateShouldOverlapMainView (value); }
		}

		public virtual nfloat VisibleZoneHeight {
			get { return _visibleZoneHeight; }
			set { UpdateVisibleZoneHeight (value); }
		}

		public virtual bool IsDraggingEnabled {
			get { return _isDraggingEnabled; }
			set {
				_isDraggingEnabled = value;
				DraggingEnabled ();
			}
		}

		public virtual bool DropShadow {
			get { return _dropShadow; }
			set {
				_dropShadow = value;
				UpdateShadow ();
			}
		}

		public virtual nfloat ShadowRadius {
			get { return _shadowRadius; }
			set {
				_shadowRadius = value;
				UpdateShadow ();
			}
		}

		public virtual float ShadowOpacity {
			get { return _shadowOpacity; }
			set {
				_shadowOpacity = value;
				UpdateShadow ();
			}
		}

		public virtual UIView MainView {
			get { return _mainView; }
			set { UpdateMainView (value); }
		}

		public virtual UIView PanelView {
			get { return _panelView; }
			set { UpdatePanelView (value); }
		}

		private void UpdateMainView (UIView newManinView)
		{
			if (newManinView == null) {
				return;
			}

			if (this._mainView != null && this._mainView.Superview != null) {
				this._mainView.RemoveFromSuperview ();
			}

			this._mainView = newManinView;
			this.View.AddSubview (this._mainView);

			UpdateMainViewConstraints ();

			if (this._panelView != null) {
				this.View.BringSubviewToFront (this._panelView);
			}
		}

		private void UpdatePanelView (UIView newPanelView)
		{
			if (newPanelView == null) {
				return;
			}

			if (this._panelView != null && this._panelView.Superview != null) {
				this._panelView.RemoveFromSuperview ();
			}

			this._panelView = newPanelView;
			this.View.AddSubview (this._panelView);

			UpdatePanelViewConstraints ();
			UpdateFrameBottomOffset (VisibleZoneHeight);
			VisibilityViewState = VisibilityViewState.VisibilityStateMinimized;
			UpdateShadow ();
			DraggingEnabled ();
		}

		private void UpdateMainViewConstraints ()
		{
			this._mainView.TranslatesAutoresizingMaskIntoConstraints = false;

			this._mainViewTopConstraint = NSLayoutConstraint.Create (this._mainView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Top, 1, 0);
			this._mainViewHeightConstraint = NSLayoutConstraint.Create (this._mainView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Height, 1, 0);
			var left = NSLayoutConstraint.Create (this._mainView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Left, 1, 0);
			var width = NSLayoutConstraint.Create (this._mainView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Width, 1, 0);

			this.View.AddConstraints (new [] { _mainViewTopConstraint, _mainViewHeightConstraint, left, width });
			this.View.LayoutIfNeeded ();
		}

		private void UpdatePanelViewConstraints ()
		{
			this._panelView.TranslatesAutoresizingMaskIntoConstraints = false;

			this._panelViewTopConstraint = NSLayoutConstraint.Create (this._panelView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Top, 1, 0);
			var height = NSLayoutConstraint.Create (this._panelView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Height, 1, 0);
			var left = NSLayoutConstraint.Create (this._panelView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Left, 1, 0);
			var right = NSLayoutConstraint.Create (this._panelView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Right, 1, 0);
			var width = NSLayoutConstraint.Create (this._panelView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Width, 1, 0);

			this.View.AddConstraints (new [] { _panelViewTopConstraint, height, left, right, width });
			this.View.LayoutIfNeeded ();
		}

		private void UpdateFrameBottomOffset (nfloat bottomOffset)
		{
			if (this._panelView != null && _panelView.Superview != null) {
				this._panelViewTopConstraint.Constant = -bottomOffset;
				if (ShouldShiftMainView && VisibilityViewState != VisibilityViewState.VisibilityStateIsClosing) {
					this._mainViewTopConstraint.Constant = -bottomOffset + VisibleZoneHeight;
				} else {
					this._mainViewTopConstraint.Constant = 0f;
				}

				if (ShouldOverlapMainView || VisibilityViewState == VisibilityViewState.VisibilityStateIsClosing) {
					this._mainViewHeightConstraint.Constant = 0f;
				} else {
					this._mainViewHeightConstraint.Constant = -VisibleZoneHeight;
				}
				this.View.LayoutIfNeeded ();
			}
		}

		private void UpdateShadow ()
		{
			if (DropShadow) {
				if (this._panelView != null && this._panelView.Superview != null) {
					_panelView.Layer.MasksToBounds = false;
					_panelView.Layer.ShadowOffset = new CoreGraphics.CGSize (0, 0);
					_panelView.Layer.ShadowRadius = ShadowRadius > 0 ? ShadowRadius : 20.0f;
					_panelView.Layer.ShadowOpacity = ShadowOpacity > 0 ? ShadowOpacity : 0.5f;
				}
			} else {
				_panelView.Layer.MasksToBounds = false;
				_panelView.Layer.ShadowOffset = new CoreGraphics.CGSize (0, 0);
				_panelView.Layer.ShadowRadius = 0;
				_panelView.Layer.ShadowOpacity = 0;
			}
		}

		private void DraggingEnabled ()
		{
			if (!IsDraggingEnabled) {
				if (PanGestureRecognizer != null && _panelView != null) {
					_panelView.RemoveGestureRecognizer (PanGestureRecognizer);
				}
			} else {
				if (PanGestureRecognizer == null) {
					PanGestureRecognizer = new UIPanGestureRecognizer ();
					_panGestureToken = PanGestureRecognizer.AddTarget (() => HandleGesture (PanGestureRecognizer));
					PanGestureRecognizer.Delegate = this;
				}
				if (_panelView != null) {
					_panelView.AddGestureRecognizer (PanGestureRecognizer);
				}
			}
		}

		private void VisibilityViewStateChanged (VisibilityViewState state)
		{
			VisibilityViewState = state;
			OnVisibilityViewStateChanged?.Invoke (this, new VisibilityViewStateEventArgs (VisibilityViewState));
		}

		private void UpdateVisibleZoneHeight (nfloat offset)
		{
			_visibleZoneHeight = NMath.Min (offset, this.View.Frame.Height);
			UpdateFrameBottomOffset (_panelView.Frame.Location.Y);
		}

		private void UpdateShouldOverlapMainView (bool shouldOverlap)
		{
			_shouldOverlapMainView = shouldOverlap;
			if (VisibilityViewState == VisibilityViewState.VisibilityStateClosed) {
				ClosePanelControllerAnimated (false, null);
			} else {
				MinimizePanelControllerAnimated (false, null);
			}
		}

		private void UpdateShouldShiftMainView (bool shouldShift)
		{
			_shouldShiftMainView = shouldShift;
			if (VisibilityViewState == VisibilityViewState.VisibilityStateClosed) {
				ClosePanelControllerAnimated (false, null);
			} else {
				MinimizePanelControllerAnimated (false, null);
			}
		}

		private void InstallPanelViewControllerConstraintToTop ()
		{
			this.View.RemoveConstraint (_panelViewTopConstraint);
			this._panelViewTopConstraint = NSLayoutConstraint.Create (this._panelView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Top, 1, 0);
			this.View.AddConstraint (_panelViewTopConstraint);
		}

		private void InstallPanelViewControllerConstraintToBottom ()
		{
			this.View.RemoveConstraint (_panelViewTopConstraint);
			this._panelViewTopConstraint = NSLayoutConstraint.Create (this._panelView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Bottom, 1, -VisibleZoneHeight);
			this.View.AddConstraint (_panelViewTopConstraint);
		}

		public void MaximizePanelControllerAnimated (bool animated, Action copmletion)
		{
			MaximizePanelControllerAnimated (animated, null, copmletion);
		}

		public void MinimizePanelControllerAnimated (bool animated, Action copmletion)
		{
			MinimizePanelControllerAnimated (animated, null, copmletion);
		}

		public void ClosePanelControllerAnimated (bool animated, Action copmletion)
		{
			ClosePanelControllerAnimated (animated, null, copmletion);
		}

		public void MaximizePanelControllerAnimated (bool animated, Action animation, Action copmletion)
		{
			VisibilityViewStateChanged (VisibilityViewState.VisibilityStateIsMaximizing);
			nfloat animationDuration = AnimationDuration > 0 ? AnimationDuration : 0.3f;

			MovePanelControllerWithBottomOffset (this._panelView.Frame.Size.Height, animated, animationDuration, animation, (finished) => {

				VisibilityViewStateChanged (VisibilityViewState.VisibilityStateMaximized);
				InstallPanelViewControllerConstraintToTop ();

				if (copmletion != null) {
					copmletion ();
				}
			});
		}

		public void MinimizePanelControllerAnimated (bool animated, Action animation, Action copmletion)
		{
			nfloat bottomOffset = this._panelViewTopConstraint.Constant == 0 ? VisibleZoneHeight - this._panelView.Frame.Size.Height : VisibleZoneHeight;
			VisibilityViewStateChanged (VisibilityViewState.VisibilityStateIsMinimizing);
			nfloat animationDuration = AnimationDuration > 0 ? AnimationDuration : 0.3f;
			MovePanelControllerWithBottomOffset (bottomOffset, animated, animationDuration, animation, (finished) => {

				VisibilityViewStateChanged (VisibilityViewState.VisibilityStateMinimized);
				InstallPanelViewControllerConstraintToBottom ();

				if (copmletion != null) {
					copmletion ();
				}
			});
		}

		public void ClosePanelControllerAnimated (bool animated, Action animation, Action copmletion)
		{
			VisibilityViewStateChanged (VisibilityViewState.VisibilityStateIsClosing);
			nfloat animationDuration = AnimationDuration > 0 ? AnimationDuration : 0.3f;

			MovePanelControllerWithBottomOffset (-this._panelView.Frame.Size.Height, animated, animationDuration, animation, (finished) => {

				VisibilityViewStateChanged (VisibilityViewState.VisibilityStateClosed);

				if (copmletion != null) {
					copmletion ();
				}
			});
		}

		private void MovePanelControllerWithBottomOffset (nfloat bottomOffset, bool animated, nfloat animationDuration, Action animations, UICompletionHandler completion)
		{
			UIView.AnimateNotify (animated ? (double)animationDuration : 0,
								  0,
								 VisibilityViewState == VisibilityViewState.VisibilityStateIsMinimizing ? 0.82f : 1,
								 0.4f,
								 UIViewAnimationOptions.CurveEaseOut,
								  () => {
									  if (animations != null) {
										  animations ();
									  }
									  UpdateFrameBottomOffset (bottomOffset);

								  },
								  completion);
		}

		private void HandleGesture (UIPanGestureRecognizer gesture)
		{
			if (IsPanelViewScrollView && (this._panelView as UIScrollView).ContentOffset.Y > 0 && VisibilityViewState == VisibilityViewState.VisibilityStateMaximized) {
				this._touchPointOffset = gesture.LocationInView (this._panelView);
				this._shouldDrag = true;
				this._isInitedTouch = true;
				return;
			}

			if (gesture.State == UIGestureRecognizerState.Began) {
				this._touchPointOffset = gesture.LocationInView (this._panelView);
				this._shouldDrag = true;
				InstallPanelViewControllerConstraintToBottom ();
			}

			CoreGraphics.CGPoint vel = gesture.VelocityInView (this.View);
			if (gesture.State == UIGestureRecognizerState.Ended || gesture.State == UIGestureRecognizerState.Cancelled) {
				if (this._shouldDrag) {
					this._shouldDrag = false;
					this._touchPointOffset = CoreGraphics.CGPoint.Empty;
					if (vel.Y > 0f) {
						MinimizePanelControllerAnimated (true, null);
					} else {
						MaximizePanelControllerAnimated (true, null);
					}
				}
			} else {
				if (this._shouldDrag) {
					if (IsPanelViewScrollView && this._isInitedTouch) {
						InstallPanelViewControllerConstraintToBottom ();
						this._isInitedTouch = false;
					}
					VisibilityViewStateChanged (VisibilityViewState.VisibilityStateIsDragging); ;
					nfloat offset = NMath.Max (ShouldOverlapMainView ? 0f : VisibleZoneHeight, this._panelView.Frame.Size.Height - gesture.LocationInView (this.View).Y + this._touchPointOffset.Y);
					UpdateFrameBottomOffset (NMath.Min (offset, this._panelView.Frame.Size.Height));
				}
			}

		}

		[Export ("gestureRecognizer:shouldRecognizeSimultaneouslyWithGestureRecognizer:")]
		public bool ShouldRecognizeSimultaneously (UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
		{
			return IsPanelViewScrollView &&
				(VisibilityViewState == VisibilityViewState.VisibilityStateMaximized || VisibilityViewState == VisibilityViewState.VisibilityStateIsMinimizing || VisibilityViewState == VisibilityViewState.VisibilityStateIsDragging) &&
														  (gestureRecognizer == PanGestureRecognizer || gestureRecognizer == (_panelView as UIScrollView).PanGestureRecognizer) &&
														  (otherGestureRecognizer == PanGestureRecognizer || otherGestureRecognizer == (_panelView as UIScrollView).PanGestureRecognizer);
		}

		protected virtual void CleanUp ()
		{
			if (!this._isCleanedUp) {
				PanGestureRecognizer.RemoveTarget (_panGestureToken);
				PanGestureRecognizer.Delegate = null;
				if (MainView != null) {
					MainView.RemoveFromSuperview ();
				}
				if (PanelView != null) {
					PanelView.RemoveFromSuperview ();
					PanelView.RemoveGestureRecognizer (PanGestureRecognizer);
				}
				this._isCleanedUp = true;
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				CleanUp ();
			}
			base.Dispose (disposing);
		}
	}
}