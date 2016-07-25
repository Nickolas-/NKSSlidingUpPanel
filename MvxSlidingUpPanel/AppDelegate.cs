using System;
using System.Collections.Generic;
using System.Diagnostics;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS.Views;
using MvvmCross.Core.ViewModels;
using MvvmCross.Core.Views;
using MvvmCross.iOS.Platform;
using MvvmCross.iOS.Views;
using MvvmCross.iOS.Views.Presenters;
using MvvmCross.Platform;
using MvvmCross.Platform.Platform;
using MvxSlidingUpPanel.Core;
using UIKit;

namespace MvxSlidingUpPanel
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register ("AppDelegate")]
	public class AppDelegate : MvxApplicationDelegate
	{
		// class-level declarations

		UIWindow _window;

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			// Override point for customization after application launch.
			// If not required for your application you can safely delete this method
			_window = new UIWindow (UIScreen.MainScreen.Bounds);

			var presenter = new MvxIosViewPresenter (this, _window);
			var setup = new Setup (this, presenter);
			setup.Initialize ();

			var startup = Mvx.Resolve<IMvxAppStart> ();
			startup.Start ();

			_window.MakeKeyAndVisible ();
			return true;
		}


	}

	public class Setup : MvxIosSetup
	{
		public Setup (MvxApplicationDelegate applicationDelegate, UIWindow window)
			: base (applicationDelegate, window)
		{
		}

		public Setup (MvxApplicationDelegate applicationDelegate, IMvxIosViewPresenter presenter)
			: base (applicationDelegate, presenter)
		{
		}

		protected override IMvxApplication CreateApp ()
		{
			return new Core.App ();
		}

		protected override IMvxTrace CreateDebugTrace ()
		{
			return new MvxDebugTrace ();
		}
	}

	public class DebugTrace : IMvxTrace
	{
		public void Trace (MvxTraceLevel level, string tag, Func<string> message)
		{
			Debug.WriteLine (tag + ":" + level + ":" + message ());
		}

		public void Trace (MvxTraceLevel level, string tag, string message)
		{
			Debug.WriteLine (tag + ":" + level + ":" + message);
		}

		public void Trace (MvxTraceLevel level, string tag, string message, params object [] args)
		{
			try {
				Debug.WriteLine (string.Format (tag + ":" + level + ":" + message, args));
			} catch (FormatException) {
				Trace (MvxTraceLevel.Error, tag, "Exception during trace of {0} {1}", level, message);
			}
		}
	}

	public class FirstView : MvxNKSSlidingUpPanel.MvxSlidingUpPanelControllerWithControllers<FirstViewModel>
	{
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			var controllerMain = new UIViewController ();
			controllerMain.View.BackgroundColor = UIColor.LightGray;
			var panelController = new MvxCustomTalbeViewController ();
			panelController.ViewModel = ViewModel.SecondVM;
			//var panelController = new UIViewController ();
			//panelController.View.BackgroundColor = UIColor.Red;
			MainViewController = controllerMain;
			PanelViewController = panelController;

			VisibleZoneHeight = 100;
			ShouldOverlapMainView = true;
			IsDraggingEnabled = true;

		}		
	}

	public class MvxCustomTalbeViewController : MvxTableViewController 
	{
		public new SencodnViewModel ViewModel {
			get { return (SencodnViewModel)base.DataContext; }
			set { base.DataContext = value; }
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();


			TableView.Bounces = false;
			var source = new MvxSimpleTableViewSource (TableView, typeof (MvxCustomTableViewCell), MvxCustomTableViewCell.Key);

			var set = this.CreateBindingSet<MvxCustomTalbeViewController, SencodnViewModel> ();
			set.Bind (source).To (v=>v.FirstData);
			set.Apply ();


			TableView.Source = source;
			TableView.ReloadData ();
		}
	}

	public class MvxCustomTableViewCell : MvxTableViewCell
	{
		public static readonly NSString Key = new NSString ("MvxCustomTableViewCell");

		public MvxCustomTableViewCell (IntPtr handle) : base(handle)
        {
			InitViewObjects ();
			InitBindingObjects ();
		}

		void InitViewObjects ()
		{
			_titleLabel = new UILabel (new CoreGraphics.CGRect (ContentView.Frame.Location, ContentView.Frame.Size));
			_titleLabel.TextColor = UIColor.Red;
			ContentView.AddSubview (_titleLabel);
		}

		UILabel _titleLabel;

		void InitBindingObjects ()
		{
			this.DelayBind (() => {
				var set = this.CreateBindingSet<MvxCustomTableViewCell, DataModel> ();
				set.Bind (_titleLabel).To (vm => vm.Nubmer);
				set.Apply ();
			});
		}
	}
}


