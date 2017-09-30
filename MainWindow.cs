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
        this._spt.SaveRegFile("Test.reg");

        this.setupWidgets();
    }

    protected void setupWidgets() {
        this.populateSection("PowerPlay Table", this._spt.atom_powerplay_table, this.vbox_powerTable);
        this.populateSection("Fan Table", this._spt.atom_vega10_fan_table, this.vbox_powerTable);

        foreach (Vega64SoftPowerTableEditor.SoftPowerTable.ATOM_Vega10_GFXCLK_Dependency_Record record in this._spt.atom_vega10_gfxclk_entries)
        {
            this.populateSection("Gfx Clock " + record.ucVddInd, record, this.vbox_gfxclk);
        }

        foreach (Vega64SoftPowerTableEditor.SoftPowerTable.ATOM_Vega10_MCLK_Dependency_Record record in this._spt.atom_vega10_memclk_entries) {
            this.populateSection("Mem Clock " + record.ucVddInd, record, this.vbox_gfxclk);
        }

        for (int i = 0; i < this._spt.atom_vega10_gfxvdd_record.Count; i++) {
            Vega64SoftPowerTableEditor.SoftPowerTable.ATOM_Vega10_Voltage_Lookup_Record record = this._spt.atom_vega10_gfxvdd_record[i];
            this.populateSection("Gfx Vdd " + i, record, this.vbox_memclk);
        }

        for (int i = 0; i< this._spt.atom_vega10_memvdd_record.Count; i++) {
            Vega64SoftPowerTableEditor.SoftPowerTable.ATOM_Vega10_Voltage_Lookup_Record record = this._spt.atom_vega10_memvdd_record[i];
			this.populateSection("Mem Vdd " + i, record, this.vbox_memclk);
        }

        this.ShowAll();
    }      

    protected void populateSection(string sectionName, object obj, VBox vbox) {
        // create the header
        Label header = new Label();
        header.Markup = String.Format("<b>{0}</b>", sectionName);
        header.ModifyFont(Pango.FontDescription.FromString("Courier 14"));
        vbox.Spacing = 0;
        vbox.BorderWidth = 10;
        vbox.Homogeneous = false;
        vbox.PackStart(header, false, false, 0);

        foreach (var field in obj.GetType().GetFields(BindingFlags.Instance |
                                                      BindingFlags.NonPublic |
                                                      BindingFlags.Public)) {
            var field_value = field.GetValue(obj).ToString();

            HBox hbox = new HBox(true, 0);

            Label l = new Label();
            Entry e = new ClampedBindedEntry(field, field.FieldType);

            l.Text = field.Name;
            l.ModifyFont(Pango.FontDescription.FromString("Courier 10"));
            e.Text = field_value;
            e.ModifyFont(Pango.FontDescription.FromString("Courier 10"));

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

            hbox.PackStart(new Alignment(0f, 0f, 0.5f, 0.5f) { l }, true, true, 10);
            hbox.PackStart(new Alignment(0f, 0f, 0f, 0f) { e }, true, true, 10);

            vbox.PackStart(hbox, false, false, 0);

            //Console.WriteLine("{0} = {1}", field.Name, field.GetValue(this._spt.atom_powerplay_table));
        }      

        vbox.PackStart(new HSeparator(), false, false, 25);
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
        if (this._spt != null)
        {
            Gtk.FileChooserDialog filechooser = 
                new Gtk.FileChooserDialog("Choose the file to open",
                                        this,
                                        FileChooserAction.Save,
                                        "Cancel", ResponseType.Cancel,
                                        "Open", ResponseType.Accept);

            if (filechooser.Run() == (int)ResponseType.Accept) {
                this._spt.SaveRegFile(filechooser.Filename);
            }

            filechooser.Destroy();
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
