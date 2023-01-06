using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdCarrierCountdown
{
    internal class Settings
    {
        public CooldownColor CooldownColor = new();
        internal Color GetCooldownColor()
        {
            return Color.FromArgb(CooldownColor.R, CooldownColor.G, CooldownColor.B);
        }

        public EtaArrivalColor EtaArrivalColor = new();
        internal Color GetEtaArrivalColor()
        {
            return Color.FromArgb(EtaArrivalColor.R, EtaArrivalColor.G, EtaArrivalColor.B);
        }

        public JumpColor JumpColor = new();
        internal Color GetJumpColor()
        {
            return Color.FromArgb(JumpColor.R, JumpColor.G, JumpColor.B);
        }

        public LockDownColor LockDownColor = new();
        internal Color GetLockDownColor()
        {
            return Color.FromArgb(LockDownColor.R, LockDownColor.G, LockDownColor.B);
        }

        public LockInColor LockInColor = new();
        internal Color GetLockInColor()
        {
            return Color.FromArgb(LockInColor.R, LockInColor.G, LockInColor.B);
        }

        public BackgroundColor BackgroundColor = new();
        internal Color GetBackgroundColor()
        {
            return Color.FromArgb(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B);
        }
    }

    public class BackgroundColor
    {
        public byte R = Color.LightGray.R;
        public byte G = Color.LightGray.G;
        public byte B = Color.LightGray.B;
    }

    public class CooldownColor
    {
        public byte R = Color.DarkBlue.R;
        public byte G = Color.DarkBlue.G;
        public byte B = Color.DarkBlue.B;
    }

    public class EtaArrivalColor
    {
        public byte R = Color.Red.R;
        public byte G = Color.Red.G;
        public byte B = Color.Red.B;
    }

    public class JumpColor
    {
        public byte R = Color.Yellow.R;
        public byte G = Color.Yellow.G;
        public byte B = Color.Yellow.B;
    }

    public class LockDownColor
    {
        public byte R = Color.Green.R;
        public byte G = Color.Green.G;
        public byte B = Color.Green.B;
    }

    public class LockInColor
    {
        public byte R = Color.DarkGreen.R;
        public byte G = Color.DarkGreen.G;
        public byte B = Color.DarkGreen.B;
    }
}
