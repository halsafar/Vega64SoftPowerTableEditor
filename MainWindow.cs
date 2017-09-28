using System;
using Gtk;

public partial class MainWindow : Gtk.Window
{
    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();
		Vega64SoftPowerTableEditor.SoftPowerTable spt = Vega64SoftPowerTableEditor.SoftPowerTable.openRegFile();
		spt.saveRegFile();
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }

	protected void OnExit(object sender, EventArgs e)
	{
		Application.Quit();
	}

	protected void OnSave(object sender, EventArgs e)
	{
	}

	protected void OnLoad(object sender, EventArgs e)
	{
	}

	protected void OnAbout(object sender, EventArgs e)
	{
	}
}
