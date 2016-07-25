using System;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Platform.IoC;

namespace MvxSlidingUpPanel.Core
{
	public class App : MvxApplication
	{

		public override void Initialize ()
		{
			CreatableTypes ()
				.EndingWith ("Service")
				.AsInterfaces ()
				.RegisterAsLazySingleton ();

			RegisterAppStart<FirstViewModel> ();
		}
	}
}

