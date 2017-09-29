using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Gtk;

class ClampedBindedEntry: Gtk.Entry
{
    private FieldInfo _fieldInfo;
    private Type _type;

    public ClampedBindedEntry(FieldInfo field, System.Type type) : base()
    {
        this._fieldInfo = field;
        this._type = type;
    }
}


public partial class MainWindow : Gtk.Window
{
    private Vega64SoftPowerTableEditor.SoftPowerTable _spt;

    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();

        this._spt = Vega64SoftPowerTableEditor.SoftPowerTable.OpenRegFile("RX_VEGA_64_Soft_PP.reg");
        this._spt.SaveRegFile();

        this.setupWidgets();
    }

    protected void setupWidgets() {
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
                // Delegate clamps value and assigns it, some python-esque c#
                string newValue = e.Text;

                var maxValueField = field.FieldType.GetField("MaxValue");
                var converter = System.ComponentModel.TypeDescriptor.GetConverter(field.FieldType);

                try {
                    var v = converter.ConvertFrom(newValue);
                    field.SetValueDirect(__makeref(this._spt.atom_powerplay_table), v); 
                } catch (System.Exception) {
                    if (maxValueField != null) {
                         var maxValue = maxValueField.GetValue(null);
                         e.Text = maxValue.ToString();
                    }
                }
            }; 

            hbox.PackStart(l, false, false, 0);
            hbox.PackStart(e, false, false, 0);

            this.vbox_powerTable.PackStart(hbox, false, false, 0);

            //Console.WriteLine("{0} = {1}", field.Name, field.GetValue(this._spt.atom_powerplay_table));
        }

        this.ShowAll();
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
        Gtk.FileChooserDialog filechooser =
        		new Gtk.FileChooserDialog("Choose the file to open",
        			this,
        			FileChooserAction.Open,
        			"Cancel", ResponseType.Cancel,
        			"Open", ResponseType.Accept);
        if (filechooser.Run() == (int)ResponseType.Accept) 
        {
            this._spt = Vega64SoftPowerTableEditor.SoftPowerTable.OpenRegFile(filechooser.Filename);
            this.setupWidgets();
        }

        filechooser.Destroy();
	}

	protected void OnAbout(object sender, EventArgs e)
	{
	}
}
