using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ShortPATH
{
    public partial class Form1 : Form
    {
        private Boolean FirstRun = false;
        private Boolean PATHUpdated = false;

        private string AppTitle = "ShortPATH";
        private string DefaultNewShortcutIdentifier = "new_shortcut";
        private List<string> CommandFileExtensions = new List<string>() { "exe", "bat", "cmd", "msi" };

        private string DirectoryPath;

        private List<Shortcut> shortcuts;

        const int HWND_BROADCAST = 0xffff;
        const uint WM_SETTINGCHANGE = 0x001a;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg,
            UIntPtr wParam, string lParam);

        public Form1()
        {
            InitializeComponent();

            DirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + AppTitle;

            // See if we can access (or create & access) the application directory in AppData
            if (!CheckAppData())
            {
                MessageBox.Show("Failed to load application data directory.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Environment.Exit(0);
            }

            // See if the application directory is in the PATH environment variable (or add & check)
            if (!CheckPATH())
            {
                MessageBox.Show("Failed add shortcut directory to PATH environment variable.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Environment.Exit(0);
            }

            // See if we can load any Shortcuts
            if (!LoadShortcuts() || !LoadShortcutList())
            {
                MessageBox.Show("Failed to load shortcuts.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Environment.Exit(0);
            }
            
            Console.WriteLine(AppTitle + " loaded");
        }

        // Check if we can access (or create & access) the application directory in AppData
        public Boolean CheckAppData()
        {
            try
            {
                // If the application directory exists we're done here
                if (Directory.Exists(DirectoryPath))
                {
                    return true;
                }

                // Create the directory
                DirectoryInfo di = Directory.CreateDirectory(DirectoryPath);

                // No exception means the directory has been created
                Console.WriteLine("Directory created: " + DirectoryPath);

                // This mostly indicates a firstrun so we can show an informative message later
                FirstRun = true;

                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine("The process failed: {0}", exception.ToString());
            }
            
            return false;
        }

        // Check if the application directory is in the PATH environment variable (or add & check)
        public Boolean CheckPATH(Boolean addToPATH = true)
        {
            // Store into CURRENT_USER for easy portability
            string registeryKey = "HKEY_CURRENT_USER\\Environment";
            string registeryValue = "Path";
            
            try
            {

                // Get current paths from registery
                string pathVariable = (string) Registry.GetValue(registeryKey, registeryValue, "");
                string[] paths = pathVariable.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                // A list to remember all current directory paths in the current PATH variable if we must edit it
                List<string> pathList = new List<string>();

                // See if our directory is present
                foreach (string path in paths)
                {
                    pathList.Add(path);

                    if (path == DirectoryPath) {
                        return true;
                    }
                }

                // addToPATH will be false if this is our second try to add the path
                if(addToPATH == false) {
                    return false;
                }

                // Add our directory to the PATH variable list
                pathList.Add(DirectoryPath);

                // Set the new registry value
                pathVariable = String.Join(";", pathList);
                Registry.SetValue(registeryKey, registeryValue, pathVariable);
                Console.WriteLine("PATH updated: " + pathVariable);

                // Signal the environment so the new PATH is used (not accurate)
                SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, (UIntPtr)0, "Environment");

                // Show a nice message to explain things
                MessageBox.Show(
                    "The PATH Environment variable of this user account has been updated. " + 
                    "You will have to sign out and sign in again before changes are reflected. " + 
                    "After that changes to the shortcuts will update instantly.",
                    AppTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                // If the PATH was updated we can make the messagebox when a file is saved context-aware
                PATHUpdated = true;

                // Double check the PATH
                return CheckPATH(false);
            }
            catch (Exception exception)
            {
                Console.WriteLine("The process failed: {0}", exception.ToString());
            }

            return false;
        }

        // Load all Shortcuts into a List
        private Boolean LoadShortcuts()
        {
            shortcuts = new List<Shortcut>();

            DirectoryInfo dir = new DirectoryInfo(DirectoryPath);
            FileInfo[] Files = dir.GetFiles("*.bat");

            foreach (FileInfo file in Files)
            {
                string name = Path.GetFileNameWithoutExtension(file.Name);
                shortcuts.Add(new Shortcut(DirectoryPath, name));
            }

            return true;
        }

        // Load the Shortcut List into the ListBox
        public Boolean LoadShortcutList(string selectedItem = null)
        {
            try
            {
                // Update Data source
                listBox1.DataSource = null;
                listBox1.DataSource = shortcuts;
                listBox1.DisplayMember = "Identifier";
                listBox1.ValueMember = "Folder";

                // Check if we need to select an item
                if(selectedItem == null)
                {
                    listBox1.SelectedIndex = -1;
                } else
                {
                    // Get index in ListBox by string
                    int index = listBox1.FindString(selectedItem);

                    // Select it if it's found
                    if (index != -1)
                    {
                        listBox1.SetSelected(index, true);
                    }
                }

                return true;
            }
            catch (Exception exception) {
                Console.WriteLine("The process failed: {0}", exception.ToString());
            }

            return false;
        }

        // Load the right panel when the listbox updates
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // The index of the listbox matches the index in the list of Shortcuts
            int index = listBox1.SelectedIndex;

            // If nothing selected reset the right panel
            if (index == -1)
            {
                textBox1.Text = "";
                textBox1.Enabled = false;

                textBox2.Text = "";
                textBox2.Enabled = false;

                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;

            } else
            {
                // Load the shortcut into the right panel
                try
                {
                    Shortcut shortcut = shortcuts.ElementAt(index);

                    textBox1.Text = shortcut.Identifier;
                    textBox1.Enabled = true;

                    textBox2.Text = shortcut.Folder;
                    textBox2.Enabled = true;

                    button2.Enabled = true;
                    button3.Enabled = true;
                    button4.Enabled = true;

                } catch (Exception exception){
                    Console.WriteLine("The process failed: {0}", exception.ToString());
                }
                
            }


        }

        // Save
        private void button3_Click(object sender, EventArgs e)
        {
            // Check for filled in fields
            if (textBox1.Text == "" || textBox2.Text == "")
            {
                MessageBox.Show("Both the shortcut identifier and folder should be filled in.",
                    AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Check for a valid filename
            if (!IsValidFilename(textBox1.Text))
            {
                MessageBox.Show("Invalid shortcut identifier. It should be able to be saved as a file",
                    AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Check for a valid directory path
            if (!IsValidDirectoryPath(textBox2.Text))
            {
                MessageBox.Show("Invalid folder path", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // The index of the listbox matches the index in the list of Shortcuts
            int index = listBox1.SelectedIndex;

            if (index != -1)
            {
                // Get the Shortcut from the list and update its properties, temporary Shortcuts are also stored in the list
                Shortcut shortcut = shortcuts.ElementAt(index);

                // If the identifier is new or has changed
                if(shortcut.Identifier != textBox1.Text)
                {
                    // Make sure the Shortcut does not exist yet by checking the ListBox
                    if (listBox1.FindString(textBox1.Text) != -1)
                    {
                        MessageBox.Show("This shortcut identifier is already in use.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    // Check if the identifier is already in use as a command
                    if (CommandExistsInPATH(textBox1.Text))
                    {
                        MessageBox.Show("The command \"" + textBox1.Text + "\" is already in use by the environment.",
                            AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    // Check if the identifier is already in use as a directory
                    string subdirectory_path = "";
                    if (SubdirectoryExistsInPATH(textBox1.Text, out subdirectory_path))
                    {
                        MessageBox.Show("\"" + textBox1.Text + "\" matches directory \"" + subdirectory_path + 
                            "\" and will therefor not work using the Windows Run Command dialog. " + 
                            "The command will still be saved as it will still work from the command line.",
                            AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                }
                
                // Shortcut object properties
                shortcut.Identifier = textBox1.Text;
                shortcut.Folder = textBox2.Text;
                shortcut.SaveFile();

                // Reload everything
                LoadShortcuts();
                LoadShortcutList();

                // On the first run we'll show a messagebox for more information
                if (FirstRun)
                {
                    // When the PATH is updated as well we'll show this context-aware messagebox
                    if (PATHUpdated)
                    {
                        MessageBox.Show(
                            "When you sign out and back in again, you will be able to run commands directly in the chosen folder by running: " +
                            "\"" + shortcut.Identifier + " <command>\". ",
                            AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    } else
                    {
                        MessageBox.Show("You can now run commands directly in the chosen folder by running: " +
                            "\"" + shortcut.Identifier + " <command>\". ",
                            AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    // Next saves shouldn't annoy the user
                    FirstRun = false;
                    PATHUpdated = false;
                }
            }
        }

        // Add
        private void button1_Click(object sender, EventArgs e)
        {
            // Reload shortcuts to remove any non saved shortcuts
            LoadShortcuts();

            // Check for the default new shortcut string and get the next available one if it already exists
            string shortcutIdentifier = DefaultNewShortcutIdentifier;
            if (listBox1.FindString(shortcutIdentifier) != -1)
            {
                int nextNumber = 2;

                // While we've not found the next identifier
                while (shortcutIdentifier == DefaultNewShortcutIdentifier)
                {
                    string nextShortcutIdentifier = DefaultNewShortcutIdentifier + nextNumber.ToString(); 

                    // If the next identifier is not yet in the list we'll leave the while loop by updating the identifier we'll use
                    if(listBox1.FindString(nextShortcutIdentifier) == -1)
                    {
                        shortcutIdentifier = nextShortcutIdentifier;
                    } else
                    {
                        nextNumber++;
                    }
                }
            }

            // Add a shortcut to the list and show it in the right panel. It is not saved yet at this point
            shortcuts.Add(new Shortcut(DirectoryPath, shortcutIdentifier));
            LoadShortcutList(shortcutIdentifier);

            // Select the shortcut text
            textBox1.Select();
        }

        // Remove
        private void button4_Click(object sender, EventArgs e)
        {
            // The index of the listbox matches the index in the list of Shortcuts
            int index = listBox1.SelectedIndex;

            if (index != -1)
            {
                // Select Shortcut and delete it
                Shortcut shortcut = shortcuts.ElementAt(index);
                shortcut.Delete();

                // Reload everything
                LoadShortcuts();
                LoadShortcutList();
            }
        }

        // Browse folder
        private void button2_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textBox2.Text = fbd.SelectedPath;
                }
            }
        }

        // Simulate a Save button click when we press enter in the field
        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button3_Click(this, new EventArgs());
            }
        }

        // Simulate a Save button click when we press enter in the field
        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button3_Click(this, new EventArgs());
            }
        }

        // Check if a string is a valid filename
        bool IsValidFilename(string testName)
        {
            // https://stackoverflow.com/a/62855/5865844
            Regex containsABadCharacter = new Regex("["
                  + Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars())) + "]");
            if (containsABadCharacter.IsMatch(testName)) { return false; };

            return true;
        }

        // Check if a string is a valid directory path
        private bool IsValidDirectoryPath(string path, bool exactPath = true)
        {
            // https://stackoverflow.com/a/48820213/5865844
            bool isValid = true;

            try {
                string fullPath = Path.GetFullPath(path);

                if (exactPath){
                    string root = Path.GetPathRoot(path);
                    isValid = string.IsNullOrEmpty(root.Trim(new char[] { '\\', '/' })) == false;
                } else {
                    isValid = Path.IsPathRooted(path);
                }
            } catch (Exception exception) {

                Console.WriteLine("Valid directory check exception: {0}", exception.ToString());
                isValid = false;
            }

            return isValid;
        }

        // Check if the given string is available to be used as a command
        private bool CommandExistsInPATH(string possibleCommand)
        {
            foreach (string path in GetPATHDirectories())
            {
                string currentDirectoryPath = path.Trim(new[] { '\\', '/' });

                if(currentDirectoryPath != DirectoryPath)
                {
                    foreach (var fileExtension in CommandFileExtensions)
                    {
                        string filePath = currentDirectoryPath + "\\" + possibleCommand + "." + fileExtension;
                        if (File.Exists(filePath))
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }

        // Check if a given string is a subdirectory in the PATH environment variable
        private bool SubdirectoryExistsInPATH(string possibleSubdirectory, out string foundSubdirectory)
        {
            foundSubdirectory = "";

            foreach (string path in GetPATHDirectories())
            {
                string currentDirectoryPath = path.Trim(new[] { '\\', '/' });

                if (currentDirectoryPath != DirectoryPath)
                {
                    string directoryPath = currentDirectoryPath + "\\" + possibleSubdirectory;
                    if (Directory.Exists(directoryPath))
                    {
                        foundSubdirectory = directoryPath;

                        return true;
                    }
                }
            }

            return false;
        }
        
        // Get a list of directories in the PATH environment variable
        private string[] GetPATHDirectories()
        {
            string PATH = System.Environment.GetEnvironmentVariable("PATH");
            string[] directories = PATH.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            return directories;
        }
    }
}
