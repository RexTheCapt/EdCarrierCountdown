using System.ComponentModel;
using System.Text;

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
            StringBuilder sb = new();

            if (jumpETA != DateTime.MinValue)
            {
                TimeSpan lockdown;
                TimeSpan jump;
                TimeSpan EtaArrival;
                TimeSpan cooldown;

                lockdown = GetTime(jumpETA.AddMinutes(-3).AddSeconds(-20));
                jump = GetTime(jumpETA);
                EtaArrival = GetTime(jumpETA.AddSeconds(60));
                cooldown = GetTime(jumpETA.AddMinutes(5).AddSeconds(-10), false);

                sb.Append($"{lockdown:mm\\:ss} | {jump:mm\\:ss} | {EtaArrival:mm\\:ss} | {cooldown:mm\\:ss}\n");

            TimeSpan maxmin = new(0, 20, 0);

            if (countdownTick % 10 != 0) return;

            if (countdownTick % 20 == 0)
                panel1.BackgroundImage?.Dispose();
            if (countdownTick % 20 == 10)
                panel2.BackgroundImage?.Dispose();
            
            var width = this.Width;
                var img = new Bitmap(width, 1);
                TimeSpan maxmin = new(0, 20, 0);

            for (int w = 0; w < width; w++)
            {
                double part = maxmin.TotalMilliseconds / width * w;

                Color c;

                    Color lockinColor = Color.DarkGreen;
                    Color lockdownColor1 = Color.Green;
                    Color lockdownColor2 = Color.Magenta;
                    Color jumpColor = Color.Blue;
                    Color EtaArrivalColor = Color.Orange;
                    Color cooldownColor = Color.Cyan;

                    if (jump.Add(new(0, -10, 0)).TotalMilliseconds > part)
                        c = lockinColor;
                    else if (lockdown.TotalMilliseconds > part)
                        c = lockdownColor1;
                else if (jump.TotalMilliseconds > part)
                        c = jumpColor;
                else if (EtaArrival.TotalMilliseconds > part)
                        c = EtaArrivalColor;
                else if (cooldown.TotalMilliseconds > part)
                        c = cooldownColor;
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
            else
                sb.Append($"--:-- | --:-- | --:-- | --:--\n");

            sb.Append($"{SystemName}\n{fuelLevel}t | {jumpRangeCurr}ly");

            label1.Text = label2.Text = sb.ToString();
        }

        private TimeSpan GetTime(DateTime time, bool stayNegative = true)
        {
            if (DateTime.Now > time && stayNegative)
                time = DateTime.Now;
            return time - DateTime.Now;
        }
    }
}