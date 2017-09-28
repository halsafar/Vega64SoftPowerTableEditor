using System;
using Gtk;

public partial class MainWindow : Gtk.Window
{
    private Vega64SoftPowerTableEditor.SoftPowerTable _spt;

    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();
        this._spt = Vega64SoftPowerTableEditor.SoftPowerTable.OpenRegFile();
		this._spt.SaveRegFile();
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
