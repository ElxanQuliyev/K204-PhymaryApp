using PhymarcyAppK204.Models;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PhymarcyAppK204.AllForms
{
    public partial class AddMedicineForm : Form
    {
        PhymarcyDBEntities db = new PhymarcyDBEntities();
        public AddMedicineForm()
        {
            InitializeComponent();
        }

        public void FillFirmsCombo()
        {
            cmbFirms.Items.AddRange(db.Firms.Select(x => x.Name).ToArray());
        }

        public void FillTagCombo()
        {
            cmbTags.Items.AddRange(db.Tags.Select(x => x.Name).ToArray());
        }

        public void FillDataGridMedicine()
        {
            dtgAddMedicine.DataSource = db.Medicines.Select(md => new
            {
                Medicine_Name = md.Name,
                md.Barcode,
                md.Quantity,
                md.Price,
                FirmName = md.Medicine_To_Firms.FirstOrDefault(mdf => mdf.MedicineId == md.Id).Firm.Name,
                tg = db.Medicines_To_Tags.Where(x => x.MedicineID == md.Id).Select(x => x.Tag.Name ).AsEnumerable().ToArray(), 
                IsReceipt = md.WithReceipt ? "Reseptli" : "Reseptsiz",
                md.ProductionDate,
                md.ExperienceDate
            }).ToList();
            for (var i = 0; i < dtgAddMedicine.Rows.Count; i++)
            {
                DateTime dtEx = (DateTime)dtgAddMedicine.Rows[i].Cells[dtgAddMedicine.Columns.Count - 1].Value;
                if (dtEx < DateTime.Now)
                {
                    dtgAddMedicine.Rows[i].DefaultCellStyle.BackColor = Color.DarkRed;
                    dtgAddMedicine.Rows[i].DefaultCellStyle.ForeColor = Color.White;
                }
            }
        }
        private void AddMedicineForm_Load(object sender, EventArgs e)
        {
            FillFirmsCombo();
            FillTagCombo();
            FillDataGridMedicine();
        }

        public int HaveFirms(string frm)
        {
            Firm hasFirm = db.Firms.FirstOrDefault(x => x.Name == frm);
            if (hasFirm == null)
            {
                Firm newFirm = new Firm()
                {
                    Name = frm
                };
                db.Firms.Add(newFirm);
                db.SaveChanges();
                return newFirm.ID;
            }
            return hasFirm.ID;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string firmName = cmbFirms.Text;
            int firmId = HaveFirms(firmName);
            string medname = txtMedName.Text;
            string barcode = txtBarcode.Text;
            decimal price = nmPrice.Value;
            string count = txtCount.Text;
            DateTime proDate = dtgProDate.Value;
            DateTime exDate = dtgExDate.Value;
            string desc = rcDesc.Text;
            string[] myArr = new string[] { firmName, medname, barcode, count, desc };
            if (Extensions.IsEmpty(myArr, string.Empty))
            {
                if (exDate >= DateTime.Now)
                {
                    Medicine newMed = new Medicine()
                    {
                        Name = medname,
                        Barcode = Convert.ToInt32(barcode),
                        Price = price,
                        Description = desc,
                        ProductionDate = proDate,
                        ExperienceDate = exDate,
                        Quantity = Convert.ToInt32(count),
                        WithReceipt = ckIsReceipt.Checked ? true : false
                    };
                    db.Medicines.Add(newMed);
                    db.SaveChanges();
                    db.Medicine_To_Firms.Add(new Medicine_To_Firms()
                    {
                        MedicineId = newMed.Id,
                        FirmID = firmId
                    });
                    db.SaveChanges();
                    AddMedWithTag(newMed.Id);
                    MessageBox.Show($"{medname} added successfully", "success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearAllData();
                }
            }
            else
            {
                lblError.Text = "Please,Fill all the field";
                lblError.Visible = true;
            }

        }
        public void ClearAllData()
        {
            foreach (var con in this.Controls)
            {
                if (con is TextBox)
                {
                    TextBox txt = (TextBox)con;
                    txt.Text = "";
                }
                else if (con is ComboBox)
                {
                    ComboBox cmb = (ComboBox)con;
                    cmb.Text = "";
                }
                else if (con is NumericUpDown)
                {
                    NumericUpDown num = (NumericUpDown)con;
                    num.Value = 0;
                }
                else if (con is RichTextBox)
                {
                    RichTextBox rich = (RichTextBox)con;
                    rich.Text = "";
                }
                else if (con is CheckedListBox)
                {
                    CheckedListBox chk = (CheckedListBox)con;
                    chk.Items.Clear();
                }
                else if (con is DateTimePicker)
                {
                    DateTimePicker dtg = (DateTimePicker)con;
                    dtg.Value = DateTime.Now;
                }
                else if (con is CheckBox)
                {
                    CheckBox ck = (CheckBox)con;
                    ck.Checked = false;
                }

            }
        }

        private void cmbTags_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                string tagName = cmbTags.Text;
                if (!checkedTagList.Items.Contains(tagName))
                {
                    checkedTagList.Items.Add(tagName, true);
                }
                cmbTags.Text = "";
            }
        }

        public void AddMedWithTag(int medId)
        {
            for (int i = checkedTagList.Items.Count - 1; i >= 0; i--)
            {
                string tagname = checkedTagList.Items[i].ToString();
                Tag selectedTag = db.Tags.FirstOrDefault(tg => tg.Name == tagname);
                int tagId;
                if (selectedTag == null)
                {
                    Tag newTag = new Tag()
                    {
                        Name = tagname
                    };
                    db.Tags.Add(newTag);
                    db.SaveChanges();
                    tagId = newTag.ID;
                }
                else
                {
                    tagId = selectedTag.ID;
                }
                db.Medicines_To_Tags.Add(new Medicines_To_Tags()
                {
                    MedicineID = medId,
                    TagID = tagId
                });
                db.SaveChanges();
            }
        }

        private void checkedTagList_SelectedIndexChanged(object sender, EventArgs e)
        {

            int selected = checkedTagList.SelectedIndex;
            if (selected != -1)
            {
                checkedTagList.Items.RemoveAt(selected);
            }
        }

        private void textOnlyNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (txt.Name == "txtCount")
            {
                if ((e.KeyChar < 48 || e.KeyChar > 57 || txtCount.Text.Length >= 5) && e.KeyChar != 8)
                {
                    e.Handled = true;
                }
            }
            else
            {
                if ((e.KeyChar < 48 || e.KeyChar > 57 || txtBarcode.Text.Length >= 8) && e.KeyChar != 8)
                {
                    e.Handled = true;
                }
            }
        }
    }
}
