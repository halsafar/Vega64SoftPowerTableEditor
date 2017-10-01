using System;
using System.Reflection;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="T:MainWindow"/> class.
    /// </summary>
    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();

        // Easy testing
        //this._spt = Vega64SoftPowerTableEditor.SoftPowerTable.OpenRegFile("RX_VEGA_64_Soft_PP.reg");
        //this._spt.SaveRegFile("Test.reg");
        //this.setupWidgets();
    }

    /// <summary>
    /// Setup all widgets based on stored SoftPowerTable
    /// </summary>
    protected void setupWidgets() 
    {
        this.ClearWidgets();

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

        this.populateSection("Power Tune Table", this._spt.atom_vega10_powertune_table, this.vbox_memclk);

        this.ShowAll();
    }      

    /// <summary>
    /// Clears the widgets.
    /// </summary>
    protected void ClearWidgets()
    {
        foreach (Gtk.Widget w in this.vbox_powerTable.Children) {
            this.vbox_powerTable.Remove(w);
        }

        foreach (Gtk.Widget w in this.vbox_gfxclk.Children) {
            this.vbox_gfxclk.Remove(w);
        }

        foreach (Gtk.Widget w in this.vbox_memclk.Children) {
            this.vbox_memclk.Remove(w);
        }
    }

    /// <summary>
    /// Auto Generate Label and Entry edit widgets for each field in obj.
    /// HBox is created for the new widgets and the HBox is appended to the supplied VBox.
    /// </summary>
    /// <param name="sectionName">Section name.</param>
    /// <param name="obj">Object.</param>
    /// <param name="vbox">Vbox.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    protected void populateSection<T>(string sectionName, T obj, VBox vbox)
    {
        // create the header
        Label header = new Label();
        header.Markup = String.Format("<b>{0}</b>", sectionName);
        header.ModifyFont(Pango.FontDescription.FromString("Courier 14"));
        vbox.Spacing = 0;
        vbox.BorderWidth = 10;
        vbox.Homogeneous = false;
        vbox.PackStart(header, false, false, 0);

        foreach (var field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)) {
            var field_value = field.GetValue(obj).ToString();

            HBox hbox = new HBox(true, 0);

            Label l = new Label();
            Entry e = new Entry(); //new ClampedBindedEntry(field, field.FieldType);

            l.Text = field.Name;
            l.ModifyFont(Pango.FontDescription.FromString("Sans 10"));
            e.Text = field_value;
            e.ModifyFont(Pango.FontDescription.FromString("Sans 10"));

            e.Changed += delegate {
                // Delegate clamps value and assigns it, some python-esque c#
                string newValue = e.Text;

                var maxValueField = field.FieldType.GetField("MaxValue");
                var minValueField = field.FieldType.GetField("MinValue");
                var converter = System.ComponentModel.TypeDescriptor.GetConverter(field.FieldType);

                try {
                    var v = converter.ConvertFrom(newValue);
                    field.SetValueDirect(__makeref(obj), v);
                } catch (System.Exception) {
                    if (newValue.Length == 0 && minValueField != null) {
                        var v = minValueField.GetValue(null);
                        e.Text = v.ToString();
                    }
                    else if (maxValueField != null) {
                        var v = maxValueField.GetValue(null);
                        e.Text = v.ToString();
                    }
                }
            };

            hbox.PackStart(new Alignment(0f, 0f, 0, 0f) { l }, true, true, 10);
            hbox.PackStart(new Alignment(0f, 0f, 0f, 0f) { e }, true, true, 10);

            vbox.PackStart(hbox, false, false, 0);

            //Console.WriteLine("{0} = {1}", field.Name, field.GetValue(this._spt.atom_powerplay_table));
        }      

        vbox.PackStart(new HSeparator(), false, false, 25);
    }

    /// <summary>
    /// Ons the delete event.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="a">The alpha component.</param>
    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }

    /// <summary>
    /// Ons the exit.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">E.</param>
    protected void OnExit(object sender, EventArgs e)
    {
        Application.Quit();
    }

    /// <summary>
    /// Always treat as Save As for now.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">E.</param>
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

    /// <summary>
    /// Load registry file.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">E.</param>
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

    /// <summary>
    /// Ons the about.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">E.</param>
    protected void OnAbout(object sender, EventArgs e)
    {
    }
}
