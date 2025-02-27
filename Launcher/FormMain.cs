using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Launcher
{
    public partial class FormMain : Form
    {
        static string path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        string sun = Path.Combine(path, "SUN.INI");
        List<string> number = new List<string>() { "One", "Two", "Three", "Four", "Five", "Six", "Seven" };
        List<List<ComboBox>> ai = new List<List<ComboBox>>();
        int mode = 4;
        int max = 0;

        public FormMain()
        {
            InitializeComponent();
            if (!Directory.Exists(Path.Combine(path, "Maps")))
            {
                MessageBox.Show("No Maps. Exit.");
                Application.Exit();
            }
            foreach (string line in Directory.GetFileSystemEntries(Path.Combine(path, "Maps"), "*.map", SearchOption.AllDirectories))
            {
                ListViewItem item = new ListViewItem();
                item.Text = lineReadWrite(line, "Basic", "Name");
                item.ToolTipText = line;
                item.SubItems.Add(lineReadWrite(line, "Waypoints", "", false, true));
                listView1.Items.Add(item);
            }
            string player = lineReadWrite(sun, "Launcher", "Player");
            if (!String.IsNullOrEmpty(player))
            {
                textBox1.Text = player;
            }
            comboBox1.SelectedIndex = 2;
            comboBox2.SelectedIndex = 8;
            comboBox3.SelectedIndex = 8;
            comboBox4.SelectedIndex = 0;
            ai.AddRange(new List<List<ComboBox>>() { new List<ComboBox>() { comboBox5, comboBox6, comboBox7, comboBox8, comboBox9 }, new List<ComboBox>() { comboBox10, comboBox11, comboBox12, comboBox13, comboBox14 }, new List<ComboBox>() { comboBox15, comboBox16, comboBox17, comboBox18, comboBox19 }, new List<ComboBox>() { comboBox20, comboBox21, comboBox22, comboBox23, comboBox24 }, new List<ComboBox>() { comboBox25, comboBox26, comboBox27, comboBox28, comboBox29 }, new List<ComboBox>() { comboBox30, comboBox31, comboBox32, comboBox33, comboBox34 }, new List<ComboBox>() { comboBox35, comboBox36, comboBox37, comboBox38, comboBox39 } });
            string map = lineReadWrite(sun, "Launcher", "LastMap");
            if (!String.IsNullOrEmpty(map))
            {
                ListViewItem findItem = listView1.FindItemWithText(map);
                if (findItem != null)
                {
                    listView1.Items[findItem.Index].Selected = true;
                    listView1.Select();
                }
            }
        }

        void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string[] size = lineReadWrite(listView1.SelectedItems[0].ToolTipText, "Preview", "Size").Replace(" ", "").Split(',');
                try
                {
                    pictureBox1.BackgroundImage = ExtractThumb(lineReadWrite(listView1.SelectedItems[0].ToolTipText, "PreviewPack", "", true), new int[] { Int32.Parse(size[0]), Int32.Parse(size[1]), Int32.Parse(size[2]), Int32.Parse(size[3]) });
                    max = Int32.Parse(listView1.SelectedItems[0].SubItems[1].Text);
                    max = mode == 1 && max > 2 ? 2 : mode == 2 && max > 4 ? 4 : mode == 3 && max > 6 ? 6 : max;
                    button2.Enabled = true;
                }
                catch
                {
                    max = 0;
                    button2.Enabled = false;
                    pictureBox1.BackgroundImage = null;
                    MessageBox.Show("Failed to read file: " + listView1.SelectedItems[0].ToolTipText);
                }
                for (int i = 6; i >= 0; i--)
                {
                    ai[i][0].Enabled = false;
                    ai[i][0].SelectedIndex = 0;
                    if (i <= max - 2)
                    {
                        ai[i][0].Enabled = true;
                        ai[i][0].SelectedIndex = 2;
                    }
                }
            }
        }

        void button1_Click(object sender, EventArgs e)
        {
            mode = mode < 4 ? mode + 1 : 1;
            button1.Text = mode == 1 ? "1 vs 1" : mode == 2 ? "2 vs 2" : mode == 3 ? "3 vs 3" : "4 vs 4";
            listView1_SelectedIndexChanged(this, new EventArgs());
        }

        void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                Random random = new Random();
                int side = comboBox1.SelectedIndex == 3 ? 0 : comboBox1.SelectedIndex == 2 ? random.Next(2) : comboBox1.SelectedIndex;
                List<string> cacheFile = new List<string>()
                {
                    "[Settings]",
                    "Name=" + textBox1.Text,
                    "Firestorm=" + (checkBox8.Checked ? "yes" : "no"),
                    "ShortGame=" + (checkBox6.Checked ? "yes" : "no"),
                    "MultiEngineer=" + (checkBox7.Checked ? "yes" : "no"),
                    "MCVRedeploy=" + (checkBox5.Checked ? "yes" : "no"),
                    "Crates=" + (checkBox2.Checked ? "yes" : "no"),
                    "Bases=" + (checkBox1.Checked ? "yes" : "no"),
                    "BridgeDestroy=" + (checkBox4.Checked ? "yes" : "no"),
                    "FogOfWar=" + (checkBox3.Checked ? "yes" : "no"),
                    "Side=" + side.ToString(),
                    "Scenario=spawnmap.ini",
                    "GameSpeed=" + (numericUpDown4.Maximum - numericUpDown4.Value).ToString(),
                    "Color=" + (comboBox2.SelectedIndex == 8 ? comboBox1.SelectedIndex == 3 ? 7 : side == 0 ? 0 : 1 : comboBox2.SelectedIndex).ToString(),
                    "Credits=" + numericUpDown2.Value.ToString(),
                    "TechLevel=" + numericUpDown3.Value.ToString(),
                    "UnitCount=" + numericUpDown1.Value.ToString()
                };
                int aicount = 0;
                List<string> handicaps = new List<string>() { "", "[HouseHandicaps]" };
                List<string> countries = new List<string>() { "", "[HouseCountries]" };
                List<string> colors = new List<string>() { "", "[HouseColors]" };
                List<string> locations = new List<string>() { "", "[SpawnLocations]" };
                if (comboBox1.SelectedIndex != 3 && comboBox3.SelectedIndex != 8)
                {
                    locations.Add("Multi1=" + comboBox3.SelectedIndex.ToString());
                }
                List<List<string>> alliances = new List<List<string>>() { new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>() };
                for (int i = 0; i < 7; i++)
                {
                    if (ai[i][0].SelectedIndex > 0)
                    {
                        aicount++;
                        handicaps.Add("Multi" + (aicount + 1).ToString() + "=" + (ai[i][0].SelectedIndex - 1).ToString());
                        if (ai[i][1].SelectedIndex != 2)
                        {
                            countries.Add("Multi" + (aicount + 1).ToString() + "=" + ai[i][1].SelectedIndex.ToString());
                        }
                        if (ai[i][2].SelectedIndex != 8)
                        {
                            colors.Add("Multi" + (aicount + 1).ToString() + "=" + ai[i][2].SelectedIndex.ToString());
                        }
                        if (ai[i][3].SelectedIndex != 8)
                        {
                            locations.Add("Multi" + (aicount + 1).ToString() + "=" + ai[i][3].SelectedIndex.ToString());
                        }
                        if (comboBox1.SelectedIndex != 3 && comboBox4.SelectedIndex < 4 && comboBox4.SelectedIndex == ai[i][4].SelectedIndex)
                        {
                            addToAlliances(alliances, 0, i + 1);
                        }
                        for (int j = 0; j < i; j++)
                        {
                            if (ai[i][4].SelectedIndex == ai[j][4].SelectedIndex)
                            {
                                addToAlliances(alliances, i + 1, j + 1);
                            }
                        }
                    }
                }
                cacheFile.Add("AIPlayers=" + aicount.ToString());
                if (handicaps.Count > 2)
                {
                    cacheFile.AddRange(handicaps);
                }
                if (countries.Count > 2)
                {
                    cacheFile.AddRange(countries);
                }
                if (colors.Count > 2)
                {
                    cacheFile.AddRange(colors);
                }
                if (locations.Count > 2)
                {
                    cacheFile.AddRange(locations);
                }
                for (int i = 0; i < 8; i++)
                {
                    if (alliances[i].Count > 0)
                    {
                        cacheFile.AddRange(alliances[i]);
                    }
                }
                if (comboBox1.SelectedIndex == 3)
                {
                    cacheFile.Add(Environment.NewLine + "[IsSpectator]" + Environment.NewLine + "Multi1=yes");
                }
                try
                {
                    File.Copy(listView1.SelectedItems[0].ToolTipText, Path.Combine(path, "spawnmap.ini"), true);
                    File.WriteAllLines(Path.Combine(path, "spawn.ini"), cacheFile);
                }
                catch
                {
                    MessageBox.Show("Failed to write: " + Path.Combine(path, "spawn.ini") + " or copy the file: " + listView1.SelectedItems[0].ToolTipText + " в " + Path.Combine(path, "spawnmap.ini"));
                }
                lineReadWrite(sun, "Launcher", "LastMap", false, false, true, listView1.SelectedItems[0].Text);
                lineReadWrite(sun, "Launcher", "Player", false, false, true, textBox1.Text);
                startGame(Path.Combine(path, "TS-Spawn.exe"), "-SPAWN");
            }
        }

        void addToAlliances(List<List<string>> alliances, int one, int two)
        {
            if (alliances[one].Count <= 0)
            {
                alliances[one].AddRange(new List<string>() { "", "[Multi" + (one + 1) + "_Alliances]", });
            }
            if (alliances[two].Count <= 0)
            {
                alliances[two].AddRange(new List<string>() { "", "[Multi" + (two + 1) + "_Alliances]", });
            }
            alliances[one].Add("HouseAlly" + number[alliances[one].Count - 2] + "=" + two);
            alliances[two].Add("HouseAlly" + number[alliances[two].Count - 2] + "=" + one);
        }

        void button3_Click(object sender, EventArgs e)
        {
            startGame(Path.Combine(path, "Game.exe"));
        }

        void startGame(string exe, string arg = null)
        {
            Process process = new Process();
            process.StartInfo.FileName = exe;
            process.StartInfo.WorkingDirectory = path;
            if (arg != null)
            {
                process.StartInfo.Arguments = arg;
            }
            try
            {
                process.Start();
            }
            catch
            {
                MessageBox.Show("Failed to start: " + process.StartInfo.FileName);
            }
            Application.Exit();
        }

        void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            aiComboBoxes(0);
        }

        void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            aiComboBoxes(1);
        }

        void comboBox15_SelectedIndexChanged(object sender, EventArgs e)
        {
            aiComboBoxes(2);
        }

        void comboBox20_SelectedIndexChanged(object sender, EventArgs e)
        {
            aiComboBoxes(3);
        }

        void comboBox25_SelectedIndexChanged(object sender, EventArgs e)
        {
            aiComboBoxes(4);
        }

        void comboBox30_SelectedIndexChanged(object sender, EventArgs e)
        {
            aiComboBoxes(5);
        }

        void comboBox35_SelectedIndexChanged(object sender, EventArgs e)
        {
            aiComboBoxes(6);
        }

        void aiComboBoxes(int index)
        {
            if (ai[index][0].SelectedIndex > 0)
            {
                if (!ai[index][1].Enabled)
                {
                    ai[index][1].Enabled = true;
                    ai[index][1].SelectedIndex = 2;
                }
                if (!ai[index][2].Enabled)
                {
                    ai[index][2].Enabled = true;
                    ai[index][2].SelectedIndex = 8;
                }
                if (!ai[index][3].Enabled)
                {
                    ai[index][3].Enabled = true;
                    ai[index][3].SelectedIndex = 8;
                }
                if (!ai[index][4].Enabled)
                {
                    ai[index][4].Enabled = true;
                    ai[index][4].SelectedIndex = ((float)max / (index + 1) <= 2) ? 1 : 0;
                }
            }
            else
            {
                ai[index][1].Enabled = false;
                ai[index][1].SelectedIndex = -1;
                ai[index][2].Enabled = false;
                ai[index][2].SelectedIndex = -1;
                ai[index][3].Enabled = false;
                ai[index][3].SelectedIndex = -1;
                ai[index][4].Enabled = false;
                ai[index][4].SelectedIndex = -1;
            }
        }

        string lineReadWrite(string file, string section, string key, bool range = false, bool players = false, bool write = false, string value = null)
        {
            List<string> cacheFile = new List<string>();
            try
            {
                cacheFile.AddRange(File.ReadAllLines(file));
            }
            catch
            {
                MessageBox.Show("Failed to read file: " + file);
            }
            bool find = false;
            bool success = false;
            string line = "";
            int index = 0;
            int pcount = 0;
            int temp = 0;
            int count = cacheFile.Count;
            for (int i = 0; i < count; i++)
            {
                if (!String.IsNullOrEmpty(cacheFile[i]))
                {
                    if (cacheFile[i].StartsWith("[" + section + "]", StringComparison.OrdinalIgnoreCase))
                    {
                        find = true;
                    }
                    else if (find && cacheFile[i].StartsWith("["))
                    {
                        if (write)
                        {
                            success = true;
                            cacheFile.Insert(index, key + "=" + value);
                        }
                        break;
                    }
                    else if (find && (range || players || cacheFile[i].StartsWith(key + "=", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (write)
                        {
                            success = true;
                            cacheFile[i] = key + "=" + value;
                            break;
                        }
                        else
                        {
                            if (range)
                            {
                                line += cacheFile[i].Remove(0, cacheFile[i].IndexOf("=") + 1);
                            }
                            else if (players && Int32.TryParse(cacheFile[i].Remove(cacheFile[i].IndexOf("=")), out temp))
                            {
                                if (pcount + 1 > temp)
                                {
                                    pcount++;
                                }
                                else
                                {
                                    cacheFile.Clear();
                                    return pcount.ToString();
                                }
                            }
                            else
                            {
                                line = cacheFile[i].Remove(0, cacheFile[i].IndexOf("=") + 1);
                                cacheFile.Clear();
                                return line;
                            }
                        }
                    }
                    index = i;
                }
            }
            if (write)
            {
                if (!find)
                {
                    cacheFile.Insert(cacheFile.Count > 0 ? index + 1 : 0, (cacheFile.Count > 0 ? Environment.NewLine : "") + "[" + section + "]" + Environment.NewLine + key + "=" + value);
                }
                else if (!success)
                {
                    cacheFile.Insert(index + 1, key + "=" + value);
                }
                try
                {
                    File.WriteAllLines(file, cacheFile);
                }
                catch
                {
                    MessageBox.Show("Failed to write file: " + file);
                }
            }
            cacheFile.Clear();
            if (range)
            {
                return line;
            }
            else
            {
                return "";
            }
        }

        unsafe Bitmap ExtractThumb(string previewPack, int[] size)
        {
            Rectangle previewSize = new Rectangle(size[0], size[1], size[2], size[3]);
            Bitmap preview = new Bitmap(previewSize.Width, previewSize.Height, PixelFormat.Format24bppRgb);
            byte[] image = new byte[preview.Width * preview.Height * 3];
            byte[] image_compressed = Convert.FromBase64String(previewPack);
            DecodeInto(image_compressed, image);
            BitmapData bmd = preview.LockBits(new Rectangle(0, 0, preview.Width, preview.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            int idx = 0;
            for (int y = 0; y < bmd.Height; y++)
            {
                byte* row = (byte*)bmd.Scan0 + bmd.Stride * y;
                byte* p = row;
                for (int x = 0; x < bmd.Width; x++)
                {
                    byte b = image[idx++];
                    byte g = image[idx++];
                    byte r = image[idx++];
                    *p++ = r;
                    *p++ = g;
                    *p++ = b;
                }
            }
            preview.UnlockBits(bmd);
            image = null;
            image_compressed = null;
            return preview;
        }
        unsafe uint DecodeInto(byte[] src, byte[] dest)
        {
            fixed (byte* pr = src, pw = dest)
            {
                byte* r = pr, w = pw;
                byte* w_end = w + dest.Length;
                while (w < w_end)
                {
                    ushort size_in = *(ushort*)r;
                    r += 2;
                    uint size_out = *(ushort*)r;
                    r += 2;
                    if (size_in == 0 || size_out == 0)
                    {
                        break;
                    }
                    Decompress(r, size_in, w, ref size_out);
                    r += size_in;
                    w += size_out;
                }
                return (uint)(w - pw);
            }
        }
        unsafe void Decompress(byte* r, uint size_in, byte* w, ref uint size_out)
        {
            fixed (byte* wrkmem = new byte[IntPtr.Size * 16384])
            {
                lzo1x_decompress(r, size_in, w, ref size_out, wrkmem);
            }
        }
        unsafe int lzo1x_decompress(byte* @in, uint in_len, byte* @out, ref uint out_len, void* wrkmem)
        {
            byte* op;
            byte* ip;
            uint t;
            byte* m_pos;
            byte* ip_end = @in + in_len;
            out_len = 0;
            op = @out;
            ip = @in;
            bool gt_first_literal_run = false;
            bool gt_match_done = false;
            if (*ip > 17)
            {
                t = (uint)(*ip++ - 17);
                if (t < 4)
                {
                    match_next(ref op, ref ip, ref t);
                }
                else
                {
                    do
                    {
                        *op++ = *ip++;
                    } while (--t > 0);
                    gt_first_literal_run = true;
                }
            }
            while (true)
            {
                if (gt_first_literal_run)
                {
                    gt_first_literal_run = false;
                    goto first_literal_run;
                }
                t = *ip++;
                if (t >= 16)
                {
                    goto match;
                }
                if (t == 0)
                {
                    while (*ip == 0)
                    {
                        t += 255;
                        ip++;
                    }
                    t += (uint)(15 + *ip++);
                }
                *(uint*)op = *(uint*)ip;
                op += 4;
                ip += 4;
                if (--t > 0)
                {
                    if (t >= 4)
                    {
                        do
                        {
                            *(uint*)op = *(uint*)ip;
                            op += 4;
                            ip += 4;
                            t -= 4;
                        } while (t >= 4);
                        if (t > 0)
                        {
                            do
                            {
                                *op++ = *ip++;
                            } while (--t > 0);
                        }
                    }
                    else
                    {
                        do
                        {
                            *op++ = *ip++;
                        } while (--t > 0);
                    }
                }
            first_literal_run:
                t = *ip++;
                if (t >= 16)
                {
                    goto match;
                }
                m_pos = op - (1 + 0x0800);
                m_pos -= t >> 2;
                m_pos -= *ip++ << 2;
                *op++ = *m_pos++;
                *op++ = *m_pos++;
                *op++ = *m_pos;
                gt_match_done = true;
            match:
                do
                {
                    if (gt_match_done)
                    {
                        gt_match_done = false;
                        goto match_done;
                    }
                    if (t >= 64)
                    {
                        m_pos = op - 1;
                        m_pos -= (t >> 2) & 7;
                        m_pos -= *ip++ << 3;
                        t = (t >> 5) - 1;
                        copy_match(ref op, ref m_pos, ref t);
                        goto match_done;
                    }
                    else if (t >= 32)
                    {
                        t &= 31;
                        if (t == 0)
                        {
                            while (*ip == 0)
                            {
                                t += 255;
                                ip++;
                            }
                            t += (uint)(31 + *ip++);
                        }
                        m_pos = op - 1;
                        m_pos -= (*(ushort*)(void*)(ip)) >> 2;
                        ip += 2;
                    }
                    else if (t >= 16)
                    {
                        m_pos = op;
                        m_pos -= (t & 8) << 11;
                        t &= 7;
                        if (t == 0)
                        {
                            while (*ip == 0)
                            {
                                t += 255;
                                ip++;
                            }
                            t += (uint)(7 + *ip++);
                        }
                        m_pos -= (*(ushort*)ip) >> 2;
                        ip += 2;
                        if (m_pos == op)
                        {
                            goto eof_found;
                        }
                        m_pos -= 0x4000;
                    }
                    else
                    {
                        m_pos = op - 1;
                        m_pos -= t >> 2;
                        m_pos -= *ip++ << 2;
                        *op++ = *m_pos++;
                        *op++ = *m_pos;
                        goto match_done;
                    }
                    if (t >= 2 * 4 - (3 - 1) && (op - m_pos) >= 4)
                    {
                        *(uint*)op = *(uint*)m_pos;
                        op += 4;
                        m_pos += 4;
                        t -= 4 - (3 - 1);
                        do
                        {
                            *(uint*)op = *(uint*)m_pos;
                            op += 4;
                            m_pos += 4;
                            t -= 4;
                        } while (t >= 4);
                        if (t > 0)
                        {
                            do
                            {
                                *op++ = *m_pos++;
                            } while (--t > 0);
                        }
                    }
                    else
                    {
                        *op++ = *m_pos++;
                        *op++ = *m_pos++;
                        do
                        {
                            *op++ = *m_pos++;
                        } while (--t > 0);
                    }
                match_done:
                    t = (uint)(ip[-2] & 3);
                    if (t == 0)
                    {
                        break;
                    }
                    *op++ = *ip++;
                    if (t > 1)
                    {
                        *op++ = *ip++;
                        if (t > 2)
                        {
                            *op++ = *ip++;
                        }
                    }
                    t = *ip++;
                } while (true);
            }
        eof_found:
            out_len = ((uint)((op) - (@out)));
            return (ip == ip_end ? 0 : (ip < ip_end ? (-8) : (-4)));
        }
        unsafe void match_next(ref byte* op, ref byte* ip, ref uint t)
        {
            do
            {
                *op++ = *ip++;
            } while (--t > 0);
            t = *ip++;
        }
        unsafe void copy_match(ref byte* op, ref byte* m_pos, ref uint t)
        {
            *op++ = *m_pos++;
            *op++ = *m_pos++;
            do
            {
                *op++ = *m_pos++;
            } while (--t > 0);
        }
    }
}
