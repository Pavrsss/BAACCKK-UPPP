#define DEBUG
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using WindowsFormsApp1.Properties;

namespace WindowsFormsApp1;

public class Form1 : Form
{
	public abstract class BaseElecPiece
	{
		public string Name { get; set; }

		public double Voltage { get; set; }

		public double Current { get; set; }

		public double Resistance { get; set; }

		public Point StartPoint { get; set; }

		public Point EndPoint { get; set; }

		public int X { get; set; }

		public int Y { get; set; }

		public Point LeftAnchor => new Point(X, Y + 50);

		public Point RightAnchor => new Point(X + 75, Y + 50);

		public abstract void Draw(Graphics g);

		public bool ContainsPoint(int mouseX, int mouseY)
		{
			return mouseX >= X && mouseX <= X + 80 && mouseY >= Y && mouseY <= Y + 80;
		}

		public virtual bool IsHit(Point clickLocation, float tolerance)
		{
			return ContainsPoint(clickLocation.X, clickLocation.Y);
		}
	}

	public class Cell : BaseElecPiece
	{
		private static readonly Pen BlackPen = new Pen(Color.Black, 3f);

		public new Point LeftAnchor => new Point(X, Y + 50);

		public new Point RightAnchor => new Point(X + 75, Y + 50);

		public Cell()
		{
			Resistance = 0.0;
			Voltage = 5.0;
		}

		public override void Draw(Graphics g)
		{
			g.DrawLine(BlackPen, X, Y + 50, X + 25, Y + 50);
			g.DrawLine(BlackPen, X + 25, Y + 25, X + 25, Y + 75);
			g.DrawLine(BlackPen, X + 50, Y + 40, X + 50, Y + 60);
			g.DrawLine(BlackPen, X + 50, Y + 50, X + 75, Y + 50);
		}
	}

	public class Switch : BaseElecPiece
	{
		public bool IsClosed { get; private set; } = false;

		public Switch()
		{
			Resistance = 999999999999.0;
			Voltage = 0.0;
		}

		public void Toggle()
		{
			IsClosed = !IsClosed;
			if (IsClosed)
			{
				Resistance = 0.0;
			}
			else
			{
				Resistance = 999999999999.0;
			}
		}

		public override void Draw(Graphics g)
		{
			using Pen blackPen = new Pen(Color.Black, 3f);
			g.DrawLine(blackPen, X, Y + 50, X + 25, Y + 50);
			if (IsClosed)
			{
				g.DrawLine(blackPen, X + 25, Y + 50, X + 50, Y + 50);
			}
			else
			{
				g.DrawLine(blackPen, X + 25, Y + 50, X + 50, Y + 25);
			}
			g.DrawLine(blackPen, X + 50, Y + 50, X + 75, Y + 50);
		}
	}

	public class Battery : BaseElecPiece
	{
		private static readonly Pen BlackPen = new Pen(Color.Black, 3f);

		public double MinVoltage { get; } = 0.0;

		public double MaxVoltage { get; } = 250.0;

		public Battery()
		{
			Resistance = 0.0;
			Voltage = 10.0;
		}

		public void SetVoltage(double v)
		{
			Voltage = Math.Max(MinVoltage, Math.Min(MaxVoltage, v));
		}

		public override void Draw(Graphics g)
		{
			g.DrawLine(BlackPen, X, Y + 50, X + 20, Y + 50);
			g.DrawLine(BlackPen, X + 20, Y + 25, X + 20, Y + 75);
			g.DrawLine(BlackPen, X + 35, Y + 40, X + 35, Y + 60);
			g.DrawLine(BlackPen, X + 50, Y + 25, X + 50, Y + 75);
			g.DrawLine(BlackPen, X + 65, Y + 40, X + 65, Y + 60);
			g.DrawLine(BlackPen, X + 65, Y + 50, X + 85, Y + 50);
		}
	}

	public class Diode : BaseElecPiece
	{
		public double ForwardResistance { get; set; } = 7.0;

		public double ReverseResistance { get; set; } = 50.0;

		public override void Draw(Graphics g)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			using Pen blackPen = new Pen(Color.Black, 3f);
			using Brush brush = new SolidBrush(Color.Black);
			g.DrawEllipse(blackPen, X + 13, Y + 25, 50, 50);
			Point[] triangle = new Point[3]
			{
				new Point(X + 28, Y + 35),
				new Point(X + 28, Y + 65),
				new Point(X + 50, Y + 50)
			};
			g.FillPolygon(brush, triangle);
			g.DrawLine(blackPen, X + 50, Y + 35, X + 50, Y + 65);
			g.DrawLine(blackPen, X, Y + 50, X + 75, Y + 50);
		}

