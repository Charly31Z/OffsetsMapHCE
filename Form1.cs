using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OffsetsHalo
{
    public partial class Form1 : Form
    {
        struct header_map
        {
            public UInt32 magic_head;
            public UInt32 cache_version;
            public UInt32 file_size;
            public UInt32 padding_lenght;
            public UInt32 tag_data_offset;
            public UInt32 tag_data_size;
            public char[] scenario_name;
            public char[] build_version;
            public UInt16 scenario_type;
            public UInt32 checksum;
            public UInt32 magic_foot;
        }

        struct header_tags
        {
            public UInt32 tag_array_pointer;
            public UInt32 checksum;
            public UInt32 scenario_id;
            public UInt32 tag_count;
            public UInt32 model_part_count;
        }

        struct new_header_tags
        {
            public UInt32 model_data_file_offset;
            public UInt32 model_part_count;
            public UInt32 vertex_data_size;
            public UInt32 model_data_size;
            public UInt32 magic;
        }

        struct tag
        {
            public UInt32 primary_fourCC;
            public UInt32 secondary_fourCC;
            public UInt32 tertiary_fourCC;
            public UInt32 tag_id;
            public UInt32 tag_path_pointer;
            public UInt32 tag_data_pointer;
            public UInt32 resource_index;
            public UInt32 external;
        }

        private header_map header;

        private header_tags tag_header;

        private tag[] tags;

        public Form1()
        {
            InitializeComponent();
        }

        void readHeaderFile(Stream fileStream)
        {
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                header.magic_head = reader.ReadUInt32();
                header.cache_version = reader.ReadUInt32();
                header.file_size = reader.ReadUInt32();
                header.padding_lenght = reader.ReadUInt32();
                header.tag_data_offset = reader.ReadUInt32();
                header.tag_data_size = reader.ReadUInt32();

                header.scenario_name = reader.ReadChars(32);
                header.build_version = reader.ReadChars(32);

                fileStream.Seek(0x0060, SeekOrigin.Begin);
                header.scenario_type = reader.ReadUInt16();

                fileStream.Seek(0x0064, SeekOrigin.Begin);
                header.checksum = reader.ReadUInt32();

                fileStream.Seek(0x07FC, SeekOrigin.Begin);
                header.magic_foot = reader.ReadUInt32();

                string scen = new string(header.scenario_name).Replace("\0", "");
                string bV = new string(header.build_version).Replace("\0", "");

                string hexChecksum = header.checksum.ToString("X");


                string crc32 = string.Empty;

                if (hexChecksum.Length <= 7)
                {
                    crc32 = "0x0" + hexChecksum;
                }
                else
                {
                    crc32 = "0x" + hexChecksum;
                }

                richTextBox1.Text = string.Empty;

                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("Magic Head: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.AppendText(header.magic_head.ToString());
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("\nCache: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.AppendText(header.cache_version.ToString());
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("\nFile Size: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.AppendText(header.file_size.ToString());
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("\nPadding Lenght: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.AppendText(header.padding_lenght.ToString());
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("\nTag Data Offset: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.AppendText(header.tag_data_offset.ToString());
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("\nTag Data Size: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.AppendText(header.tag_data_size.ToString());


                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("\n\nScenario name: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.AppendText(scen.ToString());
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("\nBuild Version: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.AppendText(bV.ToString());


                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("\n\nScenario Type: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.AppendText(header.scenario_type.ToString());
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("\nChecksum: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.AppendText(header.checksum.ToString());
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("\nCRC32: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.AppendText(crc32);
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("\nMagic Foot: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.AppendText(header.magic_foot.ToString());

                reader.Close();
            }
        }

        void readTagHeader(Stream fileStream)
        {
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                fileStream.Seek(header.tag_data_offset, SeekOrigin.Begin);
                tag_header.tag_array_pointer = reader.ReadUInt32();
                tag_header.checksum = reader.ReadUInt32();
                tag_header.scenario_id = reader.ReadUInt32();
                tag_header.tag_count = reader.ReadUInt32();
                tag_header.model_part_count = reader.ReadUInt32();

                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("\n\nTag Array Pointer: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.AppendText(tag_header.tag_array_pointer.ToString());
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("\nTag Checksum: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.AppendText(tag_header.checksum.ToString());
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("\nTag Scenario ID: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.AppendText(tag_header.scenario_id.ToString());
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("\nTag Count: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.AppendText(tag_header.tag_count.ToString());
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("\nTag Model Part Count: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
                richTextBox1.AppendText(tag_header.model_part_count.ToString());

                reader.Close();
            }
            fileStream.Close();
        }

        void readTags(Stream fileStream)
        {
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.AppendText("\n\nTags: ");
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);

                tags = new tag[tag_header.tag_count];

                fileStream.Seek(tag_header.tag_array_pointer, SeekOrigin.Begin);
                for(int i = 0; i < tag_header.tag_count; i++)
                {
                    tags[i].primary_fourCC = reader.ReadUInt32();
                    tags[i].secondary_fourCC = reader.ReadUInt32();
                    tags[i].tertiary_fourCC = reader.ReadUInt32();
                    tags[i].tag_id = reader.ReadUInt32();
                    tags[i].tag_path_pointer = reader.ReadUInt32();
                    tags[i].tag_data_pointer = reader.ReadUInt32();
                    //tags[i].resource_index = reader.ReadUInt32();
                    tags[i].external = reader.ReadUInt32();

                    fileStream.Seek(tags[i].tag_path_pointer, SeekOrigin.Begin);
                    string path = reader.ReadString();

                    richTextBox1.AppendText("\n"+path);
                }

                reader.Close();
            }
            fileStream.Close();
        }

        void read(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();

            string fileName = openFileDialog1.FileName;

            var fileStream = openFileDialog1.OpenFile();

            readHeaderFile(fileStream);

            fileStream = openFileDialog1.OpenFile();
            readTagHeader(fileStream);

            fileStream = openFileDialog1.OpenFile();
            readTags(fileStream);
        }
    }
}
