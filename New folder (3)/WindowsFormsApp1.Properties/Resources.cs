using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace WindowsFormsApp1.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				ResourceManager temp = new ResourceManager("WindowsFormsApp1.Properties.Resources", typeof(Resources).Assembly);
				resourceMan = temp;
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static Bitmap _152_1520604_delete_trash_dustbin_garbage_remove_recyclebin_ui_icon
	{
		get
		{
			object obj = ResourceManager.GetObject("152-1520604_delete-trash-dustbin-garbage-remove-recyclebin-ui-icon", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap _152_1520604_delete_trash_dustbin_garbage_remove_recyclebin_ui_icon1
	{
		get
		{
			object obj = ResourceManager.GetObject("152-1520604_delete-trash-dustbin-garbage-remove-recyclebin-ui-icon1", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap _152_1520604_delete_trash_dustbin_garbage_remove_recyclebin_ui_icon2
	{
		get
		{
			object obj = ResourceManager.GetObject("152-1520604_delete-trash-dustbin-garbage-remove-recyclebin-ui-icon2", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap trash
	{
		get
		{
			object obj = ResourceManager.GetObject("trash", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap trash1
	{
		get
		{
			object obj = ResourceManager.GetObject("trash1", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static string Tutorial_on_Circuit_Simulator => ResourceManager.GetString("Tutorial_on_Circuit_Simulator", resourceCulture);

	internal Resources()
	{
	}
}
