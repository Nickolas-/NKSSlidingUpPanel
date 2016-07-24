using System;
using Foundation;
using UIKit;

namespace SlidingDemo
{
	public class CustomTableView : UITableView , IUITableViewDataSource
	{
		public CustomTableView ()
		{
			
		}

		public UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = null;
			cell = tableView.DequeueReusableCell ("Cell");
			if (cell == null) 
				cell = new UITableViewCell (UITableViewCellStyle.Subtitle, "Cell");
			cell.TextLabel.Text = indexPath.Row.ToString();
			cell.TextLabel.TextColor = UIColor.Black;
			return cell;
		}

		public nint RowsInSection (UITableView tableView, nint section)
		{
			return 250;
		}
	}
}