		public double EffectiveResistanceForVoltage(double appliedVoltage)
		{
			return (appliedVoltage >= 0.0) ? ForwardResistance : ReverseResistance;
		}
	}

	public class Resistor : BaseElecPiece
	{
		private static readonly Pen BlackPen = new Pen(Color.Black, 3f);

		public Resistor()
		{
			Resistance = 100.0;
		}

		public override void Draw(Graphics g)
		{
			g.DrawLine(BlackPen, X + 20, Y + 70, X + 70, Y + 70);
			g.DrawLine(BlackPen, X + 20, Y + 30, X + 70, Y + 30);
			g.DrawLine(BlackPen, X + 20, Y + 70, X + 20, Y + 30);
			g.DrawLine(BlackPen, X + 70, Y + 30, X + 70, Y + 70);
			g.DrawLine(BlackPen, X, Y + 50, X + 20, Y + 50);
			g.DrawLine(BlackPen, X + 70, Y + 50, X + 90, Y + 50);
		}
	}

	public class Lamp : BaseElecPiece
	{
		private static readonly Pen BlackPen = new Pen(Color.Black, 3f);

		public override void Draw(Graphics g)
		{
			g.DrawLine(BlackPen, X, Y + 50, X + 15, Y + 50);
			g.DrawLine(BlackPen, X + 60, Y + 50, X + 75, Y + 50);
			g.DrawEllipse(BlackPen, X + 13, Y + 25, 50, 50);
			g.DrawLine(BlackPen, X + 27, Y + 39, X + 49, Y + 61);
			g.DrawLine(BlackPen, X + 27, Y + 61, X + 49, Y + 39);
		}
	}

	public class Ammeter : BaseElecPiece
	{
		private static readonly Pen BlackPen = new Pen(Color.Black, 3f);

		private static readonly Brush TextBrush = new SolidBrush(Color.Black);

		private static readonly Brush BackBrush = new SolidBrush(Color.FromArgb(200, Color.White));

		private static readonly Font DisplayFont = new Font("Segoe UI", 9f, FontStyle.Bold);

		public double MeasuredCurrent { get; set; }

		public Ammeter()
		{
			Resistance = 0.0;
			Voltage = 0.0;
			MeasuredCurrent = 0.0;
		}

		public override void Draw(Graphics g)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.DrawLine(BlackPen, X, Y + 50, X + 15, Y + 50);
			g.DrawLine(BlackPen, X + 60, Y + 50, X + 75, Y + 50);
			g.DrawEllipse(BlackPen, X + 13, Y + 25, 50, 50);
			g.DrawLine(BlackPen, X + 38, Y + 35, X + 31, Y + 57);
			g.DrawLine(BlackPen, X + 38, Y + 35, X + 45, Y + 57);
			g.DrawLine(BlackPen, X + 34, Y + 48, X + 42, Y + 48);
			string ammeterText = $"{MeasuredCurrent:F3} A";
			SizeF textSize = g.MeasureString(ammeterText, DisplayFont);
			float textX = (float)(X + 38) - textSize.Width / 2f;
			float textY = Y + 62;
			RectangleF labelA = new RectangleF(textX - 4f, textY - 2f, textSize.Width + 8f, textSize.Height + 4f);
			g.FillRectangle(BackBrush, labelA);
			g.DrawRectangle(Pens.Gray, (int)labelA.X, (int)labelA.Y, (int)labelA.Width, (int)labelA.Height);
			g.DrawString(ammeterText, DisplayFont, TextBrush, textX, textY);
		}
	}

	public class Voltmeter : BaseElecPiece
	{
		private static readonly Pen BlackPen = new Pen(Color.Black, 3f);

		private static readonly Brush TextBrush = new SolidBrush(Color.Black);

		private static readonly Brush BackBrush = new SolidBrush(Color.FromArgb(200, Color.White));

		private static readonly Font LabelFont = new Font("Segoe UI", 9f, FontStyle.Bold);

		public double MeasuredVoltage { get; set; }

		public Voltmeter()
		{
			Resistance = 1000000000.0;
			Voltage = 0.0;
			MeasuredVoltage = 0.0;
		}

		public override void Draw(Graphics g)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.DrawLine(BlackPen, X, Y + 50, X + 15, Y + 50);
			g.DrawLine(BlackPen, X + 60, Y + 50, X + 75, Y + 50);
			g.DrawEllipse(BlackPen, X + 13, Y + 25, 50, 50);
			g.DrawLine(BlackPen, X + 30, Y + 37, X + 38, Y + 59);
			g.DrawLine(BlackPen, X + 38, Y + 59, X + 46, Y + 37);
			string voltText = $"{MeasuredVoltage:F2} V";
			SizeF labelSize = g.MeasureString(voltText, LabelFont);
			float displayX = (float)(X + 38) - labelSize.Width / 2f;
			float displayY = Y + 62;
			RectangleF textFrame = new RectangleF(displayX - 4f, displayY - 2f, labelSize.Width + 8f, labelSize.Height + 4f);
			g.FillRectangle(BackBrush, textFrame);
			g.DrawRectangle(Pens.Gray, (int)textFrame.X, (int)textFrame.Y, (int)textFrame.Width, (int)textFrame.Height);
			g.DrawString(voltText, LabelFont, TextBrush, displayX, displayY);
		}
	}

	public class Wire : BaseElecPiece
	{
		private static readonly Pen WirePen = CreateWirePen();

		private static readonly Brush HitBrush = new SolidBrush(Color.FromArgb(140, Color.LightGray));

		private static readonly Pen HitOutline = new Pen(Color.Gray, 1f);

		public int handleRad { get; set; } = 8;

		public BaseElecPiece connectedStartComp { get; set; }

		public BaseElecPiece connectedEndComp { get; set; }

		public string connectedStartAnch { get; set; } = "";

		public string connectedEndAnch { get; set; } = "";

		private Point EffectiveStart => ResolveEffectivePoint(connectedStartComp, connectedStartAnch, StartPoint);

		private Point EffectiveEnd => ResolveEffectivePoint(connectedEndComp, connectedEndAnch, EndPoint);

		private static Pen CreateWirePen()
		{
			return new Pen(Color.Black, 2f)
			{
				StartCap = LineCap.Round,
				EndCap = LineCap.Round
			};
		}

		public Wire()
		{
			Resistance = 0.0;
			Voltage = 0.0;
		}

		private Point ResolveEffectivePoint(BaseElecPiece connectedComp, string anchor, Point fallbackPoint)
		{
			if (connectedComp is Wire connectedWire)
			{
				return connectedWire.GetConnectionPoint(anchor);
			}
			if (connectedComp == null)
			{
				return fallbackPoint;
			}
			return (anchor == "Left") ? connectedComp.LeftAnchor : connectedComp.RightAnchor;
		}

		public override void Draw(Graphics g)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			Point s = EffectiveStart;
			Point e = EffectiveEnd;
			g.DrawLine(WirePen, s, e);
			DrawHandle(g, s);
			DrawHandle(g, e);
		}

		private void DrawHandle(Graphics g, Point center)
		{
			int visRad = handleRad + 4;
			g.FillEllipse(HitBrush, center.X - visRad, center.Y - visRad, visRad * 2, visRad * 2);
			g.DrawEllipse(HitOutline, center.X - visRad, center.Y - visRad, visRad * 2, visRad * 2);
			g.FillEllipse(Brushes.Red, center.X - handleRad, center.Y - handleRad, handleRad * 2, handleRad * 2);
		}

		public override bool IsHit(Point clickLocation, float tolerance)
		{
			Point s = EffectiveStart;
			Point e = EffectiveEnd;
			if (s == e)
			{
				return IsNearPoint(clickLocation, s, (int)tolerance);
			}
			float num = Math.Abs((e.Y - s.Y) * clickLocation.X - (e.X - s.X) * clickLocation.Y + e.X * s.Y - e.Y * s.X);
			float den = (float)Math.Sqrt(Math.Pow(e.Y - s.Y, 2.0) + Math.Pow(e.X - s.X, 2.0));
			if (den == 0f)
			{
				return false;
			}
			float distance = num / den;
			bool withinBounds = (float)clickLocation.X >= (float)Math.Min(s.X, e.X) - tolerance && (float)clickLocation.X <= (float)Math.Max(s.X, e.X) + tolerance && (float)clickLocation.Y >= (float)Math.Min(s.Y, e.Y) - tolerance && (float)clickLocation.Y <= (float)Math.Max(s.Y, e.Y) + tolerance;
			return (distance <= tolerance) & withinBounds;
		}

		public string GetSelectedHandle(Point clickLocation)
		{
			if (IsNearPoint(clickLocation, EffectiveStart, handleRad + 4))
			{
				return "StartPoint";
			}
			if (IsNearPoint(clickLocation, EffectiveEnd, handleRad + 4))
			{
				return "EndPoint";
			}
			if (IsHit(clickLocation, 5f))
			{
				return "body";
			}
			return "";
		}

		private bool IsNearPoint(Point p1, Point p2, int tolerance)
		{
			int dx = p1.X - p2.X;
			int dy = p1.Y - p2.Y;
			return dx * dx + dy * dy <= tolerance * tolerance;
		}

		public Point GetConnectionPoint(string anchorName)
		{
			return (anchorName == "StartPoint") ? StartPoint : EndPoint;
		}
	}

	private BaseElecPiece editingComponent = null;

	private bool muteTrack = false;

	private List<BaseElecPiece> myCircuitList = new List<BaseElecPiece>();

	private BaseElecPiece selectedComp = null;

	private bool isDragging = false;

	private int lastMouseX;

	private int lastMouseY;

	private int mouseX;

	private List<BaseElecPiece> activeCircuitList = new List<BaseElecPiece>();

	private Point startDrag;

	private string selectedWireHandle = "";

	private string shapeToDraw = "";

	private int shapeX = 50;

	private int shapeY = 50;

	private int shapeWidth = 100;

	private int shapeHeight = 100;

	private string titleText;

	private string defaultText;

	private string promptText;

	private const int resizing = 70;

	private const int calcSize = 131;

	private bool lockLayoutFrames = false;

	private CancellationTokenSource simCts;

	private double simTime = 0.0;

	private readonly double simDt = 0.02;

	private double amplitude = 5.0;

	private double frequency = 1.0;

	private int vtMaxPoints = 800;

	private BaseElecPiece selectedForIV = null;

	private IContainer CircuitComponent = null;

	private BackgroundWorker backgroundWorker1;

	private System.Windows.Forms.Timer simTimer;

	private IContainer components;

	private TabPage tabCircuit;

	private Label lblComponentValue;

	private TrackBar compTrack;

	private Button btnSimulate;

	private Chart chtIV;

	private PictureBox picBin;

	private ToolStrip tlsComponents;

	private ToolStripButton btnBattery;

	private ToolStripButton btnCell;

	private ToolStripButton btnResistor;

	private ToolStripButton btnLamp;

	private ToolStripButton btnDiode;

	private ToolStrip tlsMeasures;

	private ToolStripButton btnAmmeter;

	private ToolStripButton btnVoltmeter;

	private ToolStrip tlsMaterials;

	private ToolStripButton btnWire;

	private PictureBox pictureBox1;

	private TabControl tabSimulation;

	private TabPage tabPage2;

	private PictureBox pictureBox2;

	private ToolStripMenuItem componentsToolStripMenuItem;

	private ToolStripMenuItem materialsToolStripMenuItem;

	private ToolStripMenuItem measuresToolStripMenuItem;

	private MenuStrip menuStrip1;

	private ToolStripButton btnSwitch;

	private RichTextBox richTextBox1;

	public Form1()
	{
		InitializeComponent();
		Pen redPen = new Pen(Color.Red);
		Pen blackPen = new Pen(Color.Black);
		pictureBox1.Paint += PictureBox1_Paint;
		AutoSize = false;
		AutoSizeMode = AutoSizeMode.GrowAndShrink;
		pictureBox1.Dock = DockStyle.Fill;
		pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		SetupCharts();
		compTrack.SmallChange = 1;
		compTrack.LargeChange = 10;
		compTrack.TickFrequency = 50;
		compTrack.Visible = false;
		lblComponentValue.Visible = false;
		compTrack.Scroll -= ComponentTrackBar_Scroll;
		compTrack.Scroll += ComponentTrackBar_Scroll;
		compTrack.ValueChanged -= ComponentTrackBar_Scroll;
		compTrack.ValueChanged += ComponentTrackBar_Scroll;
		compTrack.BringToFront();
		lblComponentValue.BringToFront();
	}

	private void LoadTutorial(object sender, EventArgs e)
	{
		richTextBox1.Rtf = Resources.Tutorial_on_Circuit_Simulator;
	}

	private void SetupCharts()
	{
		if (chtIV.ChartAreas.Count == 0)
		{
			chtIV.ChartAreas.Add("CA");
		}
		ChartArea caIV = chtIV.ChartAreas[0];
		caIV.AxisX.Title = "Voltage (V)";
		caIV.AxisY.Title = "Current (A)";
		caIV.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
		caIV.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
		chtIV.Series.Clear();
		Series sIV = chtIV.Series.Add("I-V");
		sIV.ChartType = SeriesChartType.Line;
		sIV.BorderWidth = 2;
	}

	private void btnWireTool_Click(object sender, EventArgs e)
	{
	}

	private void canvas_MouseDown(object sender, MouseEventArgs e)
	{
	}

	private void canvas_MouseUp(object sender, MouseEventArgs e)
	{
	}

	private void canvas_Paint(object sender, PaintEventArgs e)
	{
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		foreach (BaseElecPiece component in activeCircuitList)
		{
			component.Draw(e.Graphics);
		}
	}

	private void toolStripButton_Click(object sender, EventArgs e)
	{
		foreach (object item in tlsMaterials.Items)
		{
			if (item is ToolStripButton btn && btn != sender)
			{
				btn.Checked = false;
			}
		}
	}

	private void Form1_Paint(object sender, PaintEventArgs e)
	{
	}

	private void Form1_MouseClick(object sender, MouseEventArgs e)
	{
		shapeX = e.X - shapeWidth / 2;
		shapeY = e.Y - shapeHeight / 2;
		Invalidate();
	}

	private void measuresToolStripMenuItem_Click(object sender, EventArgs e)
	{
		tlsMeasures.Visible = !tlsMeasures.Visible;
		tlsComponents.Visible = false;
		tlsMaterials.Visible = false;
	}

	private void componentsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		tlsComponents.Visible = !tlsComponents.Visible;
		tlsMaterials.Visible = false;
		tlsMeasures.Visible = false;
	}

	private void materialsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		tlsMaterials.Visible = !tlsMaterials.Visible;
		tlsComponents.Visible = false;
		tlsMeasures.Visible = false;
	}

	private void tlsMaterials_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
	{
		pictureBox1.BackColor = Color.White;
		RefreshCanvas();
	}

	private void btnBattery_Click(object sender, EventArgs e)
	{
		myCircuitList.Add(new Battery
		{
			X = 150,
			Y = 50
		});
		RefreshCanvas();
	}

	private void btnCell_Click(object sender, EventArgs e)
	{
		myCircuitList.Add(new Cell
		{
			X = 50,
			Y = 50
		});
		RefreshCanvas();
	}

	private void btnAmmeter_Click(object sender, EventArgs e)
	{
		myCircuitList.Add(new Ammeter
		{
			X = 50,
			Y = 50
		});
		RefreshCanvas();
	}

	private void btnVoltmeter_Click(object sender, EventArgs e)
	{
		myCircuitList.Add(new Voltmeter
		{
			X = 50,
			Y = 50
		});
		RefreshCanvas();
	}

	private void pictureBox1_Click(object sender, EventArgs e)
	{
		compTrack.Parent = pictureBox1;
		lblComponentValue.Parent = pictureBox1;
		MouseEventArgs mouse = e as MouseEventArgs;
		if (mouse != null)
		{
			Switch clickedSwitch = myCircuitList.OfType<Switch>().FirstOrDefault((Switch sw) => IsMouseOverSwitch(sw, mouse));
			if (clickedSwitch != null)
			{
				clickedSwitch.Toggle();
				pictureBox1.Invalidate();
			}
		}
	}

	private bool IsMouseOverSwitch(Switch sw, MouseEventArgs mouse)
	{
		bool insideX = mouse.X >= sw.X && mouse.X <= sw.X + 75;
		bool insideY = mouse.Y >= sw.Y + 20 && mouse.Y <= sw.Y + 60;
		return insideX & insideY;
	}

	private void pictureBox1_DoubleClick(object sender, EventArgs e)
	{
	}

	private void btnSwitch_Click(object sender, EventArgs e)
	{
		myCircuitList.Add(new Switch
		{
			X = 150,
			Y = 50
		});
		RefreshCanvas();
	}

	private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
	{
		Debug.WriteLine(string.Format("MouseDown at {0}. selectedComp BEFORE search = {1}", e.Location, selectedComp?.GetType().Name ?? "(null)"));
		if (e.Button == MouseButtons.Left)
		{
			compTrack?.Hide();
			lblComponentValue?.Hide();
			Point clickPos = new Point(e.X, e.Y);
			BaseElecPiece hitComp = myCircuitList.LastOrDefault((BaseElecPiece c) => c.IsHit(clickPos, 8f));
			if (hitComp != null)
			{
				selectedComp = hitComp;
				Debug.WriteLine($">> Selected: {selectedComp.GetType().Name} | R: {selectedComp.Resistance}Ω, V: {selectedComp.Voltage}V");
				isDragging = true;
				lastMouseX = e.X;
				lastMouseY = e.Y;
				Wire wire = hitComp as Wire;
				selectedWireHandle = ((wire != null) ? wire.GetSelectedHandle(clickPos) : "");
				SyncTrackbar();
			}
		}
	}

	private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
	{
		if (isDragging)
		{
			UpdateBinHighlight(e.Location);
			if (selectedComp != null)
			{
				int moveX = e.X - lastMouseX;
				int moveY = e.Y - lastMouseY;
				MoveSelectedPiece(selectedComp, moveX, moveY);
				UpdateFloatingControls(selectedComp);
				lastMouseX = e.X;
				lastMouseY = e.Y;
				RefreshCanvas();
			}
		}
	}

	private void UpdateBinHighlight(Point mouseLocation)
	{
		if (picBin != null)
		{
			Point binScreen = picBin.PointToScreen(Point.Empty);
			Point binInCanvas = pictureBox1.PointToClient(binScreen);
			Rectangle binRect = new Rectangle(binInCanvas, picBin.Size);
			picBin.BackColor = (binRect.Contains(mouseLocation) ? Color.LightCoral : SystemColors.Control);
		}
	}

	private void MoveSelectedPiece(BaseElecPiece piece, int moveX, int moveY)
	{
		if (piece is Wire wire)
		{
			MoveWire(wire, moveX, moveY);
			return;
		}
		piece.X += moveX;
		piece.Y += moveY;
	}

	private void MoveWire(Wire wire, int moveX, int moveY)
	{
		if (selectedWireHandle == "StartPoint")
		{
			MoveWireStart(wire, moveX, moveY);
			return;
		}
		if (selectedWireHandle == "EndPoint")
		{
			MoveWireEnd(wire, moveX, moveY);
			return;
		}
		MoveWireStart(wire, moveX, moveY);
		MoveWireEnd(wire, moveX, moveY);
	}

	private void MoveWireStart(Wire wire, int moveX, int moveY)
	{
		Point start = wire.StartPoint;
		start.Offset(moveX, moveY);
		wire.StartPoint = start;
		wire.connectedStartComp = null;
		wire.connectedStartAnch = "";
	}

	private void MoveWireEnd(Wire wire, int moveX, int moveY)
	{
		Point end = wire.EndPoint;
		end.Offset(moveX, moveY);
		wire.EndPoint = end;
		wire.connectedEndComp = null;
		wire.connectedEndAnch = "";
	}

	private void UpdateFloatingControls(BaseElecPiece piece)
	{
		if (compTrack != null && compTrack.Visible)
		{
			compTrack.Location = new Point(piece.X, Math.Max(0, piece.Y - 36));
			if (lblComponentValue != null)
			{
				lblComponentValue.Location = new Point(compTrack.Location.X, Math.Max(0, compTrack.Location.Y - 20));
			}
		}
	}

	private Rectangle GetBinBoundsInCanvas()
	{
		Point binTopLeft = new Point(0, 0);
		Point binScreenPos = picBin.PointToScreen(binTopLeft);
		Point binInCanvas = pictureBox1.PointToClient(binScreenPos);
		return new Rectangle(binInCanvas.X, binInCanvas.Y, picBin.Width, picBin.Height);
	}

	private void FinishDrag()
	{
		selectedComp = null;
		selectedWireHandle = "";
		isDragging = false;
		RefreshCanvas();
	}

	private bool TryDeleteDroppedComponent(BaseElecPiece comp)
	{
		if (picBin == null)
		{
			return false;
		}
		Point binScreenPos = picBin.PointToScreen(Point.Empty);
		Point binInCanvas = pictureBox1.PointToClient(binScreenPos);
		Rectangle binRect = new Rectangle(binInCanvas, picBin.Size);
		if (!getCompBound(comp).IntersectsWith(binRect))
		{
			return false;
		}
		List<Wire> connectedWires = (from wire in myCircuitList.OfType<Wire>()
			where wire.connectedStartComp == comp || wire.connectedEndComp == comp
			select wire).ToList();
		foreach (Wire w in connectedWires)
		{
			myCircuitList.Remove(w);
		}
		myCircuitList.Remove(comp);
		return true;
	}

	private void SnapComponentToNeighbors(BaseElecPiece comp, int snapDistance)
	{
		foreach (BaseElecPiece staticComp in myCircuitList)
		{
			if (staticComp != comp)
			{
				if (GetDistance(comp.LeftAnchor, staticComp.RightAnchor) < (double)snapDistance)
				{
					comp.X = staticComp.RightAnchor.X;
					comp.Y = staticComp.RightAnchor.Y - 50;
					break;
				}
				if (GetDistance(comp.RightAnchor, staticComp.LeftAnchor) < (double)snapDistance)
				{
					comp.X = staticComp.LeftAnchor.X - 75;
					comp.Y = staticComp.LeftAnchor.Y - 50;
					break;
				}
			}
		}
	}

	private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
	{
		Debug.WriteLine(string.Format("MouseUp at {0}. selectedComp at MouseUp = {1}", e.Location, selectedComp?.GetType().Name ?? "(null)"));
		if (picBin != null)
		{
			picBin.BackColor = SystemColors.Control;
		}
		if (selectedComp != null)
		{
			if (TryDeleteDroppedComponent(selectedComp))
			{
				FinishDrag();
			}
			else if (selectedComp is Wire wire)
			{
				SnapWireEndpoint(wire, startEndpoint: true, 30);
				SnapWireEndpoint(wire, startEndpoint: false, 30);
				FinishDrag();
			}
			else
			{
				SnapComponentToNeighbors(selectedComp, 35);
				FinishDrag();
			}
		}
	}

	private double GetDistance(Point p1, Point p2)
	{
		return Math.Sqrt(Math.Pow(p1.X - p2.X, 2.0) + Math.Pow(p1.Y - p2.Y, 2.0));
	}

	private void SnapComponents(dynamic movingCell, IEnumerable<dynamic> allComponents)
	{
		int snapDist = 20;
		dynamic rightSnapTarget = allComponents.FirstOrDefault((dynamic c) => c != movingCell && GetDistance(movingCell.LeftAnchor, c.RightAnchor) < snapDist);
		if (rightSnapTarget != null)
		{
			movingCell.X = rightSnapTarget.RightAnchor.X;
			movingCell.Y = rightSnapTarget.RightAnchor.Y - 50;
			return;
		}
		dynamic leftSnapTarget = allComponents.FirstOrDefault((dynamic c) => c != movingCell && GetDistance(movingCell.RightAnchor, c.LeftAnchor) < snapDist);
		if (leftSnapTarget != null)
		{
			movingCell.X = leftSnapTarget.LeftAnchor.X - 75;
			movingCell.Y = leftSnapTarget.LeftAnchor.Y - 50;
		}
	}

	protected override void WndProc(ref Message m)
	{
		if (!lockLayoutFrames || (m.Msg != 70 && m.Msg != 131))
		{
			base.WndProc(ref m);
		}
	}

	private void RefreshCanvas()
	{
		if (pictureBox1.Width <= 0 || pictureBox1.Height <= 0)
		{
			return;
		}
		Bitmap bmp = new Bitmap(pictureBox1.ClientRectangle.Width, pictureBox1.ClientRectangle.Height);
		Graphics graphics = Graphics.FromImage(bmp);
		try
		{
			myCircuitList.ForEach((BaseElecPiece component) =>
			{
				component.Draw(graphics);
			});
		}
		finally
		{
			if (graphics != null)
			{
				((IDisposable)graphics).Dispose();
			}
		}
		pictureBox1.Image?.Dispose();
		pictureBox1.Image = bmp;
	}

	private void PictureBox1_Paint(object sender, PaintEventArgs e)
	{
		e.Graphics.Clear(Color.White);
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		foreach (BaseElecPiece comp in myCircuitList)
		{
			comp.Draw(e.Graphics);
		}
	}

	private void pctSimple_Paint(object sender, PaintEventArgs e)
	{
		e.Graphics.Clear(Color.White);
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		foreach (BaseElecPiece comp in myCircuitList)
		{
			comp.Draw(e.Graphics);
		}
	}

	private void btnDiode_Click(object sender, EventArgs e)
	{
		myCircuitList.Add(new Diode
		{
			X = 50,
			Y = 50
		});
		RefreshCanvas();
	}

	private void btnResistor_Click(object sender, EventArgs e)
	{
		myCircuitList.Add(new Resistor
		{
			X = 50,
			Y = 50
		});
		RefreshCanvas();
	}

	private void btnLamp_Click(object sender, EventArgs e)
	{
		myCircuitList.Add(new Lamp
		{
			X = 50,
			Y = 50
		});
		RefreshCanvas();
	}

	protected override void OnActivated(EventArgs e)
	{
		base.OnActivated(e);
		ActiveControl = null;
	}

	private void btnWire_Click(object sender, EventArgs e)
	{
		ActiveControl = null;
		myCircuitList.Add(new Wire
		{
			StartPoint = new Point(50, 50),
			EndPoint = new Point(150, 50)
		});
		RefreshCanvas();
	}

	private void pictureBox2_DragDrop(object sender, DragEventArgs e)
	{
	}

	private void picBin_Click(object sender, EventArgs e)
	{
	}

	private Rectangle getCompBound(BaseElecPiece comp)
	{
		if (comp == null)
		{
			return Rectangle.Empty;
		}
		if (comp is Wire wire)
		{
			return GetWireBound(wire);
		}
		return new Rectangle(comp.X, comp.Y, 80, 80);
	}

	private Rectangle GetWireBound(Wire w)
	{
		Point s = GetWireEndpoint(w.connectedStartComp, w.connectedStartAnch, w.StartPoint);
		Point e = GetWireEndpoint(w.connectedEndComp, w.connectedEndAnch, w.EndPoint);
		int pad = w.handleRad + 6;
		int left = Math.Min(s.X, e.X) - pad;
		int top = Math.Min(s.Y, e.Y) - pad;
		int width = Math.Abs(s.X - e.X) + pad * 2;
		int height = Math.Abs(s.Y - e.Y) + pad * 2;
		return new Rectangle(left, top, Math.Max(1, width), Math.Max(1, height));
	}

	private Point GetWireEndpoint(BaseElecPiece connectedComp, string anchor, Point fallbackPoint)
	{
		if (connectedComp == null)
		{
			return fallbackPoint;
		}
		return (anchor == "Left") ? connectedComp.LeftAnchor : connectedComp.RightAnchor;
	}

	private void StartSimulation()
	{
		if (simCts == null)
		{
			simCts = new CancellationTokenSource();
			simTime = 0.0;
		}
	}

	private void StopSimulation()
	{
		if (simCts != null)
		{
			simCts.Cancel();
			simCts = null;
		}
	}

	private double ComputeCircuitCurrent(double additionalOscillatorVoltage = 0.0)
	{
		double Vsum = additionalOscillatorVoltage;
		foreach (BaseElecPiece comp in myCircuitList)
		{
			if (comp is Battery || comp is Cell)
			{
				Vsum += comp.Voltage;
			}
		}
		double Rtotal = 0.0;
		foreach (BaseElecPiece comp2 in myCircuitList)
		{
			Rtotal += CircuitSimulator.GetEffectiveResistance(comp2, Vsum);
		}
		if (Rtotal <= 0.0)
		{
			return 0.0;
		}
		return Vsum / Rtotal;
	}

	private async Task RunSimulationLoop(CancellationToken token)
	{
		try
		{
			while (!token.IsCancellationRequested)
			{
				StepSimulation();
				simTime += simDt;
				await Task.Delay((int)(simDt * 1000.0), token);
			}
		}
		catch (TaskCanceledException)
		{
		}
		catch (ObjectDisposedException)
		{
		}
	}

	private void StepSimulation()
	{
		double osc = amplitude * Math.Sin(Math.PI * 2.0 * frequency * simTime);
		BaseElecPiece analysedSeed = selectedForIV ?? myCircuitList.OfType<Battery>().FirstOrDefault() ?? myCircuitList.FirstOrDefault();
		List<BaseElecPiece> compProcess = ((analysedSeed != null) ? GetConnectedComp(analysedSeed) : myCircuitList.ToList());
		if (compProcess != null && compProcess.Count != 0)
		{
			List<Wire> connectedWires = compProcess.OfType<Wire>().ToList();
			bool isParallel = IsCircuitParallel(compProcess, connectedWires);
			double current = CircuitSimulator.ComputeCircuitCurrent(compProcess, osc, isParallel);
			double totalVoltage = compProcess.Sum((BaseElecPiece c) => c.Voltage) + osc;
			UpdateAmmeters(compProcess, totalVoltage, current);
			RefreshCanvasSafe();
			double plotVolt = totalVoltage;
			if (selectedForIV != null)
			{
				double rComp = CircuitSimulator.GetEffectiveResistance(selectedForIV, totalVoltage);
				plotVolt = current * rComp;
			}
		}
	}

	private void UpdateAmmeters(List<BaseElecPiece> compProcess, double totalVoltage, double current)
	{
		List<Resistor> branchResistors = (from r in compProcess.OfType<Resistor>()
			orderby r.Y
			select r).ToList();
		List<Ammeter> branchAmmeters = (from a in compProcess.OfType<Ammeter>()
			orderby a.Y
			select a).ToList();
		for (int i = 0; i < branchAmmeters.Count && i < branchResistors.Count; i++)
		{
			double resistance = branchResistors[i].Resistance;
			branchAmmeters[i].MeasuredCurrent = ((resistance > 0.0) ? (totalVoltage / resistance) : 0.0);
		}
		Ammeter mainAmmeter = branchAmmeters.OrderByDescending((Ammeter a) => a.Y).FirstOrDefault();
		if (mainAmmeter != null && !branchResistors.Any((Resistor r) => Math.Abs(r.Y - mainAmmeter.Y) < 40))
		{
			mainAmmeter.MeasuredCurrent = current;
		}
		foreach (Ammeter ammeter in compProcess.OfType<Ammeter>())
		{
			ammeter.MeasuredCurrent = current;
		}
	}

	private void RefreshCanvasSafe()
	{
		if (pictureBox1 == null || pictureBox1.IsDisposed)
		{
			RefreshCanvas();
		}
		else if (pictureBox1.InvokeRequired)
		{
			pictureBox1.BeginInvoke((Action)(() =>
			{
				RefreshCanvas();
			}));
		}
	}

	private void UpdateVoltmeterReadings(List<BaseElecPiece> compProcess, double sourceVoltage)
	{
		List<Resistor> resistors = compProcess.OfType<Resistor>().ToList();
		double totalResistance = resistors.Sum((Resistor resistor) => resistor.Resistance);
		double seriesCurrent = ((totalResistance > 0.0) ? (sourceVoltage / totalResistance) : 0.0);
		foreach (Voltmeter voltmeter in compProcess.OfType<Voltmeter>())
		{
			Resistor measuredResistor = resistors.OrderBy((Resistor resistor) => Math.Abs(resistor.X - voltmeter.X) + Math.Abs(resistor.Y - voltmeter.Y)).FirstOrDefault();
			voltmeter.MeasuredVoltage = ((measuredResistor != null) ? (seriesCurrent * measuredResistor.Resistance) : sourceVoltage);
		}
	}

	private void SimulateOnce()
	{
		BaseElecPiece analisedSeed = selectedForIV ?? myCircuitList.FirstOrDefault();
		analisedSeed = selectedForIV ?? myCircuitList.OfType<Battery>().FirstOrDefault() ?? myCircuitList.OfType<Cell>().FirstOrDefault() ?? myCircuitList.OfType<BaseElecPiece>().FirstOrDefault();
		if (analisedSeed == null)
		{
			MessageBox.Show("No components placed.", "SimulateOnce", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		List<BaseElecPiece> compProcess = GetConnectedComp(analisedSeed);
		Debug.WriteLine($"SimulateOnce: analisedSeed={analisedSeed.GetType().Name}, connectedCount={compProcess.Count}");
		if (compProcess.Count == 0)
		{
			MessageBox.Show("Connected set is empty.", "SimulateOnce", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		double osc = 0.0;
		List<Wire> connectedWires = compProcess.OfType<Wire>().ToList();
		bool isParallel = IsCircuitParallel(compProcess, connectedWires);
		Debug.WriteLine($"Parallel circuit: {isParallel}");
		double current = CircuitSimulator.ComputeCircuitCurrent(compProcess, osc, isParallel);
		double totalVoltage = compProcess.Sum((BaseElecPiece c) => c.Voltage) + osc;
		foreach (Ammeter ammeter in compProcess.OfType<Ammeter>())
		{
			ammeter.MeasuredCurrent = current;
		}
		UpdateVoltmeterReadings(compProcess, totalVoltage);
		List<Resistor> branchResistors = (from resistor in compProcess.OfType<Resistor>()
			orderby resistor.Y
			select resistor).ToList();
		List<Ammeter> branchAmmeters = (from ammeter2 in compProcess.OfType<Ammeter>()
			orderby ammeter2.Y
			select ammeter2).ToList();
		for (int i = 0; i < branchAmmeters.Count && i < branchResistors.Count; i++)
		{
			double resistance = branchResistors[i].Resistance;
			branchAmmeters[i].MeasuredCurrent = ((resistance > 0.0) ? (totalVoltage / resistance) : 0.0);
		}
		Ammeter mainAmmeter = branchAmmeters.OrderByDescending((Ammeter ammeter2) => ammeter2.Y).FirstOrDefault();
		if (mainAmmeter != null && !branchResistors.Any((Resistor resistor) => Math.Abs(resistor.Y - mainAmmeter.Y) < 40))
		{
			mainAmmeter.MeasuredCurrent = current;
		}
		RefreshCanvas();
		double plotVolt = totalVoltage;
		if (selectedForIV != null && compProcess.Contains(selectedForIV))
		{
			double Rcomp = CircuitSimulator.GetEffectiveResistance(selectedForIV, totalVoltage);
			plotVolt = current * Rcomp;
		}
		double graphVoltage = Math.Max(1.0, Math.Abs(totalVoltage));
		PlotCircuitIV(compProcess, graphVoltage);
		Debug.WriteLine($"SimulateOnce result: I={current}, V_total={totalVoltage}, V_plot={plotVolt}");
		MessageBox.Show($"I={current:F6} A\nV_total={totalVoltage:F3} V\nV_plot={plotVolt:F3} V\nconnectedCount={compProcess.Count}", "SimulateOnce", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	private void SnapWireEndpoint(Wire wire, bool startEndpoint, int snapDistance)
	{
		Point endpoint = (startEndpoint ? wire.StartPoint : wire.EndPoint);
		double nearestDistance = double.MaxValue;
		BaseElecPiece nearestComponent = null;
		string nearestAnchor = "";
		Point nearestPoint = Point.Empty;
		myCircuitList.ForEach((BaseElecPiece component) =>
		{
			EvaluateComponentAnchors(component, wire, endpoint, ref nearestDistance, ref nearestComponent, ref nearestAnchor, ref nearestPoint);
		});
		if (nearestComponent != null && !(nearestDistance > (double)snapDistance))
		{
			ApplyWireSnap(wire, startEndpoint, nearestComponent, nearestAnchor, nearestPoint);
		}
	}

	private void EvaluateComponentAnchors(BaseElecPiece component, Wire draggedWire, Point endpoint, ref double nearestDistance, ref BaseElecPiece nearestComponent, ref string nearestAnchor, ref Point nearestPoint)
	{
		if (component != draggedWire)
		{
			if (component is Wire otherWire)
			{
				CheckAnchor(endpoint, otherWire, "StartPoint", otherWire.GetConnectionPoint("StartPoint"), ref nearestDistance, ref nearestComponent, ref nearestAnchor, ref nearestPoint);
				CheckAnchor(endpoint, otherWire, "EndPoint", otherWire.GetConnectionPoint("EndPoint"), ref nearestDistance, ref nearestComponent, ref nearestAnchor, ref nearestPoint);
			}
			else
			{
				CheckAnchor(endpoint, component, "Left", component.LeftAnchor, ref nearestDistance, ref nearestComponent, ref nearestAnchor, ref nearestPoint);
				CheckAnchor(endpoint, component, "Right", component.RightAnchor, ref nearestDistance, ref nearestComponent, ref nearestAnchor, ref nearestPoint);
			}
		}
	}

	private void CheckAnchor(Point endpoint, BaseElecPiece candidateComp, string candidateAnchor, Point candidatePoint, ref double nearestDistance, ref BaseElecPiece nearestComponent, ref string nearestAnchor, ref Point nearestPoint)
	{
		double distance = GetDistance(endpoint, candidatePoint);
		if (!(distance >= nearestDistance))
		{
			nearestDistance = distance;
			nearestComponent = candidateComp;
			nearestAnchor = candidateAnchor;
			nearestPoint = candidatePoint;
		}
	}

	private void ApplyWireSnap(Wire wire, bool startEndpoint, BaseElecPiece targetComp, string targetAnchor, Point targetPoint)
	{
		if (startEndpoint)
		{
			wire.connectedStartComp = targetComp;
			wire.connectedStartAnch = targetAnchor;
			wire.StartPoint = targetPoint;
		}
		else
		{
			wire.connectedEndComp = targetComp;
			wire.connectedEndAnch = targetAnchor;
			wire.EndPoint = targetPoint;
		}
	}

	private void chtIV_Click(object sender, EventArgs e)
	{
	}

	private void chtVt_Click(object sender, EventArgs e)
	{
	}

	private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		BaseElecPiece clicked = null;
		for (int i = myCircuitList.Count - 1; i >= 0; i--)
		{
			BaseElecPiece comp = myCircuitList[i];
			if (comp.IsHit(new Point(e.X, e.Y), 8f) || comp.ContainsPoint(e.X, e.Y))
			{
				clicked = comp;
				break;
			}
		}
		if (clicked != null)
		{
			selectedForIV = clicked;
			PlotIV(selectedForIV, 20.0, 400);
		}
	}

	private void PlotIV(BaseElecPiece comp, double vMax = 20.0, int steps = 200)
	{
		if (comp != null && chtIV != null)
		{
			if (chtIV.Series.Count == 0)
			{
				chtIV.Series.Add("I-V");
			}
			Series series = chtIV.Series[0];
			series.Points.Clear();
			double step = 2.0 * vMax / (double)steps;
			for (int i = 0; i <= steps; i++)
			{
				double v = 0.0 - vMax + (double)i * step;
				double R = CircuitSimulator.GetEffectiveResistance(comp, v);
				double I = ((R <= 0.0) ? 0.0 : (v / R));
				series.Points.AddXY(v, I);
			}
			chtIV.Invalidate();
		}
	}

	private void PlotCircuitIV(IEnumerable<BaseElecPiece> circuitComponents, double maximumVoltage, int steps = 200)
	{
		if (circuitComponents == null || chtIV == null || steps <= 0)
		{
			return;
		}
		List<BaseElecPiece> components = FilterPassiveComponents(circuitComponents);
		if (components.Count != 0)
		{
			List<Wire> wires = circuitComponents.OfType<Wire>().ToList();
			bool isParallel = IsCircuitParallel(circuitComponents, wires);
			double safeMaxVoltage = Math.Max(1.0, Math.Abs(maximumVoltage));
			double minVoltage = 0.0 - safeMaxVoltage;
			double voltageStep = 2.0 * safeMaxVoltage / (double)steps;
			double[] voltages = (from i in Enumerable.Range(0, steps + 1)
				select minVoltage + (double)i * voltageStep).ToArray();
			double[] currents = voltages.Select((double v) => ComputeCurrentAtVoltage(components, v, isParallel)).ToArray();
			RenderIVChart(voltages, currents);
		}
	}

	private List<BaseElecPiece> FilterPassiveComponents(IEnumerable<BaseElecPiece> circuitComponents)
	{
		return circuitComponents.Where((BaseElecPiece c) => c != null && !(c is Wire) && !(c is Ammeter) && !(c is Voltmeter) && !(c is Battery) && !(c is Cell)).ToList();
	}

	private double ComputeCurrentAtVoltage(List<BaseElecPiece> components, double voltage, bool isParallel)
	{
		if (isParallel)
		{
			return ComputeParallelCurrent(components, voltage);
		}
		double totalResistance = components.Sum((BaseElecPiece c) => CircuitSimulator.GetEffectiveResistance(c, voltage));
		return (totalResistance > 0.0 && !double.IsPositiveInfinity(totalResistance)) ? (voltage / totalResistance) : 0.0;
	}

	private double ComputeParallelCurrent(List<BaseElecPiece> components, double voltage)
	{
		double totalConductance = (from c in components
			select CircuitSimulator.GetEffectiveResistance(c, voltage) into r
			where r > 0.0 && !double.IsPositiveInfinity(r)
			select r).Sum((double r) => 1.0 / r);
		return voltage * totalConductance;
	}

	private void RenderIVChart(double[] voltages, double[] currents)
	{
		if (chtIV.Series.Count == 0)
		{
			chtIV.Series.Add("I-V");
		}
		Series series = chtIV.Series[0];
		series.Points.Clear();
		Enumerable.Range(0, voltages.Length).ToList().ForEach((int i) =>
		{
			series.Points.AddXY(voltages[i], currents[i]);
		});
		chtIV.ChartAreas[0].RecalculateAxesScale();
		chtIV.Invalidate();
	}

	private List<BaseElecPiece> GetConnectedComp(BaseElecPiece analisedSeed)
	{
		if (analisedSeed == null)
		{
			return new List<BaseElecPiece>();
		}
		HashSet<BaseElecPiece> visited = new HashSet<BaseElecPiece>();
		Queue<BaseElecPiece> queue = new Queue<BaseElecPiece>();
		List<Wire> allWires = myCircuitList.OfType<Wire>().ToList();
		TryVisit(analisedSeed, visited, queue);
		while (queue.Count > 0)
		{
			BaseElecPiece current = queue.Dequeue();
			EnqueueNeighbors(current, allWires, visited, queue);
		}
		return visited.ToList();
	}

	private void EnqueueNeighbors(BaseElecPiece current, List<Wire> allWires, HashSet<BaseElecPiece> visited, Queue<BaseElecPiece> queue)
	{
		allWires.Where((Wire w) => w.connectedStartComp == current || w.connectedEndComp == current).ToList().ForEach((Wire w) =>
		{
			VisitWireAndOpposite(w, current, visited, queue);
		});
		if (current is Wire wireCur)
		{
			TryVisit(wireCur.connectedStartComp, visited, queue);
			TryVisit(wireCur.connectedEndComp, visited, queue);
		}
	}

	private void VisitWireAndOpposite(Wire wire, BaseElecPiece current, HashSet<BaseElecPiece> visited, Queue<BaseElecPiece> queue)
	{
		TryVisit(wire, visited, queue);
		BaseElecPiece oppositeComp = ((wire.connectedStartComp == current) ? wire.connectedEndComp : wire.connectedStartComp);
		TryVisit(oppositeComp, visited, queue);
	}

	private void TryVisit(BaseElecPiece piece, HashSet<BaseElecPiece> visited, Queue<BaseElecPiece> queue)
	{
		if (piece != null && visited.Add(piece))
		{
			queue.Enqueue(piece);
		}
	}

	private void CircuitState()
	{
		List<string> lines = new List<string> { $"Total components: {myCircuitList.Count}" };
		lines.AddRange(myCircuitList.Select((BaseElecPiece c, int i) => FormatComponentState(c, i)));
		string summary = string.Join(Environment.NewLine, lines);
		Debug.WriteLine(summary);
		MessageBox.Show(summary, "Circuit State", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	private string FormatComponentState(BaseElecPiece c, int index)
	{
		if (c is Wire w)
		{
			string startComponent = ((w.connectedStartComp != null) ? w.connectedStartComp.GetType().Name : "None");
			string endComponent = ((w.connectedEndComp != null) ? w.connectedEndComp.GetType().Name : "None");
			return $"{index}: Wire From {w.StartPoint} to {w.EndPoint} | Links: {startComponent} -> {endComponent}";
		}
		string type = c.GetType().Name;
		return $"[{index}] Type: {type} | Position: ({c.X}, {c.Y}) | V = {c.Voltage}V, R = {c.Resistance}Ω";
	}

	private void btnSimulate_Click_1(object sender, EventArgs e)
	{
		CircuitState();
		SimulateOnce();
	}

	private string FormatResistance(double ohms)
	{
		if (ohms >= 1000000.0)
		{
			return $"{ohms / 1000000.0:F2} MΩ";
		}
		if (ohms >= 1000.0)
		{
			return $"{ohms / 1000.0:F2} kΩ";
		}
		return $"{ohms:F0} Ω";
	}

	private string FormatVoltage(double v)
	{
		return $"{v:F2} V";
	}

	private void SyncTrackbar()
	{
		if (selectedComp == null || compTrack == null || lblComponentValue == null)
		{
			editingComponent = null;
			muteTrack = false;
			compTrack?.Hide();
			lblComponentValue?.Hide();
			muteTrack = false;
			return;
		}
		editingComponent = selectedComp;
		compTrack.Show();
		lblComponentValue.Show();
		compTrack.Location = new Point(Math.Max(0, selectedComp.X), Math.Max(0, selectedComp.Y - 40));
		lblComponentValue.Location = new Point(compTrack.Location.X, Math.Max(0, compTrack.Location.Y - 20));
		compTrack.BringToFront();
		lblComponentValue.BringToFront();
		muteTrack = true;
		if (editingComponent is Resistor r)
		{
			compTrack.Minimum = 0;
			compTrack.Maximum = 500;
			compTrack.Value = Math.Min(compTrack.Maximum, Math.Max(compTrack.Minimum, (int)Math.Round(r.Resistance)));
			lblComponentValue.Text = $"R = {r.Resistance:F0} Ω";
		}
		else if (editingComponent is Battery b)
		{
			compTrack.Minimum = (int)Math.Floor(b.MinVoltage);
			compTrack.Maximum = (int)Math.Ceiling(b.MaxVoltage);
			compTrack.Value = Math.Min(compTrack.Maximum, Math.Max(compTrack.Minimum, (int)Math.Round(b.Voltage)));
			lblComponentValue.Text = $"V = {b.Voltage:F1} V";
		}
		else if (editingComponent is Cell c)
		{
			compTrack.Minimum = 0;
			compTrack.Maximum = 20;
			compTrack.Value = Math.Min(compTrack.Maximum, Math.Max(compTrack.Minimum, (int)Math.Round(c.Voltage)));
			lblComponentValue.Text = $"V = {c.Voltage:F1} V";
		}
		else
		{
			editingComponent = null;
			compTrack.Hide();
			lblComponentValue.Hide();
		}
		muteTrack = false;
	}

	private void ComponentTrackBar_Scroll(object sender, EventArgs e)
	{
		if (muteTrack)
		{
			return;
		}
		BaseElecPiece target = editingComponent ?? selectedComp;
		if (target == null)
		{
			Debug.WriteLine($"TrackBar change but no editing component (value={compTrack.Value})");
			return;
		}
		int v = compTrack.Value;
		if (target is Resistor r)
		{
			r.Resistance = v;
			lblComponentValue.Text = $"R = {r.Resistance:F0} Ω";
		}
		else if (target is Battery b)
		{
			b.SetVoltage(v);
			lblComponentValue.Text = $"V = {b.Voltage:F1} V";
		}
		else if (target is Cell c)
		{
			c.Voltage = v;
			lblComponentValue.Text = $"V = {c.Voltage:F1} V";
		}
		compTrack.BringToFront();
		lblComponentValue.BringToFront();
		Debug.WriteLine($"TrackBar -> editing={target.GetType().Name}, value={v}, model R={target.Resistance}, V={target.Voltage}");
		RefreshCanvas();
	}

	public static bool IsCircuitParallel(IEnumerable<BaseElecPiece> components, IEnumerable<Wire> wires)
	{
		if (components == null || wires == null)
		{
			return false;
		}
		List<BaseElecPiece> componentList = GetLoadComponents(components);
		if (componentList.Count < 2)
		{
			return false;
		}
		List<Wire> wireList = wires.Where((Wire wire) => wire != null).ToList();
		if (componentList.Any((BaseElecPiece component) => HasBranchingTerminal(component, wireList)))
		{
			return true;
		}
		return wireList.Any((Wire wire) => wire.connectedStartComp is Wire || wire.connectedEndComp is Wire);
	}

	private static List<BaseElecPiece> GetLoadComponents(IEnumerable<BaseElecPiece> components)
	{
		return components.Where((BaseElecPiece component) => component != null && !(component is Wire) && !(component is Battery) && !(component is Cell) && !(component is Ammeter) && !(component is Voltmeter)).ToList();
	}

	private static bool HasBranchingTerminal(BaseElecPiece component, List<Wire> wireList)
	{
		int leftConnections = CountAnchorConnections(component, "Left", wireList);
		int rightConnections = CountAnchorConnections(component, "Right", wireList);
		return leftConnections > 1 || rightConnections > 1;
	}

	private static int CountAnchorConnections(BaseElecPiece component, string anchor, List<Wire> wireList)
	{
		return Enumerable.Count(wireList, (Wire wire) => (wire.connectedStartComp == component && wire.connectedStartAnch == anchor) || (wire.connectedEndComp == component && wire.connectedEndAnch == anchor));
	}

	private static bool IsNear(Point p1, Point p2, int tolerance = 5)
	{
		return Math.Abs(p1.X - p2.X) <= tolerance && Math.Abs(p1.Y - p2.Y) <= tolerance;
	}

	public static double ComputeCircuitCurrent(IEnumerable<BaseElecPiece> components, IEnumerable<Wire> wires, double additionalOscillatorVoltage = 0.0)
	{
		bool isParallel = IsCircuitParallel(components, wires);
		return CircuitSimulator.ComputeCircuitCurrent(components, additionalOscillatorVoltage, isParallel);
	}

	private void pictureBox2_Click(object sender, EventArgs e)
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && CircuitComponent != null)
		{
			CircuitComponent.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
		System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
		System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WindowsFormsApp1.Form1));
		this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
		this.simTimer = new System.Windows.Forms.Timer(this.components);
		this.tabCircuit = new System.Windows.Forms.TabPage();
		this.lblComponentValue = new System.Windows.Forms.Label();
		this.compTrack = new System.Windows.Forms.TrackBar();
		this.btnSimulate = new System.Windows.Forms.Button();
		this.chtIV = new System.Windows.Forms.DataVisualization.Charting.Chart();
		this.picBin = new System.Windows.Forms.PictureBox();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.tlsComponents = new System.Windows.Forms.ToolStrip();
		this.btnBattery = new System.Windows.Forms.ToolStripButton();
		this.btnCell = new System.Windows.Forms.ToolStripButton();
		this.btnResistor = new System.Windows.Forms.ToolStripButton();
		this.btnLamp = new System.Windows.Forms.ToolStripButton();
		this.btnDiode = new System.Windows.Forms.ToolStripButton();
		this.tlsMaterials = new System.Windows.Forms.ToolStrip();
		this.btnWire = new System.Windows.Forms.ToolStripButton();
		this.btnSwitch = new System.Windows.Forms.ToolStripButton();
		this.tlsMeasures = new System.Windows.Forms.ToolStrip();
		this.btnAmmeter = new System.Windows.Forms.ToolStripButton();
		this.btnVoltmeter = new System.Windows.Forms.ToolStripButton();
		this.tabSimulation = new System.Windows.Forms.TabControl();
		this.tabPage2 = new System.Windows.Forms.TabPage();
		this.richTextBox1 = new System.Windows.Forms.RichTextBox();
		this.pictureBox2 = new System.Windows.Forms.PictureBox();
		this.componentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.materialsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.measuresToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.tabCircuit.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.compTrack).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.chtIV).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.picBin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		this.tlsComponents.SuspendLayout();
		this.tlsMaterials.SuspendLayout();
		this.tlsMeasures.SuspendLayout();
		this.tabSimulation.SuspendLayout();
		this.tabPage2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox2).BeginInit();
		this.menuStrip1.SuspendLayout();
		base.SuspendLayout();
		this.simTimer.Tick += new System.EventHandler(this.Form1_Load);
		this.tabCircuit.Controls.Add(this.lblComponentValue);
		this.tabCircuit.Controls.Add(this.compTrack);
		this.tabCircuit.Controls.Add(this.btnSimulate);
		this.tabCircuit.Controls.Add(this.chtIV);
		this.tabCircuit.Controls.Add(this.picBin);
		this.tabCircuit.Controls.Add(this.pictureBox1);
		this.tabCircuit.Location = new System.Drawing.Point(4, 25);
		this.tabCircuit.Name = "tabCircuit";
		this.tabCircuit.Padding = new System.Windows.Forms.Padding(3);
		this.tabCircuit.Size = new System.Drawing.Size(1160, 643);
		this.tabCircuit.TabIndex = 0;
		this.tabCircuit.Text = "Default";
		this.tabCircuit.UseVisualStyleBackColor = true;
		this.lblComponentValue.AutoSize = true;
		this.lblComponentValue.Location = new System.Drawing.Point(544, 553);
		this.lblComponentValue.Name = "lblComponentValue";
		this.lblComponentValue.Size = new System.Drawing.Size(126, 16);
		this.lblComponentValue.TabIndex = 12;
		this.lblComponentValue.Text = "Resistance/Voltage";
		this.lblComponentValue.Visible = false;
		this.compTrack.Location = new System.Drawing.Point(521, 572);
		this.compTrack.Name = "compTrack";
		this.compTrack.Size = new System.Drawing.Size(184, 56);
		this.compTrack.TabIndex = 11;
		this.compTrack.Visible = false;
		this.btnSimulate.Location = new System.Drawing.Point(929, 466);
		this.btnSimulate.Name = "btnSimulate";
		this.btnSimulate.Size = new System.Drawing.Size(200, 72);
		this.btnSimulate.TabIndex = 8;
		this.btnSimulate.Text = "Simulate";
		this.btnSimulate.UseVisualStyleBackColor = true;
		this.btnSimulate.Click += new System.EventHandler(this.btnSimulate_Click_1);
		chartArea1.Name = "ChartArea1";
		this.chtIV.ChartAreas.Add(chartArea1);
		legend1.Name = "Legend1";
		this.chtIV.Legends.Add(legend1);
		this.chtIV.Location = new System.Drawing.Point(855, 34);
		this.chtIV.Name = "chtIV";
		series1.ChartArea = "ChartArea1";
		series1.Legend = "Legend1";
		series1.Name = "Series1";
		this.chtIV.Series.Add(series1);
		this.chtIV.Size = new System.Drawing.Size(297, 169);
		this.chtIV.TabIndex = 6;
		this.chtIV.Text = "I-V graph";
		this.chtIV.Click += new System.EventHandler(this.chtIV_Click);
		this.picBin.BackColor = System.Drawing.Color.DimGray;
		this.picBin.Image = WindowsFormsApp1.Properties.Resources.trash1;
		this.picBin.Location = new System.Drawing.Point(1061, 544);
		this.picBin.Name = "picBin";
		this.picBin.Size = new System.Drawing.Size(91, 91);
		this.picBin.TabIndex = 5;
		this.picBin.TabStop = false;
		this.picBin.Click += new System.EventHandler(this.picBin_Click);
		this.picBin.DragDrop += new System.Windows.Forms.DragEventHandler(this.pictureBox2_DragDrop);
		this.pictureBox1.BackColor = System.Drawing.Color.White;
		this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pictureBox1.Location = new System.Drawing.Point(3, 3);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(1154, 637);
		this.pictureBox1.TabIndex = 4;
		this.pictureBox1.TabStop = false;
		this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
		this.pictureBox1.DoubleClick += new System.EventHandler(this.pictureBox1_DoubleClick);
		this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
		this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
		this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseUp);
		this.tlsComponents.BackColor = System.Drawing.Color.DimGray;
		this.tlsComponents.ImageScalingSize = new System.Drawing.Size(20, 20);
		this.tlsComponents.Items.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.btnBattery, this.btnCell, this.btnResistor, this.btnLamp, this.btnDiode });
		this.tlsComponents.Location = new System.Drawing.Point(0, 0);
		this.tlsComponents.Name = "tlsComponents";
		this.tlsComponents.Size = new System.Drawing.Size(1168, 31);
		this.tlsComponents.TabIndex = 3;
		this.tlsComponents.Text = "toolStrip3";
		this.tlsComponents.Visible = false;
		this.btnBattery.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnBattery.Image = (System.Drawing.Image)resources.GetObject("btnBattery.Image");
		this.btnBattery.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnBattery.Name = "btnBattery";
		this.btnBattery.Size = new System.Drawing.Size(29, 28);
		this.btnBattery.Text = "Battery";
		this.btnBattery.Click += new System.EventHandler(this.btnBattery_Click);
		this.btnCell.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnCell.Image = (System.Drawing.Image)resources.GetObject("btnCell.Image");
		this.btnCell.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnCell.Name = "btnCell";
		this.btnCell.Size = new System.Drawing.Size(29, 28);
		this.btnCell.Text = "Cell";
		this.btnCell.Click += new System.EventHandler(this.btnCell_Click);
		this.btnResistor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnResistor.Image = (System.Drawing.Image)resources.GetObject("btnResistor.Image");
		this.btnResistor.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnResistor.Name = "btnResistor";
		this.btnResistor.Size = new System.Drawing.Size(29, 28);
		this.btnResistor.Text = "Resistor";
		this.btnResistor.Click += new System.EventHandler(this.btnResistor_Click);
		this.btnLamp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnLamp.Image = (System.Drawing.Image)resources.GetObject("btnLamp.Image");
		this.btnLamp.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnLamp.Name = "btnLamp";
		this.btnLamp.Size = new System.Drawing.Size(29, 28);
		this.btnLamp.Text = "Lamp";
		this.btnLamp.Click += new System.EventHandler(this.btnLamp_Click);
		this.btnDiode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnDiode.Image = (System.Drawing.Image)resources.GetObject("btnDiode.Image");
		this.btnDiode.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnDiode.Name = "btnDiode";
		this.btnDiode.Size = new System.Drawing.Size(29, 28);
		this.btnDiode.Text = "Diode";
		this.btnDiode.Click += new System.EventHandler(this.btnDiode_Click);
		this.tlsMaterials.BackColor = System.Drawing.Color.DimGray;
		this.tlsMaterials.ImageScalingSize = new System.Drawing.Size(20, 20);
		this.tlsMaterials.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.btnWire, this.btnSwitch });
		this.tlsMaterials.Location = new System.Drawing.Point(0, 0);
		this.tlsMaterials.Name = "tlsMaterials";
		this.tlsMaterials.Size = new System.Drawing.Size(1168, 27);
		this.tlsMaterials.TabIndex = 1;
		this.tlsMaterials.Text = "toolStrip1";
		this.tlsMaterials.Visible = false;
		this.tlsMaterials.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.tlsMaterials_ItemClicked);
		this.btnWire.AutoSize = false;
		this.btnWire.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnWire.Image = (System.Drawing.Image)resources.GetObject("btnWire.Image");
		this.btnWire.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnWire.Name = "btnWire";
		this.btnWire.Size = new System.Drawing.Size(29, 24);
		this.btnWire.Text = "Wire";
		this.btnWire.Click += new System.EventHandler(this.btnWire_Click);
		this.btnSwitch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnSwitch.Image = (System.Drawing.Image)resources.GetObject("btnSwitch.Image");
		this.btnSwitch.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnSwitch.Name = "btnSwitch";
		this.btnSwitch.Size = new System.Drawing.Size(29, 24);
		this.btnSwitch.Text = "Switch";
		this.btnSwitch.Click += new System.EventHandler(this.btnSwitch_Click);
		this.tlsMeasures.BackColor = System.Drawing.Color.DimGray;
		this.tlsMeasures.ImageScalingSize = new System.Drawing.Size(20, 20);
		this.tlsMeasures.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.btnAmmeter, this.btnVoltmeter });
		this.tlsMeasures.Location = new System.Drawing.Point(0, 0);
		this.tlsMeasures.Name = "tlsMeasures";
		this.tlsMeasures.Size = new System.Drawing.Size(1168, 31);
		this.tlsMeasures.TabIndex = 2;
		this.tlsMeasures.Text = "toolStrip2";
		this.tlsMeasures.Visible = false;
		this.btnAmmeter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnAmmeter.Image = (System.Drawing.Image)resources.GetObject("btnAmmeter.Image");
		this.btnAmmeter.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnAmmeter.Name = "btnAmmeter";
		this.btnAmmeter.Size = new System.Drawing.Size(29, 28);
		this.btnAmmeter.Text = "Ammeter";
		this.btnAmmeter.Click += new System.EventHandler(this.btnAmmeter_Click);
		this.btnVoltmeter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnVoltmeter.Image = (System.Drawing.Image)resources.GetObject("btnVoltmeter.Image");
		this.btnVoltmeter.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnVoltmeter.Name = "btnVoltmeter";
		this.btnVoltmeter.Size = new System.Drawing.Size(29, 28);
		this.btnVoltmeter.Text = "Voltmeter";
		this.btnVoltmeter.Click += new System.EventHandler(this.btnVoltmeter_Click);
		this.tabSimulation.Controls.Add(this.tabCircuit);
		this.tabSimulation.Controls.Add(this.tabPage2);
		this.tabSimulation.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tabSimulation.Location = new System.Drawing.Point(0, 28);
		this.tabSimulation.Name = "tabSimulation";
		this.tabSimulation.SelectedIndex = 0;
		this.tabSimulation.Size = new System.Drawing.Size(1168, 672);
		this.tabSimulation.TabIndex = 3;
		this.tabPage2.Controls.Add(this.richTextBox1);
		this.tabPage2.Controls.Add(this.pictureBox2);
		this.tabPage2.Location = new System.Drawing.Point(4, 25);
		this.tabPage2.Name = "tabPage2";
		this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
		this.tabPage2.Size = new System.Drawing.Size(1160, 643);
		this.tabPage2.TabIndex = 2;
		this.tabPage2.Text = "Tutorial";
		this.tabPage2.UseVisualStyleBackColor = true;
		this.richTextBox1.Location = new System.Drawing.Point(6, 6);
		this.richTextBox1.Name = "richTextBox1";
		this.richTextBox1.ReadOnly = true;
		this.richTextBox1.Size = new System.Drawing.Size(1146, 629);
		this.richTextBox1.TabIndex = 1;
		this.richTextBox1.Text = "";
		this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pictureBox2.Location = new System.Drawing.Point(3, 3);
		this.pictureBox2.Name = "pictureBox2";
		this.pictureBox2.Size = new System.Drawing.Size(1154, 637);
		this.pictureBox2.TabIndex = 0;
		this.pictureBox2.TabStop = false;
		this.componentsToolStripMenuItem.Name = "componentsToolStripMenuItem";
		this.componentsToolStripMenuItem.Size = new System.Drawing.Size(107, 24);
		this.componentsToolStripMenuItem.Text = "Components";
		this.componentsToolStripMenuItem.Click += new System.EventHandler(this.componentsToolStripMenuItem_Click);
		this.materialsToolStripMenuItem.Name = "materialsToolStripMenuItem";
		this.materialsToolStripMenuItem.Size = new System.Drawing.Size(60, 24);
		this.materialsToolStripMenuItem.Text = "Wires";
		this.materialsToolStripMenuItem.Click += new System.EventHandler(this.materialsToolStripMenuItem_Click);
		this.measuresToolStripMenuItem.Name = "measuresToolStripMenuItem";
		this.measuresToolStripMenuItem.Size = new System.Drawing.Size(85, 24);
		this.measuresToolStripMenuItem.Text = "Measures";
		this.measuresToolStripMenuItem.Click += new System.EventHandler(this.measuresToolStripMenuItem_Click);
		this.menuStrip1.BackColor = System.Drawing.Color.Cyan;
		this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.componentsToolStripMenuItem, this.materialsToolStripMenuItem, this.measuresToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(1168, 28);
		this.menuStrip1.TabIndex = 0;
		this.menuStrip1.Text = "menuStrip1";
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1168, 700);
		base.Controls.Add(this.tabSimulation);
		base.Controls.Add(this.menuStrip1);
		base.Controls.Add(this.tlsComponents);
		base.Controls.Add(this.tlsMaterials);
		base.Controls.Add(this.tlsMeasures);
		base.Name = "Form1";
		this.Text = "Physics Engine";
		base.Load += new System.EventHandler(this.LoadTutorial);
		base.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
		this.tabCircuit.ResumeLayout(false);
		this.tabCircuit.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.compTrack).EndInit();
		((System.ComponentModel.ISupportInitialize)this.chtIV).EndInit();
		((System.ComponentModel.ISupportInitialize)this.picBin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		this.tlsComponents.ResumeLayout(false);
		this.tlsComponents.PerformLayout();
		this.tlsMaterials.ResumeLayout(false);
		this.tlsMaterials.PerformLayout();
		this.tlsMeasures.ResumeLayout(false);
		this.tlsMeasures.PerformLayout();
		this.tabSimulation.ResumeLayout(false);
		this.tabPage2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.pictureBox2).EndInit();
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	private void resistorToolStripMenuItem_Click(object sender, EventArgs e)
	{
		throw new NotImplementedException();
	}

	private void Form1_Load(object sender, EventArgs e)
	{
		SetupCharts();
	}

	private void menuStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
	{
	}
}
