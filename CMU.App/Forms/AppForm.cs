using CMU.App.Model;
using CredentialManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using DialogResult = System.Windows.Forms.DialogResult;

namespace CMU.App.Forms
{
    public partial class AppForm : Form
    {
        private const string CACHE_FILE_NAME = "_CachedValues.json";
        private ServerListFile _currentServerListFile;
        private Cache _cache;

        public AppForm()
        {
            InitializeComponent();
            ReadCache();
        }

        private void ReadCache()
        {
            var cacheFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CACHE_FILE_NAME);

            if ( File.Exists(cacheFilePath) )
                _cache = JsonConvert.DeserializeObject<Cache>( File.ReadAllText(cacheFilePath) );
            else
                _cache = new Cache();
        } // ReadCache

        private void StoreCache()
        {
            _cache.Domain = txtDomain.Text;
            _cache.Username = txtUsername.Text;
            _cache.ServerFilePath = txtServerFile.Text;

            var json = JsonConvert.SerializeObject(_cache, Formatting.Indented);
            var cacheFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CACHE_FILE_NAME);
            File.WriteAllText(cacheFilePath, json, Encoding.UTF8);
        } // StoreCache

        private void LoadFileServerList(string path)
        {
            _currentServerListFile = JsonConvert.DeserializeObject<ServerListFile>( File.ReadAllText(path) );
            gridView.DataSource = _currentServerListFile.ServerList;
            gridView.Refresh();
        } // LoadFileServerList

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        } // btnCancel_Click

        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            using (var openFileDiag = new OpenFileDialog())
            {
                openFileDiag.Filter = "Server List File (*.json)|*.json";
                openFileDiag.Title = "Load server list file";
                openFileDiag.Multiselect = false;

                if (openFileDiag.ShowDialog() == DialogResult.OK)
                {
                    txtServerFile.Text = openFileDiag.FileName;
                    LoadFileServerList(txtServerFile.Text);
                }
            }
        } // btnBrowseFile_Click

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if ( string.IsNullOrEmpty(txtDomain.Text) )
            {
                MessageBox.Show("Domain is required", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDomain.Focus();
                return;
            }

            if ( string.IsNullOrEmpty(txtUsername.Text) )
            {
                MessageBox.Show("Username is required", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if ( string.IsNullOrEmpty(txtPass.Text) )
            {
                MessageBox.Show("Password is required", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPass.Focus();
                return;
            }

            if ( string.IsNullOrEmpty(txtPassConfirm.Text) )
            {
                MessageBox.Show("Password is required", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassConfirm.Focus();
                return;
            }

            if ( txtPass.Text != txtPassConfirm.Text)
            {
                MessageBox.Show("Password and Password Confirmation are different", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassConfirm.Focus();
                txtPassConfirm.SelectAll();
                return;
            }

            if ( _currentServerListFile == null )
            {
                MessageBox.Show("Please select a Server File List first", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if ( _currentServerListFile.ServerList.Count <= 0 )
            {
                MessageBox.Show("Server File List contains no data", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if ( MessageBox.Show(
                    "You are about to update the credentials on all the servers on the list. Are you sure you want to continue?",
                    "Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.No )
            {
                return;
            }

            StoreCache();

            Enabled = false;
            Cursor = Cursors.WaitCursor;
            UpdateServersOnWindowsCredentialsManager();
            Cursor = Cursors.Default;
            Enabled = true;

            MessageBox.Show("The servers have been updated successfully on Windows Credentials Manager", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        } // btnUpdate_Click

        private void UpdateServersOnWindowsCredentialsManager()
        {
            foreach (var server in _currentServerListFile.ServerList)
            {
                var securePass = new NetworkCredential("", txtPass.Text.Trim()).SecurePassword;
                
                var credentials =
                    new Credential
                    {
                        Target = server.Server,
                        Username = $@"{txtDomain.Text.Trim()}\{txtUsername.Text.Trim()}",
                        SecurePassword = securePass,
                        Type = CredentialType.DomainPassword,
                        PersistanceType = PersistanceType.Enterprise
                    };

                credentials.Delete();
                credentials.Save();
            }            
        } // UpdateServersOnWindowsCredentialsManager

        private void AppForm_Load(object sender, EventArgs e)
        {
            this.txtDomain.Text = _cache.Domain;
            this.txtUsername.Text = _cache.Username;

            if ( File.Exists(_cache.ServerFilePath) )
            {
                txtServerFile.Text = _cache.ServerFilePath;
                LoadFileServerList(txtServerFile.Text);
            }
        } // AppForm_Load
    }
}
