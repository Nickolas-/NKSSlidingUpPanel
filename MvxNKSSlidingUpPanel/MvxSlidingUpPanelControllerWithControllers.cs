using System;
using Foundation;
using MvvmCross.Core.ViewModels;
using MvvmCross.iOS.Views;
using UIKit;

namespace MvxNKSSlidingUpPanel
{
	public class MvxSlidingUpPanelControllerWithControllers : MvxViewController, IUIGestureRecognizerDelegate
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

		private UIViewController _mainViewController;
		private UIViewController _panelViewController;
		private NSLayoutConstraint _mainViewTopConstraint;
		private NSLayoutConstraint _mainViewHeightConstraint;
		private NSLayoutConstraint _panelViewTopConstraint;
		private UIGestureRecognizer.Token _panGestureToken;
		private CoreGraphics.CGPoint _touchPointOffset;

		public MvxSlidingUpPanelControllerWithControllers () 
		{

		}

		public MvxSlidingUpPanelControllerWithControllers (IntPtr handle)
			: base (handle)
		{
		}

		protected MvxSlidingUpPanelControllerWithControllers (string nibName, NSBundle bundle)
			: base (nibName, bundle)
		{
		}

		public event EventHandler<VisibilityViewStateEventArgs> OnVisibilityViewStateChanged;

		public VisibilityViewState VisibilityViewState { get; protected set; }
		public UIPanGestureRecognizer PanGestureRecognizer { get; protected set; }

		private bool IsPanelViewControllerScrollView { get { return _panelViewController is MvxTableViewController; } }

		public nfloat AnimationDuration { get; set; }

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

		public virtual UIViewController MainViewController {
			get { return _mainViewController; }
			set { UpdateMainViewController (value); }
		}

		public virtual UIViewController PanelViewController {
			get { return _panelViewController; }
			set { UpdatePanelViewController (value); }
		}

		private void UpdateMainViewController (UIViewController newManinViewController)
		{
			if (newManinViewController == null) {
				return;
			}

			if (this._mainViewController != null && this._mainViewController.ParentViewController != null) {
				this._mainViewController.RemoveFromParentViewController ();
				if (_mainViewController.View.Superview != null) {
					_mainViewController.View.RemoveFromSuperview ();
				}
			}

			this._mainViewController = newManinViewController;
			this.AddChildViewController (this._mainViewController);
			this.View.AddSubview (this._mainViewController.View);

			UpdateMainViewControllerConstraints ();
			if (this._panelViewController != null && this._panelViewController.View != null) {
				this.View.BringSubviewToFront (this._panelViewController.View);
			}
		}

		private void UpdatePanelViewController (UIViewController newPanelViewController)
		{
			if (newPanelViewController == null) {
				return;
			}

			if (this._panelViewController != null && this._mainViewController.ParentViewController != null) {
				this._panelViewController.RemoveFromParentViewController ();
				if (_panelViewController.View.Superview != null) {
					_mainViewController.View.RemoveFromSuperview ();
				}
			}

			this._panelViewController = newPanelViewController;
			this.View.AddSubview (this._panelViewController.View);

			UpdatePanelViewControllerConstraints ();
			UpdateFrameBottomOffset (VisibleZoneHeight);
			VisibilityViewState = VisibilityViewState.VisibilityStateMinimized;
			UpdateShadow ();
			DraggingEnabled ();
		}

		private void UpdateMainViewControllerConstraints ()
		{
			this._mainViewController.View.TranslatesAutoresizingMaskIntoConstraints = false;

			this._mainViewTopConstraint = NSLayoutConstraint.Create (this._mainViewController.View, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Top, 1, 0);
			this._mainViewHeightConstraint = NSLayoutConstraint.Create (this._mainViewController.View, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Height, 1, 0);
			var left = NSLayoutConstraint.Create (this._mainViewController.View, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Left, 1, 0);
			var width = NSLayoutConstraint.Create (this._mainViewController.View, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Width, 1, 0);

			this.View.AddConstraints (new [] { _mainViewTopConstraint, _mainViewHeightConstraint, left, width });
			this.View.LayoutIfNeeded ();
		}

