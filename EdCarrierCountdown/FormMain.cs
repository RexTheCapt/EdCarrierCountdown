using System.ComponentModel;

using EdTools;

namespace EdCarrierCountdown
{
    public partial class FormMain : Form
    {
        private JournalScanner js = new();
        private int fuelLevel = 0;
        private float jumpRangeCurr = 0;
        private float jumpRangeMax = 0;
        private string SystemName = "";
        private DateTime jumpETA = DateTime.MinValue;
        private List<string> lastEvents = new();

        public FormMain()
        {
            InitializeComponent();

            JournalScanner.OnEventHandler += JournalScanner_OnEventHandler;

            this.TopMost = true;
            panel1.BackgroundImageLayout = ImageLayout.Stretch;
            panel2.BackgroundImageLayout = ImageLayout.Stretch;
            panel2.Parent = this;

            this.DoubleBuffered = true;

            label1.BackColor = Color.Transparent;
            label2.BackColor = Color.Transparent;

            label1.Parent = panel1;
            label2.Parent = panel2;
        }

        private void JournalScanner_OnEventHandler(object? sender, EventArgs e)
        {
            List<string> subs = new() { "CarrierJumpRequest", "CarrierJump", "CarrierStats", "CarrierJumpCancelled", "CarrierDepositFuel" };
            var eargs = (JournalScanner.OnEventArgs)e;
            JsonClass.Root root = eargs.OnEvent;
            var ev = eargs.OnEvent._event;

            while (lastEvents.Count > 50)
                lastEvents.RemoveAt(0);

            DateTimeOffset dto = new(root.timestamp);
            lastEvents.Add($"{dto.LocalDateTime} {root._event}");

            //string s = "";

            //foreach (string l in lastEvents)
            //    s += $"{l}\r\n";

            //textBox1.Text = s;

            if (!subs.Contains(ev)) return;

            switch (ev)
            {
                case "CarrierJumpRequest":
                    SystemName = root.SystemName;
                    jumpETA = dto.LocalDateTime.AddMinutes(15).AddSeconds(14);
                    break;
                case "CarrierJump":
                    SystemName = root.StarSystem;
                    break;
                case "CarrierStats":
                    fuelLevel = (int)root.FuelLevel;
                    jumpRangeCurr = (int)root.JumpRangeCurr;
                    jumpRangeMax = (int)root.JumpRangeMax;
                    break;
                case "CarrierJumpCancelled":
                    break;
                case "CarrierDepositFuel":
                    fuelLevel = (int)root.Total;
                    break;
            }
        }

        int countdownTick = 0;
        private void timerCountdown_Tick(object sender, EventArgs e)
        {
            countdownTick++;
            js.Tick();

            var lockdown = GetTime(jumpETA.AddMinutes(-3).AddSeconds(-20));
            var jump = GetTime(jumpETA);
            var EtaArrival = GetTime(jumpETA.AddSeconds(60));
            var cooldown = GetTime(jumpETA.AddMinutes(5).AddSeconds(-10), false);

            string output = $"{lockdown:mm\\:ss} | {jump:mm\\:ss} | {EtaArrival:mm\\:ss} | {cooldown:mm\\:ss}\n{SystemName}\n{fuelLevel}t | {jumpRangeCurr}ly";

            label1.Text = label2.Text = output;

            TimeSpan maxmin = new(0, 20, 0);

            if (countdownTick % 10 != 0) return;

            if (countdownTick % 20 == 0)
                panel1.BackgroundImage?.Dispose();
            if (countdownTick % 20 == 10)
                panel2.BackgroundImage?.Dispose();
            
            var width = this.Width;
            var img = new Bitmap(width,1);

            for (int w = 0; w < width; w++)
            {
                double part = maxmin.TotalMilliseconds / width * w;

                Color c;

                if (lockdown.TotalMilliseconds > part)
                    c = Color.Magenta;
                else if (jump.TotalMilliseconds > part)
                    c = Color.Green;
                else if (EtaArrival.TotalMilliseconds > part)
                    c = Color.Yellow;
                else if (cooldown.TotalMilliseconds > part)
                    c = Color.Red;
                else
                    c = this.BackColor;

                img.SetPixel(w, 0, c);
            }

            if (countdownTick % 20 == 0)
            {
                panel1.BackgroundImage = img;
                panel1.BringToFront();
            }
            if (countdownTick % 20 == 10)
            {
                panel2.BackgroundImage = img;
                panel2.BringToFront();
            }
        }

        private TimeSpan GetTime(DateTime time, bool stayNegative = true)
        {
            if (DateTime.Now > time && stayNegative)
                time = DateTime.Now;
            return time - DateTime.Now;
        }
    }
}