
// This file has been generated by the GUI designer. Do not modify.
namespace NotMuchGUI
{
	public partial class MessageWidget
	{
		private global::Gtk.VBox vbox3;
		private global::Gtk.Table table1;
		private global::Gtk.Label label4;
		private global::Gtk.Label label5;
		private global::Gtk.Label label6;
		private global::Gtk.Label label9;
		private global::Gtk.Label labelCc;
		private global::Gtk.Label labelContent;
		private global::Gtk.Label labelDate;
		private global::Gtk.Label labelFrom;
		private global::Gtk.Label labelMsgID;
		private global::Gtk.Label labelSubject;
		private global::Gtk.Label labelThreadID;
		private global::Gtk.Label labelTo;
		private global::Gtk.HBox hbox1;
		private global::Gtk.ScrolledWindow scrolledwindowWeb;
		private global::Gtk.ScrolledWindow GtkScrolledWindow;
		private global::Gtk.NodeView attachmentNodeview;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget NotMuchGUI.MessageWidget
			global::Stetic.BinContainer.Attach (this);
			this.Name = "NotMuchGUI.MessageWidget";
			// Container child NotMuchGUI.MessageWidget.Gtk.Container+ContainerChild
			this.vbox3 = new global::Gtk.VBox ();
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = 6;
			// Container child vbox3.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table (((uint)(4)), ((uint)(4)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(10));
			this.table1.BorderWidth = ((uint)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label4 = new global::Gtk.Label ();
			this.label4.Name = "label4";
			this.label4.Xalign = 1F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString ("From");
			this.table1.Add (this.label4);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1 [this.label4]));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label5 = new global::Gtk.Label ();
			this.label5.Name = "label5";
			this.label5.Xalign = 1F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString ("Subject");
			this.table1.Add (this.label5);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1 [this.label5]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label6 = new global::Gtk.Label ();
			this.label6.Name = "label6";
			this.label6.Xalign = 1F;
			this.label6.LabelProp = global::Mono.Unix.Catalog.GetString ("To");
			this.table1.Add (this.label6);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1 [this.label6]));
			w3.TopAttach = ((uint)(2));
			w3.BottomAttach = ((uint)(3));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label9 = new global::Gtk.Label ();
			this.label9.Name = "label9";
			this.label9.Xalign = 1F;
			this.label9.LabelProp = global::Mono.Unix.Catalog.GetString ("Cc");
			this.table1.Add (this.label9);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1 [this.label9]));
			w4.TopAttach = ((uint)(3));
			w4.BottomAttach = ((uint)(4));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelCc = new global::Gtk.Label ();
			this.labelCc.Name = "labelCc";
			this.labelCc.Xalign = 0F;
			this.labelCc.LabelProp = global::Mono.Unix.Catalog.GetString ("labelCc");
			this.labelCc.Selectable = true;
			this.table1.Add (this.labelCc);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1 [this.labelCc]));
			w5.TopAttach = ((uint)(3));
			w5.BottomAttach = ((uint)(4));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(4));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelContent = new global::Gtk.Label ();
			this.labelContent.Name = "labelContent";
			this.labelContent.LabelProp = global::Mono.Unix.Catalog.GetString ("contentType");
			this.labelContent.Selectable = true;
			this.table1.Add (this.labelContent);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1 [this.labelContent]));
			w6.LeftAttach = ((uint)(3));
			w6.RightAttach = ((uint)(4));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelDate = new global::Gtk.Label ();
			this.labelDate.Name = "labelDate";
			this.labelDate.LabelProp = global::Mono.Unix.Catalog.GetString ("date");
			this.labelDate.Selectable = true;
			this.table1.Add (this.labelDate);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1 [this.labelDate]));
			w7.TopAttach = ((uint)(1));
			w7.BottomAttach = ((uint)(2));
			w7.LeftAttach = ((uint)(3));
			w7.RightAttach = ((uint)(4));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelFrom = new global::Gtk.Label ();
			this.labelFrom.Name = "labelFrom";
			this.labelFrom.Xalign = 0F;
			this.labelFrom.LabelProp = global::Mono.Unix.Catalog.GetString ("labelFrom");
			this.labelFrom.Selectable = true;
			this.table1.Add (this.labelFrom);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1 [this.labelFrom]));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelMsgID = new global::Gtk.Label ();
			this.labelMsgID.Name = "labelMsgID";
			this.labelMsgID.LabelProp = global::Mono.Unix.Catalog.GetString ("msgID");
			this.labelMsgID.Selectable = true;
			this.table1.Add (this.labelMsgID);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1 [this.labelMsgID]));
			w9.LeftAttach = ((uint)(2));
			w9.RightAttach = ((uint)(3));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelSubject = new global::Gtk.Label ();
			this.labelSubject.Name = "labelSubject";
			this.labelSubject.Xalign = 0F;
			this.labelSubject.LabelProp = global::Mono.Unix.Catalog.GetString ("labelSubject");
			this.labelSubject.Selectable = true;
			this.table1.Add (this.labelSubject);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1 [this.labelSubject]));
			w10.TopAttach = ((uint)(1));
			w10.BottomAttach = ((uint)(2));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelThreadID = new global::Gtk.Label ();
			this.labelThreadID.Name = "labelThreadID";
			this.labelThreadID.LabelProp = global::Mono.Unix.Catalog.GetString ("threadID");
			this.labelThreadID.Selectable = true;
			this.table1.Add (this.labelThreadID);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1 [this.labelThreadID]));
			w11.TopAttach = ((uint)(1));
			w11.BottomAttach = ((uint)(2));
			w11.LeftAttach = ((uint)(2));
			w11.RightAttach = ((uint)(3));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.labelTo = new global::Gtk.Label ();
			this.labelTo.Name = "labelTo";
			this.labelTo.Xalign = 0F;
			this.labelTo.LabelProp = global::Mono.Unix.Catalog.GetString ("labelTo");
			this.labelTo.Selectable = true;
			this.table1.Add (this.labelTo);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1 [this.labelTo]));
			w12.TopAttach = ((uint)(2));
			w12.BottomAttach = ((uint)(3));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(4));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox3.Add (this.table1);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.table1]));
			w13.Position = 0;
			w13.Expand = false;
			w13.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.scrolledwindowWeb = new global::Gtk.ScrolledWindow ();
			this.scrolledwindowWeb.CanFocus = true;
			this.scrolledwindowWeb.Name = "scrolledwindowWeb";
			this.scrolledwindowWeb.VscrollbarPolicy = ((global::Gtk.PolicyType)(0));
			this.scrolledwindowWeb.ShadowType = ((global::Gtk.ShadowType)(1));
			this.hbox1.Add (this.scrolledwindowWeb);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.scrolledwindowWeb]));
			w14.Position = 0;
			// Container child hbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.WidthRequest = 150;
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.attachmentNodeview = new global::Gtk.NodeView ();
			this.attachmentNodeview.CanFocus = true;
			this.attachmentNodeview.Name = "attachmentNodeview";
			this.GtkScrolledWindow.Add (this.attachmentNodeview);
			this.hbox1.Add (this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.GtkScrolledWindow]));
			w16.Position = 1;
			w16.Expand = false;
			this.vbox3.Add (this.hbox1);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.hbox1]));
			w17.PackType = ((global::Gtk.PackType)(1));
			w17.Position = 1;
			this.Add (this.vbox3);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
			this.attachmentNodeview.RowActivated += new global::Gtk.RowActivatedHandler (this.OnAttachmentNodeviewRowActivated);
		}
	}
}