		private void UpdatePanelViewControllerConstraints ()
		{
			this._panelViewController.View.TranslatesAutoresizingMaskIntoConstraints = false;

			this._panelViewTopConstraint = NSLayoutConstraint.Create (this._panelViewController.View, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Top, 1, 0);
			var height = NSLayoutConstraint.Create (this._panelViewController.View, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Height, 1, 0);
			var left = NSLayoutConstraint.Create (this._panelViewController.View, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Left, 1, 0);
			var right = NSLayoutConstraint.Create (this._panelViewController.View, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Right, 1, 0);
			var width = NSLayoutConstraint.Create (this._panelViewController.View, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Width, 1, 0);

			this.View.AddConstraints (new [] { _panelViewTopConstraint, height, left, right, width });
			this.View.LayoutIfNeeded ();
		}

		private void UpdateFrameBottomOffset (nfloat bottomOffset)
		{
			if (this._panelViewController != null && _panelViewController.View.Superview != null) {
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
				if (this._panelViewController != null && this._panelViewController.View.Superview != null) {
					_panelViewController.View.Layer.MasksToBounds = false;
					_panelViewController.View.Layer.ShadowOffset = new CoreGraphics.CGSize (0, 0);
					_panelViewController.View.Layer.ShadowRadius = ShadowRadius > 0 ? ShadowRadius : 20.0f;
					_panelViewController.View.Layer.ShadowOpacity = ShadowOpacity > 0 ? ShadowOpacity : 0.5f;
				}
			} else {
				_panelViewController.View.Layer.MasksToBounds = false;
				_panelViewController.View.Layer.ShadowOffset = new CoreGraphics.CGSize (0, 0);
				_panelViewController.View.Layer.ShadowRadius = 0;
				_panelViewController.View.Layer.ShadowOpacity = 0;
			}
		}

