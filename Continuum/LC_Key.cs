﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ContinuumNS
{
    public partial class LC_Key : Form
    {
        public LC_Key(Continuum continuum)
        {
            InitializeComponent();
            thisInst = continuum;
        }

        public Continuum thisInst;
        public TopoInfo.LC_SR_DH[] LC_Key_Orig;   // Original (i.e unaltered) land cover key
        public TopoInfo.LC_SR_DH[] LC_Key_New; // new (modified) land cover key

        private void btnModKey_Click(object sender, EventArgs e)
        {

            // Opens the Mod_LC_Key form with the selected land cover code, description, SR, and DH for the user to edit. 

            if (lstLC_SR_DH.SelectedItems.Count == 0) {
                MessageBox.Show("Select a land cover code to modify.", "Continuum 3");
                return;
            }

            TopoInfo.LC_SR_DH thisLC = new TopoInfo.LC_SR_DH();

            thisLC.code = Convert.ToInt16(lstLC_SR_DH.SelectedItems[0].Text);
            thisLC.desc = lstLC_SR_DH.SelectedItems[0].SubItems[1].Text;
            thisLC.SR = Convert.ToSingle(lstLC_SR_DH.SelectedItems[0].SubItems[2].Text);
            thisLC.DH = Convert.ToSingle(lstLC_SR_DH.SelectedItems[0].SubItems[3].Text);

            Mod_LC_Key thisMod = new Mod_LC_Key(thisInst, this);

            thisMod.txtCode.Text = thisLC.code.ToString();
            thisMod.txtDesc.Text = thisLC.desc;
            thisMod.txtSR.Text = Math.Round(thisLC.SR,3).ToString();
            thisMod.txtDH.Text = Math.Round(thisLC.DH, 1).ToString();

            if (cboLC_Key.SelectedIndex == 0)
                thisInst.topo.SetUS_NLCD_Key();
            else if (cboLC_Key.SelectedIndex == 1)
                thisInst.topo.SetNA_LC_Key();
            else if (cboLC_Key.SelectedIndex == 2)
                thisInst.topo.SetEU_Corine_LC_Key();

            thisMod.ShowDialog();
            LC_Key_New = thisMod.thisLC_Key.LC_Key_New;
        }

        private void btnNewKey_Click(object sender, EventArgs e)
        {
            // Imports new (user-defined) land cover key and updates the land cover key table on form
            if (thisInst.ofdLC_Key.ShowDialog() == DialogResult.OK) {
                              
                string wholePath = thisInst.ofdLC_Key.FileName;
                int ind = wholePath.LastIndexOf('\\');

                StreamReader sr = new StreamReader(wholePath);
                
                // MyReader.SetDelimiters(",", vbTab)
                //  MyReader.TrimWhiteSpace = true

                string[] fileRow;
                TopoInfo.LC_SR_DH[] newLC_Key = null;
                int LC_Count = 0;

                while (sr.EndOfStream == false)
                {
                    var dataStr = sr.ReadLine();
                    fileRow = dataStr.Split(',');

                    if (fileRow.Length == 4)
                    {
                        Array.Resize(ref newLC_Key, LC_Count + 1);
                        newLC_Key[LC_Count] = new TopoInfo.LC_SR_DH();

                        try {
                            newLC_Key[LC_Count].code = Convert.ToInt16(fileRow[0]);
                            newLC_Key[LC_Count].desc = fileRow[1];
                            newLC_Key[LC_Count].SR = Convert.ToSingle(fileRow[2]);
                            newLC_Key[LC_Count].DH = Convert.ToSingle(fileRow[3]);
                        }
                        catch {
                            MessageBox.Show("Error reading land cover key. Format of file should be: Code, Description, Surface roughness, Displacement height.", "Continuum 3");
                            sr.Close();
                            return;
                        }

                        LC_Count++;
                    }
                    else {
                        MessageBox.Show("Error reading in the land cover key at line: " + LC_Count + "The format should be Code, Description, Surface roughness, Displacement height.", "Continuum 3");
                        sr.Close();
                        return;
                    }
                }

                sr.Close();

                LC_Key_New = newLC_Key;
                thisInst.Populate_LC_Key_Form(this);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Resets the land covery key to the original (unmodified) key and closes form
            thisInst.topo.LC_Key = LC_Key_Orig;
            Close();
        }

        private void cboLC_Key_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Updates the land cover key table based on key selected from dropdown
            Update_LC_table();
        }

        public void Update_LC_table()
        {
            // Clears and populates land cover key table based on key selected from dropdown
            string thisKey = "";
            ListViewItem objListItem = new ListViewItem();
            
            lstLC_SR_DH.Items.Clear();

            try {
                thisKey = cboLC_Key.SelectedItem.ToString();
            }
            catch {
                return;
            }

            TopoInfo topo = new TopoInfo();

            if (thisKey == "US NLCD")
            {
                topo.SetUS_NLCD_Key();
                LC_Key_New = topo.LC_Key;
            }
            else if (thisKey == "North America NALCMS")
            {
                topo.SetNA_LC_Key();
                LC_Key_New = topo.LC_Key;
            }
            else if (thisKey == "EU Corine 2006 LC")
            {
                topo.SetEU_Corine_LC_Key();
                LC_Key_New = topo.LC_Key;
            }
            else if (thisKey == "User-Defined")
            {
                bool isNLCD = topo.LC_IsDefaultNLCD(LC_Key_New);
                bool Is_NALCD = topo.LC_IsDefaultNALC(LC_Key_New);
                bool Is_EULCD = topo.LC_IsDefaultEU_Corine(LC_Key_New);

                if (isNLCD == true)
                {
                    cboLC_Key.SelectedIndex = 0;
                    return;
                }
                else if (Is_NALCD == true)
                {
                    cboLC_Key.SelectedIndex = 1;
                    return;
                }
                else if (Is_EULCD == true)
                {
                    cboLC_Key.SelectedIndex = 2;
                    return;
                }
            }

            int numSR = LC_Key_New.Length;
            for (int i = 0; i < numSR; i++)
            {
                objListItem = lstLC_SR_DH.Items.Add(LC_Key_New[i].code.ToString());
                objListItem.SubItems.Add(LC_Key_New[i].desc);
                objListItem.SubItems.Add(Math.Round(LC_Key_New[i].SR, 2).ToString());
                objListItem.SubItems.Add(Math.Round(LC_Key_New[i].DH, 2).ToString());
            }
            
        }

        private void btnOK_Click(object sender, EventArgs e)
        {

            // if the LC key was changed and calcs have been done already then clears UWDW + Round Robin models, turbine WS/AEP ests gross/net, and all 
            // maps and recalculates SR/DH at Mets, Turbines, Nodes in DB 
            NodeCollection nodeList = new NodeCollection();
            Update updateThe = new Update();

            if (Orig_and_New_LC_Same() == false ) {
                DialogResult Good_to_go = DialogResult.Yes;

                if (thisInst.modelList.ModelCount > 1 || thisInst.mapList.ThisCount > 0) {
                    Good_to_go = MessageBox.Show("Changing the Land Cover key will reset the analysis file and delete all generated estimates and maps. Do you want to continue?", 
                        "Continuum 3", MessageBoxButtons.YesNo);
                }

                if (Good_to_go == DialogResult.Yes)
                {                   
                    thisInst.modelList.ClearAllExceptImported();
                    thisInst.mapList.ClearAllMaps();
                    thisInst.turbineList.ClearAllWSEsts();
                    thisInst.turbineList.ClearAllGrossEsts();
                    thisInst.turbineList.ClearAllNetEsts();
                    thisInst.turbineList.turbineCalcsDone = false;

                    thisInst.metPairList.ClearAll();
                    thisInst.metPairList.ClearRoundRobin();

                    thisInst.topo.LC_Key = LC_Key_New;
                    thisInst.topo.GetElevsAndSRDH_ForCalcs(thisInst, null, false);

                    if (thisInst.topo.gotSR == true)
                    {
                        thisInst.metList.ReCalcSRDH(thisInst.topo, thisInst.radiiList);
                        thisInst.turbineList.ReCalcTurbine_SRDH(thisInst);
                        
                        // Call Background worker to recalculate node SR/DH
                        thisInst.BW_worker = new BackgroundWork();
                        thisInst.BW_worker.Call_BW_Node_Recalc(thisInst);
                    }
                    
                }                
                
                else
                    thisInst.topo.LC_Key = LC_Key_Orig;
            }

            updateThe.AllTABs(thisInst);
            
            Close();
        }

        public bool Orig_and_New_LC_Same()
        {
            // Returns true if LC_Key_Orig and LC_Key_New are the same
            bool sameKey = true;

            try {

                if (LC_Key_Orig.Length != LC_Key_New.Length)
                    sameKey = false;
                else {
                    for (int i = 0; i <= LC_Key_Orig.Length - 1; i++) {
                        if (LC_Key_Orig[i].SR != LC_Key_New[i].SR || LC_Key_Orig[i].DH != LC_Key_New[i].DH || LC_Key_Orig[i].code != LC_Key_New[i].code) {
                            sameKey = false;
                            break;
                        }
                    }
                }
            }
            catch {
                sameKey = false;
            }

            return sameKey;
        }
        
        public void Read_Orig_and_New(TopoInfo.LC_SR_DH[] thisLC_Key)
        {  
            // Populates LC_Key_Orig and LC_Key_New with LC_Key

            int numCodes = 0;

            try {
                numCodes = thisLC_Key.Length;
            }
            catch  {
                numCodes = 0;
                return;
            }

            LC_Key_New = new TopoInfo.LC_SR_DH[numCodes];
            LC_Key_Orig = new TopoInfo.LC_SR_DH[numCodes];
            
            for (int i = 0; i <= numCodes - 1; i++) {
                LC_Key_New[i].code = thisLC_Key[i].code;
                LC_Key_New[i].desc = thisLC_Key[i].desc;
                LC_Key_New[i].SR = thisLC_Key[i].SR;
                LC_Key_New[i].DH = thisLC_Key[i].DH;

                LC_Key_Orig[i].code = thisLC_Key[i].code;
                LC_Key_Orig[i].desc = thisLC_Key[i].desc;
                LC_Key_Orig[i].SR = thisLC_Key[i].SR;
                LC_Key_Orig[i].DH = thisLC_Key[i].DH;
            }
            
        }
        
    }
}
