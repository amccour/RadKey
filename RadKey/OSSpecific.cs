using MovablePython;
using System.Windows.Forms;

namespace RadKey
{
    partial class RadKey
    {
        private static class OSSpecific
        {
            public static class Windows
            {
                public static Hotkey CreateRestoreHotkey(RadKey radKey)
                {
                    // Hotkey section.
                    Hotkey toRegister = new Hotkey(Keys.Back, true, true, false, false);
                    toRegister.Pressed += delegate
                    {
                        if (radKey.WindowState == FormWindowState.Normal)
                        {
                            radKey.WindowState = FormWindowState.Minimized;
                            radKey.Visible = false;
                        }
                        else if (radKey.WindowState == FormWindowState.Minimized)
                        {
                            // Need to make it visible first, otherwise it ignores the WindowState change.
                            radKey.Visible = true;
                            radKey.WindowState = FormWindowState.Normal;
                        }
                    };

                    if (!toRegister.GetCanRegister(radKey))
                    {
                        // TODO
                    }
                    else
                    {
                        toRegister.Register(radKey);
                    }

                    return toRegister;
                }

                public static void InitializeComponent(RadKey radKey)
                {
                    // Tray section.
                    radKey.ShowInTaskbar = false;

                    // Duplicates the component registration code from Form1.Designer since it gets mad if I try to modify that file directly.
                    if (radKey.components == null)
                    {
                        radKey.components = new System.ComponentModel.Container();
                    }

                    radKey.RadKeyNotifyIcon = new System.Windows.Forms.NotifyIcon(radKey.components);

                    radKey.RadKeyNotifyIcon.BalloonTipText = "RadKey";
                    radKey.RadKeyNotifyIcon.Icon = ((System.Drawing.Icon)(GlobeIcon.ResourceManager.GetObject("globe")));
                    radKey.RadKeyNotifyIcon.Text = "RadKey";
                    radKey.RadKeyNotifyIcon.Visible = true;
                    radKey.RadKeyNotifyIcon.Click += new System.EventHandler(radKey.RKNIClick);
                }
            }

            public static class Linux
            {
                public static void InitializeComponent(RadKey radKey)
                {
                    radKey.messageBox.Font = new System.Drawing.Font("MS Gothic", (float)9);
                    radKey.meaningBox.Font = new System.Drawing.Font("MS Gothic", (float)8.5);
                    radKey.readingBox.Font = new System.Drawing.Font("MS Gothic", (float)11.5);
                }
            }
        }
    }
}