		private void DraggingEnabled ()
		{
			if (!IsDraggingEnabled) {
				if (PanGestureRecognizer != null && _panelViewController != null) {
					_panelViewController.View.RemoveGestureRecognizer (PanGestureRecognizer);
				}
			} else {
				if (PanGestureRecognizer == null) {
					PanGestureRecognizer = new UIPanGestureRecognizer ();
					_panGestureToken = PanGestureRecognizer.AddTarget (() => HandleGesture (PanGestureRecognizer));
					PanGestureRecognizer.Delegate = this;
				}
				if (_panelViewController != null) {
					_panelViewController.View.AddGestureRecognizer (PanGestureRecognizer);
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
			UpdateFrameBottomOffset (_panelViewController.View.Frame.Location.Y);
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
			this._panelViewTopConstraint = NSLayoutConstraint.Create (this._panelViewController.View, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Top, 1, 0);
			this.View.AddConstraint (_panelViewTopConstraint);
		}

		private void InstallPanelViewControllerConstraintToBottom ()
		{
			this.View.RemoveConstraint (_panelViewTopConstraint);
			this._panelViewTopConstraint = NSLayoutConstraint.Create (this._panelViewController.View, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.View, NSLayoutAttribute.Bottom, 1, -VisibleZoneHeight);
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

			MovePanelControllerWithBottomOffset (this._panelViewController.View.Frame.Size.Height, animated, animationDuration, animation, (finished) => {

				VisibilityViewStateChanged (VisibilityViewState.VisibilityStateMaximized);
				InstallPanelViewControllerConstraintToTop ();

				if (copmletion != null) {
					copmletion ();
				}
			});
		}

		public void MinimizePanelControllerAnimated (bool animated, Action animation, Action copmletion)
		{
			nfloat bottomOffset = this._panelViewTopConstraint.Constant == 0 ? VisibleZoneHeight - this._panelViewController.View.Frame.Size.Height : VisibleZoneHeight;
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

			MovePanelControllerWithBottomOffset (-this._panelViewController.View.Frame.Size.Height, animated, animationDuration, animation, (finished) => {

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
			if (IsPanelViewControllerScrollView && (this._panelViewController as MvxTableViewController).TableView.ContentOffset.Y > 0 && VisibilityViewState == VisibilityViewState.VisibilityStateMaximized) {
				this._touchPointOffset = gesture.LocationInView (this._panelViewController.View);
				this._shouldDrag = true;
				this._isInitedTouch = true;
				return;
			}

			if (gesture.State == UIGestureRecognizerState.Began) {
				this._touchPointOffset = gesture.LocationInView (this._panelViewController.View);
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
					if (IsPanelViewControllerScrollView && this._isInitedTouch) {
						InstallPanelViewControllerConstraintToBottom ();
						this._isInitedTouch = false;
					}
					VisibilityViewStateChanged (VisibilityViewState.VisibilityStateIsDragging); ;
					nfloat offset = NMath.Max (ShouldOverlapMainView ? 0f : VisibleZoneHeight, this._panelViewController.View.Frame.Size.Height - gesture.LocationInView (this.View).Y + this._touchPointOffset.Y);
					UpdateFrameBottomOffset (NMath.Min (offset, this._panelViewController.View.Frame.Size.Height));
				}
			}

		}

		[Export ("gestureRecognizer:shouldRecognizeSimultaneouslyWithGestureRecognizer:")]
		public bool ShouldRecognizeSimultaneously (UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
		{
			return IsPanelViewControllerScrollView &&
				(VisibilityViewState == VisibilityViewState.VisibilityStateMaximized || VisibilityViewState == VisibilityViewState.VisibilityStateIsMinimizing || VisibilityViewState == VisibilityViewState.VisibilityStateIsDragging) &&
				(gestureRecognizer == PanGestureRecognizer || gestureRecognizer == (this._panelViewController as MvxTableViewController).TableView.PanGestureRecognizer) &&
				(otherGestureRecognizer == PanGestureRecognizer || otherGestureRecognizer == (this._panelViewController as MvxTableViewController).TableView.PanGestureRecognizer);
		}

		public override void DidMoveToParentViewController (UIViewController parent)
		{
			if (parent == null && !this._isCleanedUp) {
				CleanUp ();
				this._isCleanedUp = true;
			}
			base.DidMoveToParentViewController (parent);
		}

		protected virtual void CleanUp ()
		{
			PanGestureRecognizer.RemoveTarget (_panGestureToken);
			PanGestureRecognizer.Delegate = null;
			if (MainViewController != null) {
				MainViewController.RemoveFromParentViewController ();
				if (MainViewController.View != null && MainViewController.View.Superview != null) {
					MainViewController.View.RemoveFromSuperview ();
				}
			}
			if (PanelViewController != null) {
				PanelViewController.RemoveFromParentViewController ();
				if (PanelViewController.View != null && PanelViewController.View.Superview != null) {
					PanelViewController.View.RemoveFromSuperview ();
					PanelViewController.View.RemoveGestureRecognizer (PanGestureRecognizer);
				}
			}
		}
	}

	public class MvxSlidingUpPanelControllerWithControllers<TViewModel>
		: MvxSlidingUpPanelControllerWithControllers
		  , IMvxIosView<TViewModel> where TViewModel : class, IMvxViewModel
	{
		public MvxSlidingUpPanelControllerWithControllers ()
		{

		}

		public MvxSlidingUpPanelControllerWithControllers (IntPtr handle)
			: base (handle)
		{
		}

		protected MvxSlidingUpPanelControllerWithControllers (string nibName, NSBundle bundle)
			: base (nibName, bundle)
		{
		}

		public new TViewModel ViewModel {
			get { return (TViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}
	}
}