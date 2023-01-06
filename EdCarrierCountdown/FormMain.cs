using System.ComponentModel;
using System.Configuration;
using System.Text;

using EdTools;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdCarrierCountdown
{
    public partial class FormMain : Form
    {
        private JournalScanner js = new();
        private List<string> lastEvents = new();
        private int CarrierIndex = 0;
        private Settings settings = new Settings();

        public FormMain()
        {
            InitializeComponent();

            if (File.Exists("settings.json"))
            {
                var v = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
                if (v != null)
                    settings = v;
            }
            else
                File.WriteAllText("settings.json", JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(settings)).ToString());

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

            label1.Click += Label_Click;
            label2.Click += Label_Click;
        }

        private void Label_Click(object? sender, EventArgs e)
        {
            CarrierIndex++;

            //if (carrierInfos.Count == 1)
            //{
            //    CarrierInfo ci = carrierInfos.First();

            //    CarrierInfo n = new()
            //    {
            //        CallSign = "123-ABC",
            //        Destination = "Sagittarius",
            //        FuelLevel = 666,
            //        Journal = "journal",
            //        JumpETA = ci.JumpETA.AddDays(1).AddMinutes(-12),
            //        JumpRangeCurr = 500,
            //        JumpRangeMax = 500
            //    };

            //    carrierInfos.Add(n);
            //}
        }

        List<CarrierInfo> carrierInfos = new();
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

            if (!subs.Contains(ev)) return;

            CarrierInfo? ci;

            switch (ev)
            {
                case "CarrierJumpRequest":
                    ci = carrierInfos.Find(x => x.CarrierID == root.CarrierID || x.Journal.Equals(eargs.Journal));

                    if (ci == null)
                    {
                        ci = new();
                        carrierInfos.Add(ci);
                    }

                    ci.Journal = eargs.Journal;
                    ci.Destination = root.SystemName;
                    ci.JumpETA = dto.LocalDateTime.AddMinutes(15).AddSeconds(14);
                    ci.CarrierID = root.CarrierID;
                    return;
                case "CarrierJump":
                    //Ci.SystemName = root.StarSystem;
                    return;
                case "CarrierStats":
                    ci = carrierInfos.Find(x => (x.CallSign != null && x.CallSign.Equals(root.Callsign)) || x.Journal.Equals(eargs.Journal));

                    if (ci == null)
                    {
                        ci = new();
                        carrierInfos.Add(ci);
                    }

                    ci.CallSign = root.Callsign;
                    ci.Journal = eargs.Journal;
                    ci.FuelLevel = (int)root.FuelLevel;
                    ci.JumpRangeCurr = (int)root.JumpRangeCurr;
                    ci.JumpRangeMax = (int)root.JumpRangeMax;
                    ci.CarrierID = root.CarrierID;
                    return;
                case "CarrierJumpCancelled":
                    ci = carrierInfos.Find(x => x.CarrierID == root.CarrierID || x.Journal.Equals(eargs.Journal));
                    
                    if (ci == null)
                    {
                        ci = new();
                        carrierInfos.Add(ci);
                    }

                    ci.JumpETA = DateTime.MinValue;
                    ci.CarrierID = root.CarrierID;
                    ci.Journal = eargs.Journal;
                    return;
                case "CarrierDepositFuel":
                    ci = carrierInfos.Find(x => x.CarrierID == root.CarrierID || x.Journal.Equals(eargs.Journal));

                    if (ci == null)
                    {
                        ci = new();
                        carrierInfos.Add(ci);
                    }

                    ci.FuelLevel = (int)root.Total;
                    ci.Journal = eargs.Journal;
                    ci.CarrierID = root.CarrierID;
                    return;
            }

            if (ev.Contains("carrier", StringComparison.OrdinalIgnoreCase))
                throw new Exception("carrier");
        }

        int countdownTick = 0;
        private void timerCountdown_Tick(object sender, EventArgs e)
        {
            countdownTick++;
            js.Tick();

            CarrierInfo? ci = null;

            if (carrierInfos.Count > 0)
            {
                while (true)
                {
                    ci = carrierInfos[CarrierIndex % carrierInfos.Count];
                    if (ci.CallSign != null)
                        break;
                    carrierInfos.Remove(ci);
                }
            }
            StringBuilder sb = new();

            if (ci != null && ci.JumpETA != DateTime.MinValue)
            {
                TimeSpan lockdown;
                TimeSpan jump;
                TimeSpan EtaArrival;
                TimeSpan cooldown;

                lockdown = GetTime(ci.JumpETA.AddMinutes(-3).AddSeconds(-20));
                jump = GetTime(ci.JumpETA);
                EtaArrival = GetTime(ci.JumpETA.AddSeconds(60));
                cooldown = GetTime(ci.JumpETA.AddMinutes(5).AddSeconds(-10), false);

                if (cooldown > new TimeSpan(0, 0, 0))
                {
                    sb.Append($"{lockdown:mm\\:ss} | {jump:mm\\:ss} | {EtaArrival:mm\\:ss} | {cooldown:mm\\:ss}\n");
                }
                else
                    sb.Append($"Idle: {new TimeSpan() - cooldown}\n");

                TimeSpan maxmin = new(0, 20, 0);

                if (countdownTick % 10 == 0)
                {

                    if (countdownTick % 20 == 0)
                        panel1.BackgroundImage?.Dispose();
                    if (countdownTick % 20 == 10)
                        panel2.BackgroundImage?.Dispose();

                    var width = this.Width;
                    var img = new Bitmap(width, 1);

                        string? stage = null;
                    for (int w = 0; w < width; w++)
                    {
                        double part = maxmin.TotalMilliseconds / width * w;

                        Color c;

                        Color lockInColor = settings.GetLockInColor(); // Color.DarkGreen
                        Color lockDownColor = settings.GetLockDownColor(); // Color.Green
                        Color jumpColor = settings.GetJumpColor(); // Color.Yellow
                        Color EtaArrivalColor = settings.GetEtaArrivalColor(); // Color.Red
                        Color cooldownColor = settings.GetCooldownColor(); // Color.DarkBlue
                        Color backgroundColor = settings.GetBackgroundColor();

                        if (jump.Add(new(0, -10, 0)).TotalMilliseconds > part)
                        {
                            c = lockInColor;
                        }
                        else if (lockdown.TotalMilliseconds > part)
                        {
                            c = lockDownColor;
                            stage = stage == null ? $"Lockdown {lockdown:mm\\:ss}" : stage;
                        }
                        else if (jump.TotalMilliseconds > part)
                        {
                            c = jumpColor;
                            stage = stage == null ? $"Traversal {jump:mm\\:ss}" : stage;
                        }
                        else if (EtaArrival.TotalMilliseconds > part)
                        {
                            c = EtaArrivalColor;
                            stage = stage == null ? $"Arriving {EtaArrival:mm\\:ss}" : stage;
                        }
                        else if (cooldown.TotalMilliseconds > part)
                        {
                            c = cooldownColor;
                            stage = stage == null ? $"Cooldown {cooldown:mm\\:ss}" : stage;
                        }
                        else
                        {
                            c = backgroundColor;
                            stage = stage == null ? $"Idle" : stage;
                        }

                        img.SetPixel(w, 0, c);
                    }

                    if (countdownTick % 20 == 0)
                    {
                        panel1.BackgroundImage = img;
                        panel2.BringToFront();
                        this.Text = stage;
                    }
                    if (countdownTick % 20 == 10)
                    {
                        panel2.BackgroundImage = img;
                        panel1.BringToFront();
                        this.Text = stage;
                    }
                }
            }
            else
                sb.Append($"--:-- | --:-- | --:-- | --:--\n");

            if (ci == null)
                ci = new()
                {
                    Destination = "---",
                    FuelLevel = 0,
                    JumpRangeCurr = 0,
                    CallSign = "---:---"
                };

            sb.Append($"{ci.Destination}\n{ci.FuelLevel}t | {ci.JumpRangeCurr}ly | {ci.CallSign}");

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