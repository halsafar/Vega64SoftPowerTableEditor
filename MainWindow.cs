using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Gtk;

class ClampedBindedEntry: Gtk.Entry
{
    public FieldInfo f;
    public Type t;

    public ClampedBindedEntry(FieldInfo field, System.Type type) : base()
    {
        this.f = field;
        this.t = type;

        try {
            Console.WriteLine(type.GetField("MaxValue").GetValue(null));
            //if (field != null && field.IsLiteral && !field.IsInitOnly) {
            //    Console.WriteLine(field.GetRawConstantValue());
            //}
        } catch (NullReferenceException e)
        {
            
        }
    }
}


public partial class MainWindow : Gtk.Window
{
    private Vega64SoftPowerTableEditor.SoftPowerTable _spt;

    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();
        this._spt = Vega64SoftPowerTableEditor.SoftPowerTable.OpenRegFile();
		this._spt.SaveRegFile();

        object foo = this._spt.atom_powerplay_table;
        foreach (var field in this._spt.atom_powerplay_table.GetType().GetFields(BindingFlags.Instance |
                                                                                 BindingFlags.NonPublic |
                                                                                 BindingFlags.Public)) {
            var field_value = field.GetValue(this._spt.atom_powerplay_table).ToString();

            HBox hbox = new HBox();

            Label l = new Label();
            Entry e = new ClampedBindedEntry(field, field.FieldType);

            l.Text = field.Name;
            e.Text = field_value;

            e.Changed += delegate {
                var converter = System.ComponentModel.TypeDescriptor.GetConverter(field.FieldType);
                field.SetValueDirect(__makeref(this._spt.atom_powerplay_table), converter.ConvertFrom(e.Text)); 
            }; 

            hbox.PackStart(l, false, false, 0);
            hbox.PackStart(e, false, false, 0);

            this.vbox_powerTable.PackStart(hbox, false, false, 0);
            Console.WriteLine("{0} = {1}", field.Name, field.GetValue(this._spt.atom_powerplay_table));
        }

        this.ShowAll();
    }

    void callback(object obj, EventArgs args)
    {
        
        Console.WriteLine("Hello again - cool button was pressed");
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
        if (this._spt != null) {
            this._spt.SaveRegFile();
        }
	}

	protected void OnLoad(object sender, EventArgs e)
	{
	}

	protected void OnAbout(object sender, EventArgs e)
	{
	}
}
