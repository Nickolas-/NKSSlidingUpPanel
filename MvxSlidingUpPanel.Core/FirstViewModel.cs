using System;
using System.Collections.Generic;
using MvvmCross.Core.ViewModels;

namespace MvxSlidingUpPanel.Core
{
	public class FirstViewModel : MvxViewModel
	{
		public FirstViewModel ()
		{
			var data = new List<DataModel> ();
			for (int i = 0; i < 250; i++) {
				data.Add (new DataModel () { Nubmer = i });
			}
			SecondVM = new SencodnViewModel ();
			SecondVM.FirstData = data;
		}

		public SencodnViewModel SecondVM { get; set; }


	}

	public class SencodnViewModel : MvxViewModel
	{
		IEnumerable<DataModel> _firstData;
		public IEnumerable<DataModel> FirstData {
			get {
				return _firstData;
			}

			set {
				_firstData = value;
				RaisePropertyChanged ();

			}
		}
	}
}

