
// This file has been generated by the GUI designer. Do not modify.
namespace NotMuchGUI
{
	public partial class FetchDialog
	{
		private global::Gtk.ScrolledWindow termScrolledwindow;
		private global::Gtk.Button buttonCancel;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget NotMuchGUI.FetchDialog
			this.Name = "NotMuchGUI.FetchDialog";
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Internal child NotMuchGUI.FetchDialog.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.termScrolledwindow = new global::Gtk.ScrolledWindow ();
			this.termScrolledwindow.CanFocus = true;
			this.termScrolledwindow.Name = "termScrolledwindow";
			this.termScrolledwindow.ShadowType = ((global::Gtk.ShadowType)(1));
			w1.Add (this.termScrolledwindow);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(w1 [this.termScrolledwindow]));
			w2.Position = 0;
			// Internal child NotMuchGUI.FetchDialog.ActionArea
			global::Gtk.HButtonBox w3 = this.ActionArea;
			w3.Name = "dialog1_ActionArea";
			w3.Spacing = 10;
			w3.BorderWidth = ((uint)(5));
			w3.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button ();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseStock = true;
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = "gtk-cancel";
			w3.Add (this.buttonCancel);
			global::Gtk.ButtonBox.ButtonBoxChild w4 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w3 [this.buttonCancel]));
			w4.Expand = false;
			w4.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 400;
			this.DefaultHeight = 300;
			this.Show ();
			this.buttonCancel.Clicked += new global::System.EventHandler (this.OnButtonCancelClicked);
		}
	}
}
